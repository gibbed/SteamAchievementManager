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
			if (disposing && (components != null))
			{
				components.Dispose();
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GamePicker));
            this._LogoImageList = new System.Windows.Forms.ImageList(this.components);
            this._CallbackTimer = new System.Windows.Forms.Timer(this.components);
            this._PickerToolStrip = new System.Windows.Forms.ToolStrip();
            this._RefreshGamesButton = new System.Windows.Forms.ToolStripButton();
            this._AddGameTextBox = new System.Windows.Forms.ToolStripTextBox();
            this._AddGameButton = new System.Windows.Forms.ToolStripButton();
            this._FilterDropDownButton = new System.Windows.Forms.ToolStripDropDownButton();
            this._FilterGamesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._FilterDemosMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._FilterModsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._FilterJunkMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._GameListView = new SAM.Picker.DoubleBufferedListView();
            this._PickerStatusStrip = new System.Windows.Forms.StatusStrip();
            this._PickerStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this._DownloadStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this._LogoWorker = new System.ComponentModel.BackgroundWorker();
            this._ListWorker = new System.ComponentModel.BackgroundWorker();
            this._ToolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this._ToolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this._ToolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this._FindGamesLabel = new System.Windows.Forms.ToolStripLabel();
            this._SearchGameTextBox = new System.Windows.Forms.ToolStripTextBox();
            this._PickerToolStrip.SuspendLayout();
            this._PickerStatusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // _ToolStripSeparator1
            // 
            this._ToolStripSeparator1.Name = "_ToolStripSeparator1";
            this._ToolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // _ToolStripSeparator2
            // 
            _ToolStripSeparator2.Name = "_ToolStripSeparator2";
            _ToolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // _ToolStripSeparator3
            // 
            this._ToolStripSeparator3.Name = "_ToolStripSeparator3";
            this._ToolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // _FindGamesLabel
            // 
            this._FindGamesLabel.Name = "_FindGamesLabel";
            this._FindGamesLabel.Size = new System.Drawing.Size(33, 22);
            this._FindGamesLabel.Text = "Filter";
            // 
            // _SearchGameTextBox
            // 
            this._SearchGameTextBox.Name = "_SearchGameTextBox";
            this._SearchGameTextBox.Size = new System.Drawing.Size(100, 25);
            this._SearchGameTextBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.OnFilterUpdate);
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
            this._PickerToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this._RefreshGamesButton,
                this._ToolStripSeparator1,
                this._AddGameTextBox,
                this._AddGameButton,
                this._ToolStripSeparator2,
                this._FilterDropDownButton,
                this._ToolStripSeparator3,
                this._FindGamesLabel,
                this._SearchGameTextBox});
            this._PickerToolStrip.Location = new System.Drawing.Point(0, 0);
            this._PickerToolStrip.Name = "_PickerToolStrip";
            this._PickerToolStrip.Size = new System.Drawing.Size(742, 25);
            this._PickerToolStrip.TabIndex = 1;
            this._PickerToolStrip.Text = "toolStrip1";
            // 
            // _RefreshGamesButton
            // 
            this._RefreshGamesButton.Image = global::SAM.Picker.Resources.Refresh;
            this._RefreshGamesButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._RefreshGamesButton.Name = "_RefreshGamesButton";
            this._RefreshGamesButton.Size = new System.Drawing.Size(105, 22);
            this._RefreshGamesButton.Text = "Refresh Games";
            this._RefreshGamesButton.Click += new System.EventHandler(this.OnRefresh);
            // 
            // _AddGameTextBox
            // 
            this._AddGameTextBox.Name = "_AddGameTextBox";
            this._AddGameTextBox.Size = new System.Drawing.Size(100, 25);
            // 
            // _AddGameButton
            // 
            this._AddGameButton.Image = global::SAM.Picker.Resources.Search;
            this._AddGameButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._AddGameButton.Name = "_AddGameButton";
            this._AddGameButton.Size = new System.Drawing.Size(83, 22);
            this._AddGameButton.Text = "Add Game";
            this._AddGameButton.Click += new System.EventHandler(this.OnAddGame);
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
            this._FilterDropDownButton.Size = new System.Drawing.Size(29, 22);
            this._FilterDropDownButton.Text = "Game filtering";
            // 
            // _FilterGamesMenuItem
            // 
            this._FilterGamesMenuItem.Checked = true;
            this._FilterGamesMenuItem.CheckOnClick = true;
            this._FilterGamesMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this._FilterGamesMenuItem.Name = "_FilterGamesMenuItem";
            this._FilterGamesMenuItem.Size = new System.Drawing.Size(142, 22);
            this._FilterGamesMenuItem.Text = "Show &games";
            this._FilterGamesMenuItem.CheckedChanged += new System.EventHandler(this.OnFilterUpdate);
            // 
            // _FilterDemosMenuItem
            // 
            this._FilterDemosMenuItem.CheckOnClick = true;
            this._FilterDemosMenuItem.Name = "_FilterDemosMenuItem";
            this._FilterDemosMenuItem.Size = new System.Drawing.Size(142, 22);
            this._FilterDemosMenuItem.Text = "Show &demos";
            this._FilterDemosMenuItem.CheckedChanged += new System.EventHandler(this.OnFilterUpdate);
            // 
            // _FilterModsMenuItem
            // 
            this._FilterModsMenuItem.CheckOnClick = true;
            this._FilterModsMenuItem.Name = "_FilterModsMenuItem";
            this._FilterModsMenuItem.Size = new System.Drawing.Size(142, 22);
            this._FilterModsMenuItem.Text = "Show &mods";
            this._FilterModsMenuItem.CheckedChanged += new System.EventHandler(this.OnFilterUpdate);
            // 
            // _FilterJunkMenuItem
            // 
            this._FilterJunkMenuItem.CheckOnClick = true;
            this._FilterJunkMenuItem.Name = "_FilterJunkMenuItem";
            this._FilterJunkMenuItem.Size = new System.Drawing.Size(142, 22);
            this._FilterJunkMenuItem.Text = "Show &junk";
            this._FilterJunkMenuItem.CheckedChanged += new System.EventHandler(this.OnFilterUpdate);
            // 
            // _GameListView
            // 
            this._GameListView.BackColor = System.Drawing.Color.Black;
            this._GameListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this._GameListView.ForeColor = System.Drawing.Color.White;
            this._GameListView.LargeImageList = this._LogoImageList;
            this._GameListView.Location = new System.Drawing.Point(0, 25);
            this._GameListView.MultiSelect = false;
            this._GameListView.Name = "_GameListView";
            this._GameListView.Size = new System.Drawing.Size(742, 245);
            this._GameListView.SmallImageList = this._LogoImageList;
            this._GameListView.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this._GameListView.TabIndex = 0;
            this._GameListView.TileSize = new System.Drawing.Size(184, 69);
            this._GameListView.UseCompatibleStateImageBehavior = false;
            this._GameListView.VirtualMode = true;
            this._GameListView.ItemActivate += new System.EventHandler(this.OnActivateGame);
            this._GameListView.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.OnGameListViewRetrieveVirtualItem);
            this._GameListView.SearchForVirtualItem += new System.Windows.Forms.SearchForVirtualItemEventHandler(this.OnGameListViewSearchForVirtualItem);
            // 
            // _PickerStatusStrip
            // 
            this._PickerStatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._PickerStatusLabel,
            this._DownloadStatusLabel});
            this._PickerStatusStrip.Location = new System.Drawing.Point(0, 270);
            this._PickerStatusStrip.Name = "_PickerStatusStrip";
            this._PickerStatusStrip.Size = new System.Drawing.Size(742, 22);
            this._PickerStatusStrip.TabIndex = 2;
            this._PickerStatusStrip.Text = "statusStrip";
            // 
            // _PickerStatusLabel
            // 
            this._PickerStatusLabel.Name = "_PickerStatusLabel";
            this._PickerStatusLabel.Size = new System.Drawing.Size(727, 17);
            this._PickerStatusLabel.Spring = true;
            this._PickerStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _DownloadStatusLabel
            // 
            this._DownloadStatusLabel.Image = global::SAM.Picker.Resources.Download;
            this._DownloadStatusLabel.Name = "_DownloadStatusLabel";
            this._DownloadStatusLabel.Size = new System.Drawing.Size(111, 17);
            this._DownloadStatusLabel.Text = "Download status";
            this._DownloadStatusLabel.Visible = false;
            // 
            // _LogoWorker
            // 
            this._LogoWorker.WorkerSupportsCancellation = true;
            this._LogoWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.DoDownloadLogo);
            this._LogoWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.OnDownloadLogo);
            // 
            // _ListWorker
            // 
            this._ListWorker.WorkerSupportsCancellation = true;
            this._ListWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.DoDownloadList);
            this._ListWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.OnDownloadList);
            // 
            // GamePicker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(742, 292);
            this.Controls.Add(this._GameListView);
            this.Controls.Add(this._PickerStatusStrip);
            this.Controls.Add(this._PickerToolStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "GamePicker";
            this.Text = "Steam Achievement Manager 7.0 | Pick a game... Any game...";
            this._PickerToolStrip.ResumeLayout(false);
            this._PickerToolStrip.PerformLayout();
            this._PickerStatusStrip.ResumeLayout(false);
            this._PickerStatusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
	}

	#endregion

	private DoubleBufferedListView _GameListView;
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
	private System.Windows.Forms.ToolStripSeparator _ToolStripSeparator1;
	private System.Windows.Forms.ToolStripSeparator _ToolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator _ToolStripSeparator3;
    }
}

