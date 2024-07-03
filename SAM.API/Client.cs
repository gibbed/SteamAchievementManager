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

        private readonly List<ICallback> _Callbacks = [];

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

            SteamClient = Steam.CreateInterface<Wrappers.SteamClient018>("SteamClient018");
            if (SteamClient == null)
            {
                throw new ClientInitializeException(ClientInitializeFailure.CreateSteamClient, "failed to create ISteamClient018");
            }

            _Pipe = SteamClient.CreateSteamPipe();
            if (_Pipe == 0)
            {
                throw new ClientInitializeException(ClientInitializeFailure.CreateSteamPipe, "failed to create pipe");
            }

            _User = SteamClient.ConnectToGlobalUser(_Pipe);
            if (_User == 0)
            {
                throw new ClientInitializeException(ClientInitializeFailure.ConnectToGlobalUser, "failed to connect to global user");
            }

            SteamUtils = SteamClient.GetSteamUtils004(_Pipe);
            if (appId > 0 && SteamUtils.GetAppId() != (uint)appId)
            {
                throw new ClientInitializeException(ClientInitializeFailure.AppIdMismatch, "appID mismatch");
            }

            SteamUser = SteamClient.GetSteamUser012(_User, _Pipe);
            SteamUserStats = SteamClient.GetSteamUserStats006(_User, _Pipe);
            SteamApps001 = SteamClient.GetSteamApps001(_User, _Pipe);
            SteamApps008 = SteamClient.GetSteamApps008(_User, _Pipe);
        }

        ~Client()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_IsDisposed == true)
            {
                return;
            }

            if (SteamClient != null && _Pipe > 0)
            {
                if (_User > 0)
                {
                    SteamClient.ReleaseUser(_Pipe, _User);
                    _User = 0;
                }

                SteamClient.ReleaseSteamPipe(_Pipe);
                _Pipe = 0;
            }

            _IsDisposed = true;
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
            _Callbacks.Add(callback);
            return callback;
        }

        private bool _RunningCallbacks;

        public void RunCallbacks(bool server)
        {
            if (_RunningCallbacks == true)
            {
                return;
            }

            _RunningCallbacks = true;

            while (Steam.GetCallback(_Pipe, out Types.CallbackMessage message, out int call) == true)
            {
                var callbackId = message.Id;
                foreach (ICallback callback in _Callbacks.Where(
                    candidate => candidate.Id == callbackId &&
                                 candidate.IsServer == server))
                {
                    callback.Run(message.ParamPointer);
                }

                Steam.FreeLastCallback(_Pipe);
            }

            _RunningCallbacks = false;
        }
    }
}