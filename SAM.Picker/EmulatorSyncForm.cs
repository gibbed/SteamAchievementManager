using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using SAM.API;

namespace SAM.Picker
{
    internal partial class EmulatorSyncForm : Form
    {
        private readonly List<EmulatorGameInfo> _games;
        private readonly Client _steamClient;
n        public EmulatorSyncForm(Client steamClient)
        {
            this._steamClient = steamClient;
            this._games = EmulatorScanner.ScanDefaultPaths().ToList();
            InitializeComponent();
            PopulateList();
        }

        private void InitializeComponent()
        {
            this.Text = "Emulator achievements sync";
            this.Width = 700;
            this.Height = 400;
            this.StartPosition = FormStartPosition.CenterParent;
n            var list = new ListView();
            list.Name = "_GameList";
            list.View = View.Details;
            list.FullRowSelect = true;
            list.Dock = DockStyle.Top;
            list.Height = 300;
            list.Columns.Add("Name", 250);
            list.Columns.Add("AppId", 80);
            list.Columns.Add("Emulator", 120);
            list.Columns.Add("Unlocked", 80);
            list.DoubleClick += OnDoubleClickSync;
            this.Controls.Add(list);
n            var syncButton = new Button();
            syncButton.Text = "Sincronizar seleccionado";
            syncButton.Dock = DockStyle.Bottom;
            syncButton.Height = 30;
            syncButton.Click += (s, e) => SyncSelected();
            this.Controls.Add(syncButton);
        }

        private void PopulateList()
        {
            var list = (ListView)this.Controls.Find("_GameList", true).FirstOrDefault();
            if (list == null) return;
            list.Items.Clear();
            foreach (var g in _games)
            {
                var unlocked = g.Achievements?.Values.Count(v => v) ?? 0;
                var idStr = g.AppId == 0 ? "N/A" : g.AppId.ToString();
                var it = new ListViewItem(new[] { g.Name, idStr, g.Emulator, unlocked.ToString() });
                it.Tag = g;
                list.Items.Add(it);
            }
        }

        private void OnDoubleClickSync(object sender, EventArgs e)
        {
            SyncSelected();
        }

        private void SyncSelected()
        {
            var list = (ListView)this.Controls.Find("_GameList", true).FirstOrDefault();
            if (list == null || list.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "Seleccione un juego.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
n            var sel = list.SelectedItems[0];
            var game = (EmulatorGameInfo)sel.Tag;
            if (game.AppId == 0)
            {
                MessageBox.Show(this, "No se pudo determinar el AppID para este juego.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
n            try
            {
                using (var client = new Client())
                {
                    client.Initialize(game.AppId);
                    int setCount = 0;
                    foreach (var kv in game.Achievements)
                    {
                        if (kv.Value == true)
                        {
                            // attempt to set achievement by name (best-effort)
                            var ok = client.SteamUserStats.SetAchievement(kv.Key, true);
                            if (ok) setCount++;
                        }
                    }
                    client.SteamUserStats.StoreStats();
                    MessageBox.Show(this, $"Sincronizados {setCount} logros para AppID {game.AppId}.", "Completado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
