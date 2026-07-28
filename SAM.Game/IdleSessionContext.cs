/* Copyright (c) 2026 Rafael
 *
 * Background session used by the batch launcher. Steam associates a process
 * with one app ID, so a process is still required for each idled game; this
 * context intentionally creates no window or taskbar entry.
 */

using System;
using System.Threading;
using System.Windows.Forms;

namespace SAM.Game
{
    internal sealed class IdleSessionContext : ApplicationContext
    {
        private readonly API.Client _SteamClient;
        private readonly System.Windows.Forms.Timer _CallbackTimer;
        private readonly EventWaitHandle _StopEvent;

        public IdleSessionContext(API.Client client, string stopEventName)
        {
            this._SteamClient = client;
            if (string.IsNullOrEmpty(stopEventName) == false)
            {
                try
                {
                    this._StopEvent = EventWaitHandle.OpenExisting(stopEventName);
                }
                catch (WaitHandleCannotBeOpenedException)
                {
                    // A manually started --idle session has no controller.
                }
            }

            this._CallbackTimer = new System.Windows.Forms.Timer
            {
                Interval = 1000,
                Enabled = true,
            };
            this._CallbackTimer.Tick += this.OnCallbackTimer;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing == true)
            {
                this._CallbackTimer.Dispose();
                this._StopEvent?.Dispose();
            }

            base.Dispose(disposing);
        }

        private void OnCallbackTimer(object sender, EventArgs e)
        {
            if (this._StopEvent?.WaitOne(0) == true)
            {
                this.ExitThread();
                return;
            }

            this._SteamClient.RunCallbacks(false);
        }
    }
}
