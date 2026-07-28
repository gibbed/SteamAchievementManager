/* Copyright (c) 2026 Rafael
 *
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 */

using System.Drawing;
using System.Windows.Forms;

namespace SAM.Picker
{
    internal enum PickerCloseAction
    {
        Cancel,
        MinimizeToTray,
        StopAndExit,
    }

    internal sealed class ActiveSessionsCloseDialog : Form
    {
        public PickerCloseAction SelectedAction { get; private set; } = PickerCloseAction.Cancel;

        private ActiveSessionsCloseDialog(int running)
        {
            this.Text = "Running idle sessions";
            this.ClientSize = new Size(555, 165);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.CenterParent;

            var warningIcon = new PictureBox
            {
                Image = SystemIcons.Warning.ToBitmap(),
                Location = new Point(20, 22),
                Size = new Size(40, 40),
                SizeMode = PictureBoxSizeMode.CenterImage,
            };

            var message = new Label
            {
                AutoSize = false,
                Location = new Point(75, 18),
                Size = new Size(455, 70),
                Text =
                    $"There {(running == 1 ? "is" : "are")} {running} idle game " +
                    $"session{(running == 1 ? "" : "s")} still running.\r\n\r\n" +
                    "Choose whether to keep them running in the system tray or stop them and exit.",
            };

            var minimizeButton = this.CreateActionButton(
                "Minimize to tray",
                new Point(76, 112),
                PickerCloseAction.MinimizeToTray);
            var exitButton = this.CreateActionButton(
                "Stop sessions and exit",
                new Point(222, 112),
                PickerCloseAction.StopAndExit);
            exitButton.Width = 160;
            var cancelButton = this.CreateActionButton(
                "Cancel",
                new Point(398, 112),
                PickerCloseAction.Cancel);

            this.AcceptButton = minimizeButton;
            this.CancelButton = cancelButton;
            this.Controls.AddRange(new Control[]
            {
                warningIcon,
                message,
                minimizeButton,
                exitButton,
                cancelButton,
            });
        }

        public static PickerCloseAction Show(IWin32Window owner, int running)
        {
            using (var dialog = new ActiveSessionsCloseDialog(running))
            {
                dialog.ShowDialog(owner);
                return dialog.SelectedAction;
            }
        }

        private Button CreateActionButton(string text, Point location, PickerCloseAction action)
        {
            var button = new Button
            {
                Location = location,
                Size = new Size(130, 30),
                Text = text,
                UseVisualStyleBackColor = true,
            };
            button.Click += (sender, args) =>
            {
                this.SelectedAction = action;
                this.DialogResult = DialogResult.OK;
                this.Close();
            };
            return button;
        }
    }
}
