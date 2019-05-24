/* Copyright (c) 2019 Rick (rick 'at' gibbed 'dot' us)
 * 
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 * 
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 * 
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 * 
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 * 
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SAM.API
{
    public class Client : IDisposable
    {
        public Wrappers.SteamClient018 SteamClient;
        public Wrappers.SteamUser012 SteamUser;
        public Wrappers.SteamUserStats007 SteamUserStats;
        public Wrappers.SteamUtils005 SteamUtils;
        public Wrappers.SteamApps001 SteamApps001;
        public Wrappers.SteamApps008 SteamApps008;

        private bool _IsDisposed = false;
        private int _Pipe;
        private int _User;

        private readonly List<ICallback> _Callbacks = new List<ICallback>();

        public void Initialize(long appId)
        {
            if (string.IsNullOrEmpty(Steam.GetInstallPath()) == true)
            {
                throw new ClientInitializeException(ClientInitializeFailure.GetInstallPath, "failed to get Steam install path");
            }

            if (appId != 0)
            {
                Environment.SetEnvironmentVariable("SteamAppId", appId.ToString(CultureInfo.InvariantCulture));
            }

            if (Steam.Load() == false)
            {
                throw new ClientInitializeException(ClientInitializeFailure.Load, "failed to load SteamClient");
            }

            this.SteamClient = Steam.CreateInterface<Wrappers.SteamClient018>("SteamClient018");
            if (this.SteamClient == null)
            {
                throw new ClientInitializeException(ClientInitializeFailure.CreateSteamClient, "failed to create ISteamClient018");
            }

            this._Pipe = this.SteamClient.CreateSteamPipe();
            if (this._Pipe == 0)
            {
                throw new ClientInitializeException(ClientInitializeFailure.CreateSteamPipe, "failed to create pipe");
            }

            this._User = this.SteamClient.ConnectToGlobalUser(this._Pipe);
            if (this._User == 0)
            {
                throw new ClientInitializeException(ClientInitializeFailure.ConnectToGlobalUser, "failed to connect to global user");
            }

            this.SteamUtils = this.SteamClient.GetSteamUtils004(this._Pipe);
            if (appId > 0 && this.SteamUtils.GetAppId() != (uint)appId)
            {
                throw new ClientInitializeException(ClientInitializeFailure.AppIdMismatch, "appID mismatch");
            }

            this.SteamUser = this.SteamClient.GetSteamUser012(this._User, this._Pipe);
            this.SteamUserStats = this.SteamClient.GetSteamUserStats006(this._User, this._Pipe);
            this.SteamApps001 = this.SteamClient.GetSteamApps001(this._User, this._Pipe);
            this.SteamApps008 = this.SteamClient.GetSteamApps008(this._User, this._Pipe);
        }

        ~Client()
        {
            this.Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this._IsDisposed == true)
            {
                return;
            }

            if (this.SteamClient != null && this._Pipe > 0)
            {
                if (this._User > 0)
                {
                    this.SteamClient.ReleaseUser(this._Pipe, this._User);
                    this._User = 0;
                }

                this.SteamClient.ReleaseSteamPipe(this._Pipe);
                this._Pipe = 0;
            }

            this._IsDisposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public TCallback CreateAndRegisterCallback<TCallback>()
            where TCallback : ICallback, new()
        {
            var callback = new TCallback();
            this._Callbacks.Add(callback);
            return callback;
        }

        private bool _RunningCallbacks;

        public void RunCallbacks(bool server)
        {
            if (this._RunningCallbacks == true)
            {
                return;
            }

            this._RunningCallbacks = true;

            Types.CallbackMessage message;
            int call;
            while (Steam.GetCallback(this._Pipe, out message, out call) == true)
            {
                var callbackId = message.Id;
                foreach (ICallback callback in this._Callbacks.Where(
                    candidate => candidate.Id == callbackId &&
                                 candidate.IsServer == server))
                {
                    callback.Run(message.ParamPointer);
                }

                Steam.FreeLastCallback(this._Pipe);
            }

            this._RunningCallbacks = false;
        }
    }
}
