/* Copyright (c) 2026 Rafael
 *
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SAM.Picker
{
    internal sealed class RunningSessionsForm : Form
    {
        private readonly Func<IReadOnlyList<RunningSessionInfo>> _GetSessions;
        private readonly ListView _SessionListView;
        private readonly Label _EmptyLabel;
        private readonly Timer _RefreshTimer;

        public RunningSessionsForm(Func<IReadOnlyList<RunningSessionInfo>> getSessions)
        {
            this._GetSessions = getSessions;

            this.Text = "Steam Achievement Manager | Running sessions";
            this.ClientSize = new Size(710, 320);
            this.MinimumSize = new Size(580, 240);
            this.StartPosition = FormStartPosition.CenterParent;

            this._SessionListView = new ListView
            {
                Dock = DockStyle.Fill,
                FullRowSelect = true,
                GridLines = true,
                HideSelection = false,
                View = View.Details,
            };
            this._SessionListView.Columns.Add("Game", 315);
            this._SessionListView.Columns.Add("App ID", 85);
            this._SessionListView.Columns.Add("Started", 145);
            this._SessionListView.Columns.Add("Elapsed", 125);

            this._EmptyLabel = new Label
            {
                BackColor = SystemColors.Window,
                Dock = DockStyle.Fill,
                Font = new Font(this.Font, FontStyle.Italic),
                ForeColor = SystemColors.GrayText,
                Text = "No idle game sessions are currently running.",
                TextAlign = ContentAlignment.MiddleCenter,
            };

            this.Controls.Add(this._SessionListView);
            this.Controls.Add(this._EmptyLabel);

            this._RefreshTimer = new Timer
            {
                Enabled = true,
                Interval = 1000,
            };
            this._RefreshTimer.Tick += this.OnRefreshTimer;
            this.FormClosed += this.OnFormClosed;

            this.RefreshSessions();
        }

        private void OnRefreshTimer(object sender, EventArgs e)
        {
            this.RefreshSessions();
        }

        private void OnFormClosed(object sender, FormClosedEventArgs e)
        {
            this._RefreshTimer.Dispose();
        }

        private void RefreshSessions()
        {
            var sessions = this._GetSessions();

            this._SessionListView.BeginUpdate();
            try
            {
                this._SessionListView.Items.Clear();
                foreach (var session in sessions)
                {
                    var item = new ListViewItem(session.GameName);
                    item.SubItems.Add(session.GameId.ToString());
                    item.SubItems.Add(session.StartedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss"));
                    item.SubItems.Add(FormatElapsed(DateTimeOffset.UtcNow - session.StartedAt));
                    this._SessionListView.Items.Add(item);
                }
            }
            finally
            {
                this._SessionListView.EndUpdate();
            }

            this._EmptyLabel.Visible = sessions.Count == 0;
            this.Text = $"Steam Achievement Manager | Running sessions ({sessions.Count})";
        }

        private static string FormatElapsed(TimeSpan elapsed)
        {
            return elapsed.Days > 0
                ? $"{elapsed.Days}d {elapsed.ToString(@"hh\:mm\:ss")}"
                : elapsed.ToString(@"hh\:mm\:ss");
        }
    }
}
