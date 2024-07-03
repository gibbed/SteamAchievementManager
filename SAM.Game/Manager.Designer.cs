namespace SAM.Game
{
	partial class Manager
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
            components = new System.ComponentModel.Container();
            System.Windows.Forms.ToolStripSeparator _ToolStripSeparator1;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Manager));
            _MainToolStrip = new System.Windows.Forms.ToolStrip();
            _StoreButton = new System.Windows.Forms.ToolStripButton();
            _ReloadButton = new System.Windows.Forms.ToolStripButton();
            _ResetButton = new System.Windows.Forms.ToolStripButton();
            _AchievementImageList = new System.Windows.Forms.ImageList(components);
            _MainStatusStrip = new System.Windows.Forms.StatusStrip();
            _CountryStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            _GameStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            _DownloadStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            _CallbackTimer = new System.Windows.Forms.Timer(components);
            _MainTabControl = new System.Windows.Forms.TabControl();
            _AchievementsTabPage = new System.Windows.Forms.TabPage();
            _AchievementListView = new SAM.Game.DoubleBufferedListView();
            _AchievementNameColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            _AchievementDescriptionColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            _AchievementsToolStrip = new System.Windows.Forms.ToolStrip();
            _LockAllButton = new System.Windows.Forms.ToolStripButton();
            _InvertAllButton = new System.Windows.Forms.ToolStripButton();
            _UnlockAllButton = new System.Windows.Forms.ToolStripButton();
            _StatisticsTabPage = new System.Windows.Forms.TabPage();
            _EnableStatsEditingCheckBox = new System.Windows.Forms.CheckBox();
            _StatisticsDataGridView = new System.Windows.Forms.DataGridView();
            _ToolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            _MainToolStrip.SuspendLayout();
            _MainStatusStrip.SuspendLayout();
            _MainTabControl.SuspendLayout();
            _AchievementsTabPage.SuspendLayout();
            _AchievementsToolStrip.SuspendLayout();
            _StatisticsTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(_StatisticsDataGridView)).BeginInit();
            SuspendLayout();
            // 
            // _ToolStripSeparator1
            // 
            _ToolStripSeparator1.Name = "_ToolStripSeparator1";
            _ToolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // _MainToolStrip
            // 
            _MainToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            _StoreButton,
            _ReloadButton,
            _ToolStripSeparator1,
            _ResetButton});
            _MainToolStrip.Location = new System.Drawing.Point(0, 0);
            _MainToolStrip.Name = "_MainToolStrip";
            _MainToolStrip.Size = new System.Drawing.Size(632, 25);
            _MainToolStrip.TabIndex = 1;
            // 
            // _StoreButton
            // 
            _StoreButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            _StoreButton.Enabled = false;
            _StoreButton.Image = global::SAM.Game.Resources.Save;
            _StoreButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            _StoreButton.Name = "_StoreButton";
            _StoreButton.Size = new System.Drawing.Size(120, 22);
            _StoreButton.Text = "Commit Changes";
            _StoreButton.ToolTipText = "Store achievements and statistics for active game.";
            _StoreButton.Click += new System.EventHandler(OnStore);
            // 
            // _ReloadButton
            // 
            _ReloadButton.Enabled = false;
            _ReloadButton.Image = global::SAM.Game.Resources.Refresh;
            _ReloadButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            _ReloadButton.Name = "_ReloadButton";
            _ReloadButton.Size = new System.Drawing.Size(66, 22);
            _ReloadButton.Text = "Refresh";
            _ReloadButton.ToolTipText = "Refresh achievements and statistics for active game.";
            _ReloadButton.Click += new System.EventHandler(OnRefresh);
            // 
            // _ResetButton
            // 
            _ResetButton.Image = global::SAM.Game.Resources.Reset;
            _ResetButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            _ResetButton.Name = "_ResetButton";
            _ResetButton.Size = new System.Drawing.Size(55, 22);
            _ResetButton.Text = "Reset";
            _ResetButton.ToolTipText = "Reset achievements and/or statistics for active game.";
            _ResetButton.Click += new System.EventHandler(OnResetAllStats);
            // 
            // _AchievementImageList
            // 
            _AchievementImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            _AchievementImageList.ImageSize = new System.Drawing.Size(64, 64);
            _AchievementImageList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // _MainStatusStrip
            // 
            _MainStatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            _CountryStatusLabel,
            _GameStatusLabel,
            _DownloadStatusLabel});
            _MainStatusStrip.Location = new System.Drawing.Point(0, 370);
            _MainStatusStrip.Name = "_MainStatusStrip";
            _MainStatusStrip.Size = new System.Drawing.Size(632, 22);
            _MainStatusStrip.TabIndex = 4;
            _MainStatusStrip.Text = "statusStrip1";
            // 
            // _CountryStatusLabel
            // 
            _CountryStatusLabel.Name = "_CountryStatusLabel";
            _CountryStatusLabel.Size = new System.Drawing.Size(0, 17);
            // 
            // _GameStatusLabel
            // 
            _GameStatusLabel.Name = "_GameStatusLabel";
            _GameStatusLabel.Size = new System.Drawing.Size(617, 17);
            _GameStatusLabel.Spring = true;
            _GameStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _DownloadStatusLabel
            // 
            _DownloadStatusLabel.Image = global::SAM.Game.Resources.Download;
            _DownloadStatusLabel.Name = "_DownloadStatusLabel";
            _DownloadStatusLabel.Size = new System.Drawing.Size(111, 17);
            _DownloadStatusLabel.Text = "Download status";
            _DownloadStatusLabel.Visible = false;
            // 
            // _CallbackTimer
            // 
            _CallbackTimer.Enabled = true;
            _CallbackTimer.Tick += new System.EventHandler(OnTimer);
            // 
            // _MainTabControl
            // 
            _MainTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            _MainTabControl.Controls.Add(_AchievementsTabPage);
            _MainTabControl.Controls.Add(_StatisticsTabPage);
            _MainTabControl.Location = new System.Drawing.Point(8, 33);
            _MainTabControl.Name = "_MainTabControl";
            _MainTabControl.SelectedIndex = 0;
            _MainTabControl.Size = new System.Drawing.Size(616, 334);
            _MainTabControl.TabIndex = 5;
            // 
            // _AchievementsTabPage
            // 
            _AchievementsTabPage.Controls.Add(_AchievementListView);
            _AchievementsTabPage.Controls.Add(_AchievementsToolStrip);
            _AchievementsTabPage.Location = new System.Drawing.Point(4, 22);
            _AchievementsTabPage.Name = "_AchievementsTabPage";
            _AchievementsTabPage.Padding = new System.Windows.Forms.Padding(3);
            _AchievementsTabPage.Size = new System.Drawing.Size(608, 308);
            _AchievementsTabPage.TabIndex = 0;
            _AchievementsTabPage.Text = "Achievements";
            _AchievementsTabPage.UseVisualStyleBackColor = true;
            // 
            // _AchievementListView
            // 
            _AchievementListView.Activation = System.Windows.Forms.ItemActivation.OneClick;
            _AchievementListView.BackColor = System.Drawing.Color.Black;
            _AchievementListView.BackgroundImageTiled = true;
            _AchievementListView.CheckBoxes = true;
            _AchievementListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            _AchievementNameColumnHeader,
            _AchievementDescriptionColumnHeader});
            _AchievementListView.Dock = System.Windows.Forms.DockStyle.Fill;
            _AchievementListView.ForeColor = System.Drawing.Color.White;
            _AchievementListView.FullRowSelect = true;
            _AchievementListView.GridLines = true;
            _AchievementListView.HideSelection = false;
            _AchievementListView.LargeImageList = _AchievementImageList;
            _AchievementListView.Location = new System.Drawing.Point(3, 28);
            _AchievementListView.Name = "_AchievementListView";
            _AchievementListView.Size = new System.Drawing.Size(602, 277);
            _AchievementListView.SmallImageList = _AchievementImageList;
            _AchievementListView.Sorting = System.Windows.Forms.SortOrder.Ascending;
            _AchievementListView.TabIndex = 4;
            _AchievementListView.UseCompatibleStateImageBehavior = false;
            _AchievementListView.View = System.Windows.Forms.View.Details;
            _AchievementListView.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(OnCheckAchievement);
            // 
            // _AchievementNameColumnHeader
            // 
            _AchievementNameColumnHeader.Text = "Name";
            _AchievementNameColumnHeader.Width = 200;
            // 
            // _AchievementDescriptionColumnHeader
            // 
            _AchievementDescriptionColumnHeader.Text = "Description";
            _AchievementDescriptionColumnHeader.Width = 380;
            // 
            // _AchievementsToolStrip
            // 
            _AchievementsToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            _LockAllButton,
            _InvertAllButton,
            _UnlockAllButton});
            _AchievementsToolStrip.Location = new System.Drawing.Point(3, 3);
            _AchievementsToolStrip.Name = "_AchievementsToolStrip";
            _AchievementsToolStrip.Size = new System.Drawing.Size(602, 25);
            _AchievementsToolStrip.TabIndex = 5;
            // 
            // _LockAllButton
            // 
            _LockAllButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            _LockAllButton.Image = global::SAM.Game.Resources.Lock;
            _LockAllButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            _LockAllButton.Name = "_LockAllButton";
            _LockAllButton.Size = new System.Drawing.Size(23, 22);
            _LockAllButton.Text = "Lock All";
            _LockAllButton.ToolTipText = "Lock all achievements.";
            _LockAllButton.Click += new System.EventHandler(OnLockAll);
            // 
            // _InvertAllButton
            // 
            _InvertAllButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            _InvertAllButton.Image = global::SAM.Game.Resources.Invert;
            _InvertAllButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            _InvertAllButton.Name = "_InvertAllButton";
            _InvertAllButton.Size = new System.Drawing.Size(23, 22);
            _InvertAllButton.Text = "Invert All";
            _InvertAllButton.ToolTipText = "Invert all achievements.";
            _InvertAllButton.Click += new System.EventHandler(OnInvertAll);
            // 
            // _UnlockAllButton
            // 
            _UnlockAllButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            _UnlockAllButton.Image = global::SAM.Game.Resources.Unlock;
            _UnlockAllButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            _UnlockAllButton.Name = "_UnlockAllButton";
            _UnlockAllButton.Size = new System.Drawing.Size(23, 22);
            _UnlockAllButton.Text = "Unlock All";
            _UnlockAllButton.ToolTipText = "Unlock all achievements.";
            _UnlockAllButton.Click += new System.EventHandler(OnUnlockAll);
            // 
            // _StatisticsTabPage
            // 
            _StatisticsTabPage.Controls.Add(_EnableStatsEditingCheckBox);
            _StatisticsTabPage.Controls.Add(_StatisticsDataGridView);
            _StatisticsTabPage.Location = new System.Drawing.Point(4, 22);
            _StatisticsTabPage.Name = "_StatisticsTabPage";
            _StatisticsTabPage.Padding = new System.Windows.Forms.Padding(3);
            _StatisticsTabPage.Size = new System.Drawing.Size(608, 308);
            _StatisticsTabPage.TabIndex = 1;
            _StatisticsTabPage.Text = "Statistics";
            _StatisticsTabPage.UseVisualStyleBackColor = true;
            // 
            // _EnableStatsEditingCheckBox
            // 
            _EnableStatsEditingCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            _EnableStatsEditingCheckBox.AutoSize = true;
            _EnableStatsEditingCheckBox.Location = new System.Drawing.Point(6, 285);
            _EnableStatsEditingCheckBox.Name = "_EnableStatsEditingCheckBox";
            _EnableStatsEditingCheckBox.Size = new System.Drawing.Size(512, 17);
            _EnableStatsEditingCheckBox.TabIndex = 1;
            _EnableStatsEditingCheckBox.Text = "I understand by modifying the values of stats, I may screw things up and can\'t bl" +
    "ame anyone but myself.";
            _EnableStatsEditingCheckBox.UseVisualStyleBackColor = true;
            _EnableStatsEditingCheckBox.CheckedChanged += new System.EventHandler(OnStatAgreementChecked);
            // 
            // _StatisticsDataGridView
            // 
            _StatisticsDataGridView.AllowUserToAddRows = false;
            _StatisticsDataGridView.AllowUserToDeleteRows = false;
            _StatisticsDataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            _StatisticsDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            _StatisticsDataGridView.Location = new System.Drawing.Point(6, 6);
            _StatisticsDataGridView.Name = "_StatisticsDataGridView";
            _StatisticsDataGridView.Size = new System.Drawing.Size(596, 273);
            _StatisticsDataGridView.TabIndex = 0;
            _StatisticsDataGridView.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(OnStatCellEndEdit);
            _StatisticsDataGridView.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(OnStatDataError);
            // 
            // Manager
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(632, 392);
            Controls.Add(_MainToolStrip);
            Controls.Add(_MainTabControl);
            Controls.Add(_MainStatusStrip);
            Icon = ((System.Drawing.Icon)(resources.GetObject("$Icon")));
            MinimumSize = new System.Drawing.Size(640, 50);
            Name = "Manager";
            Text = "Steam Achievement Manager 7.0";
            _MainToolStrip.ResumeLayout(false);
            _MainToolStrip.PerformLayout();
            _MainStatusStrip.ResumeLayout(false);
            _MainStatusStrip.PerformLayout();
            _MainTabControl.ResumeLayout(false);
            _AchievementsTabPage.ResumeLayout(false);
            _AchievementsTabPage.PerformLayout();
            _AchievementsToolStrip.ResumeLayout(false);
            _AchievementsToolStrip.PerformLayout();
            _StatisticsTabPage.ResumeLayout(false);
            _StatisticsTabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(_StatisticsDataGridView)).EndInit();
            ResumeLayout(false);
            PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ToolStrip _MainToolStrip;
		private System.Windows.Forms.ToolStripButton _StoreButton;
        private System.Windows.Forms.ToolStripButton _ReloadButton;
		private System.Windows.Forms.StatusStrip _MainStatusStrip;
		private System.Windows.Forms.ToolStripStatusLabel _CountryStatusLabel;
		private System.Windows.Forms.ToolStripStatusLabel _GameStatusLabel;
		private System.Windows.Forms.ImageList _AchievementImageList;
        private System.Windows.Forms.Timer _CallbackTimer;
        private System.Windows.Forms.TabControl _MainTabControl;
        private System.Windows.Forms.TabPage _AchievementsTabPage;
        private System.Windows.Forms.TabPage _StatisticsTabPage;
        private DoubleBufferedListView _AchievementListView;
        private System.Windows.Forms.ColumnHeader _AchievementNameColumnHeader;
        private System.Windows.Forms.ColumnHeader _AchievementDescriptionColumnHeader;
        private System.Windows.Forms.ToolStrip _AchievementsToolStrip;
        private System.Windows.Forms.ToolStripButton _LockAllButton;
        private System.Windows.Forms.ToolStripButton _InvertAllButton;
        private System.Windows.Forms.ToolStripButton _UnlockAllButton;
        private System.Windows.Forms.DataGridView _StatisticsDataGridView;
        public System.Windows.Forms.CheckBox _EnableStatsEditingCheckBox;
        private System.Windows.Forms.ToolStripButton _ResetButton;
        private System.Windows.Forms.ToolStripStatusLabel _DownloadStatusLabel;
	}
}

