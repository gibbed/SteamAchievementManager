namespace SAM.Picker
{
    partial class GamePicker
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this._LogoWorker != null && this._LogoWorker.IsBusy)
                {
                    this._LogoWorker.CancelAsync();
                    while (this._LogoWorker.IsBusy)
                    {
                        System.Windows.Forms.Application.DoEvents();
                    }
                }

                if (this._ListWorker != null && this._ListWorker.IsBusy)
                {
                    this._ListWorker.CancelAsync();
                    while (this._ListWorker.IsBusy)
                    {
                        System.Windows.Forms.Application.DoEvents();
                    }
                }

                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.ToolStripSeparator _ToolStripSeparator1;
            System.Windows.Forms.ToolStripSeparator _ToolStripSeparator2;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GamePicker));
            this._LogoImageList = new System.Windows.Forms.ImageList(this.components);
            this._CallbackTimer = new System.Windows.Forms.Timer(this.components);
            this._PickerToolStrip = new System.Windows.Forms.ToolStrip();
            this._RefreshGamesButton = new System.Windows.Forms.ToolStripButton();
            this._AddGameTextBox = new System.Windows.Forms.ToolStripTextBox();
            this._AddGameButton = new System.Windows.Forms.ToolStripButton();
            this._FindGamesLabel = new System.Windows.Forms.ToolStripLabel();
            this._SearchGameTextBox = new System.Windows.Forms.ToolStripTextBox();
            this._FilterDropDownButton = new System.Windows.Forms.ToolStripDropDownButton();
            this._FilterGamesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._FilterDemosMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._FilterModsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._FilterJunkMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._LanguageLabel = new System.Windows.Forms.ToolStripLabel();
            this._LanguageComboBox = new System.Windows.Forms.ToolStripComboBox();
            this._PickerStatusStrip = new System.Windows.Forms.StatusStrip();
            this._PickerStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this._DownloadStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this._LogoWorker = new System.ComponentModel.BackgroundWorker();
            this._ListWorker = new System.ComponentModel.BackgroundWorker();
            this._GameListView = new SAM.Picker.MyListView();
            _ToolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            _ToolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this._PickerToolStrip.SuspendLayout();
            this._PickerStatusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // _ToolStripSeparator1
            // 
            _ToolStripSeparator1.Name = "_ToolStripSeparator1";
            _ToolStripSeparator1.Size = new System.Drawing.Size(6, 45);
            // 
            // _ToolStripSeparator2
            // 
            _ToolStripSeparator2.Name = "_ToolStripSeparator2";
            _ToolStripSeparator2.Size = new System.Drawing.Size(6, 45);
            // 
            // _LogoImageList
            // 
            this._LogoImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth24Bit;
            this._LogoImageList.ImageSize = new System.Drawing.Size(184, 69);
            this._LogoImageList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // _CallbackTimer
            // 
            this._CallbackTimer.Enabled = true;
            this._CallbackTimer.Tick += new System.EventHandler(this.OnTimer);
            // 
            // _PickerToolStrip
            // 
            this._PickerToolStrip.Font = new System.Drawing.Font("Microsoft JhengHei UI", 11.14286F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this._PickerToolStrip.ImageScalingSize = new System.Drawing.Size(28, 28);
            this._PickerToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._RefreshGamesButton,
            _ToolStripSeparator1,
            this._AddGameTextBox,
            this._AddGameButton,
            _ToolStripSeparator2,
            this._FindGamesLabel,
            this._SearchGameTextBox,
            this._FilterDropDownButton,
            this._LanguageLabel,
            this._LanguageComboBox});
            this._PickerToolStrip.Location = new System.Drawing.Point(0, 0);
            this._PickerToolStrip.Name = "_PickerToolStrip";
            this._PickerToolStrip.Padding = new System.Windows.Forms.Padding(0, 0, 5, 0);
            this._PickerToolStrip.Size = new System.Drawing.Size(1633, 45);
            this._PickerToolStrip.TabIndex = 1;
            this._PickerToolStrip.Text = "toolStrip1";
            // 
            // _RefreshGamesButton
            // 
            this._RefreshGamesButton.Image = global::SAM.Picker.Resources.Refresh;
            this._RefreshGamesButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._RefreshGamesButton.Name = "_RefreshGamesButton";
            this._RefreshGamesButton.Size = new System.Drawing.Size(235, 39);
            this._RefreshGamesButton.Text = "Refresh Games";
            this._RefreshGamesButton.Click += new System.EventHandler(this.OnRefresh);
            // 
            // _AddGameTextBox
            // 
            this._AddGameTextBox.Font = new System.Drawing.Font("Microsoft JhengHei UI", 11.14286F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this._AddGameTextBox.Name = "_AddGameTextBox";
            this._AddGameTextBox.Size = new System.Drawing.Size(228, 45);
            // 
            // _AddGameButton
            // 
            this._AddGameButton.Image = global::SAM.Picker.Resources.Search;
            this._AddGameButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._AddGameButton.Name = "_AddGameButton";
            this._AddGameButton.Size = new System.Drawing.Size(182, 39);
            this._AddGameButton.Text = "Add Game";
            this._AddGameButton.Click += new System.EventHandler(this.OnAddGame);
            // 
            // _FindGamesLabel
            // 
            this._FindGamesLabel.Name = "_FindGamesLabel";
            this._FindGamesLabel.Size = new System.Drawing.Size(78, 39);
            this._FindGamesLabel.Text = "Filter";
            // 
            // _SearchGameTextBox
            // 
            this._SearchGameTextBox.Font = new System.Drawing.Font("Microsoft JhengHei UI", 11.14286F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this._SearchGameTextBox.Name = "_SearchGameTextBox";
            this._SearchGameTextBox.Size = new System.Drawing.Size(228, 45);
            this._SearchGameTextBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.OnFilterUpdate);
            // 
            // _FilterDropDownButton
            // 
            this._FilterDropDownButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._FilterDropDownButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._FilterGamesMenuItem,
            this._FilterDemosMenuItem,
            this._FilterModsMenuItem,
            this._FilterJunkMenuItem});
            this._FilterDropDownButton.Image = global::SAM.Picker.Resources.Filter;
            this._FilterDropDownButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._FilterDropDownButton.Name = "_FilterDropDownButton";
            this._FilterDropDownButton.Size = new System.Drawing.Size(49, 39);
            this._FilterDropDownButton.Text = "Game filtering";
            // 
            // _FilterGamesMenuItem
            // 
            this._FilterGamesMenuItem.Checked = true;
            this._FilterGamesMenuItem.CheckOnClick = true;
            this._FilterGamesMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this._FilterGamesMenuItem.Name = "_FilterGamesMenuItem";
            this._FilterGamesMenuItem.Size = new System.Drawing.Size(297, 44);
            this._FilterGamesMenuItem.Text = "Show &games";
            this._FilterGamesMenuItem.CheckedChanged += new System.EventHandler(this.OnFilterUpdate);
            // 
            // _FilterDemosMenuItem
            // 
            this._FilterDemosMenuItem.CheckOnClick = true;
            this._FilterDemosMenuItem.Name = "_FilterDemosMenuItem";
            this._FilterDemosMenuItem.Size = new System.Drawing.Size(297, 44);
            this._FilterDemosMenuItem.Text = "Show &demos";
            this._FilterDemosMenuItem.CheckedChanged += new System.EventHandler(this.OnFilterUpdate);
            // 
            // _FilterModsMenuItem
            // 
            this._FilterModsMenuItem.CheckOnClick = true;
            this._FilterModsMenuItem.Name = "_FilterModsMenuItem";
            this._FilterModsMenuItem.Size = new System.Drawing.Size(297, 44);
            this._FilterModsMenuItem.Text = "Show &mods";
            this._FilterModsMenuItem.CheckedChanged += new System.EventHandler(this.OnFilterUpdate);
            // 
            // _FilterJunkMenuItem
            // 
            this._FilterJunkMenuItem.CheckOnClick = true;
            this._FilterJunkMenuItem.Name = "_FilterJunkMenuItem";
            this._FilterJunkMenuItem.Size = new System.Drawing.Size(297, 44);
            this._FilterJunkMenuItem.Text = "Show &junk";
            this._FilterJunkMenuItem.CheckedChanged += new System.EventHandler(this.OnFilterUpdate);
            // 
            // _LanguageLabel
            // 
            this._LanguageLabel.Name = "_LanguageLabel";
            this._LanguageLabel.Size = new System.Drawing.Size(140, 39);
            this._LanguageLabel.Text = "Language";
            this._LanguageLabel.Visible = false;
            // 
            // _LanguageComboBox
            // 
            this._LanguageComboBox.Font = new System.Drawing.Font("Microsoft JhengHei UI", 11.14286F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this._LanguageComboBox.Items.AddRange(new object[] {
            "arabic",
            "brazilian",
            "bulgarian",
            "czech",
            "danish",
            "dutch",
            "english",
            "finnish",
            "french",
            "german",
            "greek",
            "hungarian",
            "indonesian",
            "italian",
            "japanese",
            "koreana",
            "latam",
            "norwegian",
            "polish",
            "portuguese",
            "romanian",
            "russian",
            "schinese",
            "spanish",
            "swedish",
            "tchinese",
            "th",
            "turkish",
            "ukrainian",
            "vietnamese"});
            this._LanguageComboBox.Name = "_LanguageComboBox";
            this._LanguageComboBox.Size = new System.Drawing.Size(160, 45);
            this._LanguageComboBox.Visible = false;
            // 
            // _PickerStatusStrip
            // 
            this._PickerStatusStrip.ImageScalingSize = new System.Drawing.Size(28, 28);
            this._PickerStatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._PickerStatusLabel,
            this._DownloadStatusLabel});
            this._PickerStatusStrip.Location = new System.Drawing.Point(0, 618);
            this._PickerStatusStrip.Name = "_PickerStatusStrip";
            this._PickerStatusStrip.Padding = new System.Windows.Forms.Padding(3, 0, 33, 0);
            this._PickerStatusStrip.Size = new System.Drawing.Size(1633, 22);
            this._PickerStatusStrip.TabIndex = 2;
            this._PickerStatusStrip.Text = "statusStrip";
            // 
            // _PickerStatusLabel
            // 
            this._PickerStatusLabel.Name = "_PickerStatusLabel";
            this._PickerStatusLabel.Size = new System.Drawing.Size(1597, 13);
            this._PickerStatusLabel.Spring = true;
            this._PickerStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _DownloadStatusLabel
            // 
            this._DownloadStatusLabel.Image = global::SAM.Picker.Resources.Download;
            this._DownloadStatusLabel.Name = "_DownloadStatusLabel";
            this._DownloadStatusLabel.Size = new System.Drawing.Size(207, 28);
            this._DownloadStatusLabel.Text = "Download status";
            this._DownloadStatusLabel.Visible = false;
            // 
            // _LogoWorker
            // 
            this._LogoWorker.WorkerSupportsCancellation = true;
            this._LogoWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.DoDownloadLogo);
            this._LogoWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.OnDownloadLogo);
            this.components.Add(this._LogoWorker);
            // 
            // _ListWorker
            // 
            this._ListWorker.WorkerSupportsCancellation = true;
            this._ListWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.DoDownloadList);
            this._ListWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.OnDownloadList);
            this.components.Add(this._ListWorker);
            // 
            // _GameListView
            // 
            this._GameListView.BackColor = System.Drawing.Color.Black;
            this._GameListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this._GameListView.ForeColor = System.Drawing.Color.White;
            this._GameListView.HideSelection = false;
            this._GameListView.LargeImageList = this._LogoImageList;
            this._GameListView.Location = new System.Drawing.Point(0, 45);
            this._GameListView.Margin = new System.Windows.Forms.Padding(8, 6, 8, 6);
            this._GameListView.MultiSelect = false;
            this._GameListView.Name = "_GameListView";
            this._GameListView.OwnerDraw = true;
            this._GameListView.Size = new System.Drawing.Size(1633, 573);
            this._GameListView.SmallImageList = this._LogoImageList;
            this._GameListView.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this._GameListView.TabIndex = 0;
            this._GameListView.TileSize = new System.Drawing.Size(184, 69);
            this._GameListView.UseCompatibleStateImageBehavior = false;
            this._GameListView.VirtualMode = true;
            this._GameListView.DrawItem += new System.Windows.Forms.DrawListViewItemEventHandler(this.OnGameListViewDrawItem);
            this._GameListView.ItemActivate += new System.EventHandler(this.OnActivateGame);
            this._GameListView.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.OnGameListViewRetrieveVirtualItem);
            this._GameListView.SearchForVirtualItem += new System.Windows.Forms.SearchForVirtualItemEventHandler(this.OnGameListViewSearchForVirtualItem);
            // 
            // GamePicker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(168F, 168F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1633, 640);
            this.Controls.Add(this._GameListView);
            this.Controls.Add(this._PickerStatusStrip);
            this.Controls.Add(this._PickerToolStrip);
            this.Font = new System.Drawing.Font("新細明體", 11.14286F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(8, 6, 8, 6);
            this.Name = "GamePicker";
            this.Text = "Steam Achievement ManagerX 7.0 | Pick a game... Any game...";
            this._PickerToolStrip.ResumeLayout(false);
            this._PickerToolStrip.PerformLayout();
            this._PickerStatusStrip.ResumeLayout(false);
            this._PickerStatusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private MyListView _GameListView;
        private System.Windows.Forms.ImageList _LogoImageList;
        private System.Windows.Forms.Timer _CallbackTimer;
        private System.Windows.Forms.ToolStrip _PickerToolStrip;
        private System.Windows.Forms.ToolStripButton _RefreshGamesButton;
        private System.Windows.Forms.ToolStripTextBox _AddGameTextBox;
        private System.Windows.Forms.ToolStripButton _AddGameButton;
        private System.Windows.Forms.ToolStripDropDownButton _FilterDropDownButton;
        private System.Windows.Forms.ToolStripMenuItem _FilterGamesMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _FilterJunkMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _FilterDemosMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _FilterModsMenuItem;
        private System.Windows.Forms.StatusStrip _PickerStatusStrip;
        private System.Windows.Forms.ToolStripStatusLabel _DownloadStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel _PickerStatusLabel;
        private System.ComponentModel.BackgroundWorker _LogoWorker;
        private System.ComponentModel.BackgroundWorker _ListWorker;
        private System.Windows.Forms.ToolStripTextBox _SearchGameTextBox;
        private System.Windows.Forms.ToolStripLabel _FindGamesLabel;

        #endregion

        private System.Windows.Forms.ToolStripLabel _LanguageLabel;
        private System.Windows.Forms.ToolStripComboBox _LanguageComboBox;
    }
}
