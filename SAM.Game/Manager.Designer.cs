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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.ToolStripSeparator _ToolStripSeparator1;
            System.Windows.Forms.ToolStripSeparator _ToolStripSeparator2;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Manager));
            this._MainToolStrip = new System.Windows.Forms.ToolStrip();
            this._StoreButton = new System.Windows.Forms.ToolStripButton();
            this._ReloadButton = new System.Windows.Forms.ToolStripButton();
            this._ResetButton = new System.Windows.Forms.ToolStripButton();
            this._TimeNowLabel = new System.Windows.Forms.ToolStripLabel();
            this._TimerLabel = new System.Windows.Forms.ToolStripLabel();
            this._AchievementImageList = new System.Windows.Forms.ImageList(this.components);
            this._MainStatusStrip = new System.Windows.Forms.StatusStrip();
            this._CountryStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this._GameStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this._DownloadStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this._CallbackTimer = new System.Windows.Forms.Timer(this.components);
            this._MainTabControl = new System.Windows.Forms.TabControl();
            this._AchievementsTabPage = new System.Windows.Forms.TabPage();
            this._AchievementListView = new SAM.Game.DoubleBufferedListView();
            this._AchievementNameColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._AchievementDescriptionColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._AchievementUnlockTimeColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._AchievementIDColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._AchievementTimerColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._AchievementsToolStrip = new System.Windows.Forms.ToolStrip();
            this._LockAllButton = new System.Windows.Forms.ToolStripButton();
            this._InvertAllButton = new System.Windows.Forms.ToolStripButton();
            this._UnlockAllButton = new System.Windows.Forms.ToolStripButton();
            this._DisplayLabel = new System.Windows.Forms.ToolStripLabel();
            this._DisplayLockedOnlyButton = new System.Windows.Forms.ToolStripButton();
            this._DisplayUnlockedOnlyButton = new System.Windows.Forms.ToolStripButton();
            this._MatchingStringLabel = new System.Windows.Forms.ToolStripLabel();
            this._MatchingStringTextBox = new System.Windows.Forms.ToolStripTextBox();
            this._LanguageLabel = new System.Windows.Forms.ToolStripLabel();
            this._LanguageComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.AddTimerLabel = new System.Windows.Forms.ToolStripLabel();
            this._AddTimerTextBox = new System.Windows.Forms.ToolStripTextBox();
            this._AddTimerButton = new System.Windows.Forms.ToolStripButton();
            this._TimerSwitchButton = new System.Windows.Forms.ToolStripButton();
            this._StatisticsTabPage = new System.Windows.Forms.TabPage();
            this._EnableStatsEditingCheckBox = new System.Windows.Forms.CheckBox();
            this._StatisticsDataGridView = new System.Windows.Forms.DataGridView();
            this._TimeNowtimer = new System.Windows.Forms.Timer(this.components);
            this._SumbitAchievementsTimer = new System.Windows.Forms.Timer(this.components);
            _ToolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            _ToolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this._MainToolStrip.SuspendLayout();
            this._MainStatusStrip.SuspendLayout();
            this._MainTabControl.SuspendLayout();
            this._AchievementsTabPage.SuspendLayout();
            this._AchievementsToolStrip.SuspendLayout();
            this._StatisticsTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._StatisticsDataGridView)).BeginInit();
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
            // _MainToolStrip
            // 
            this._MainToolStrip.Font = new System.Drawing.Font("Microsoft JhengHei UI", 11.14286F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this._MainToolStrip.ImageScalingSize = new System.Drawing.Size(28, 28);
            this._MainToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._StoreButton,
            this._ReloadButton,
            this._ResetButton,
            this._TimeNowLabel,
            this._TimerLabel});
            this._MainToolStrip.Location = new System.Drawing.Point(0, 0);
            this._MainToolStrip.Name = "_MainToolStrip";
            this._MainToolStrip.Padding = new System.Windows.Forms.Padding(0, 0, 5, 0);
            this._MainToolStrip.Size = new System.Drawing.Size(1778, 45);
            this._MainToolStrip.TabIndex = 1;
            // 
            // _StoreButton
            // 
            this._StoreButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this._StoreButton.Enabled = false;
            this._StoreButton.Image = global::SAM.Game.Resources.Save;
            this._StoreButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._StoreButton.Name = "_StoreButton";
            this._StoreButton.Size = new System.Drawing.Size(265, 39);
            this._StoreButton.Text = "Commit Changes";
            this._StoreButton.ToolTipText = "Store achievements and statistics for active game.";
            this._StoreButton.Click += new System.EventHandler(this.OnStore);
            // 
            // _ReloadButton
            // 
            this._ReloadButton.Enabled = false;
            this._ReloadButton.Image = global::SAM.Game.Resources.Refresh;
            this._ReloadButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._ReloadButton.Name = "_ReloadButton";
            this._ReloadButton.Size = new System.Drawing.Size(141, 39);
            this._ReloadButton.Text = "Refresh";
            this._ReloadButton.ToolTipText = "Refresh achievements and statistics for active game.";
            this._ReloadButton.Click += new System.EventHandler(this.OnRefresh);
            // 
            // _ResetButton
            // 
            this._ResetButton.Image = global::SAM.Game.Resources.Reset;
            this._ResetButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._ResetButton.Name = "_ResetButton";
            this._ResetButton.Size = new System.Drawing.Size(116, 39);
            this._ResetButton.Text = "Reset";
            this._ResetButton.ToolTipText = "Reset achievements and/or statistics for active game.";
            this._ResetButton.Click += new System.EventHandler(this.OnResetAllStats);
            // 
            // _TimeNowLabel
            // 
            this._TimeNowLabel.Name = "_TimeNowLabel";
            this._TimeNowLabel.Size = new System.Drawing.Size(299, 39);
            this._TimeNowLabel.Text = "Time: ____/__/__ __:__:__";
            // 
            // _TimerLabel
            // 
            this._TimerLabel.Name = "_TimerLabel";
            this._TimerLabel.Size = new System.Drawing.Size(27, 39);
            this._TimerLabel.Text = "-";
            // 
            // _AchievementImageList
            // 
            this._AchievementImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth24Bit;
            this._AchievementImageList.ImageSize = new System.Drawing.Size(64, 64);
            this._AchievementImageList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // _MainStatusStrip
            // 
            this._MainStatusStrip.ImageScalingSize = new System.Drawing.Size(28, 28);
            this._MainStatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._CountryStatusLabel,
            this._GameStatusLabel,
            this._DownloadStatusLabel});
            this._MainStatusStrip.Location = new System.Drawing.Point(0, 762);
            this._MainStatusStrip.Name = "_MainStatusStrip";
            this._MainStatusStrip.Padding = new System.Windows.Forms.Padding(3, 0, 33, 0);
            this._MainStatusStrip.Size = new System.Drawing.Size(1778, 22);
            this._MainStatusStrip.TabIndex = 4;
            // 
            // _CountryStatusLabel
            // 
            this._CountryStatusLabel.Name = "_CountryStatusLabel";
            this._CountryStatusLabel.Size = new System.Drawing.Size(0, 13);
            // 
            // _GameStatusLabel
            // 
            this._GameStatusLabel.Name = "_GameStatusLabel";
            this._GameStatusLabel.Size = new System.Drawing.Size(1742, 13);
            this._GameStatusLabel.Spring = true;
            this._GameStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _DownloadStatusLabel
            // 
            this._DownloadStatusLabel.Image = global::SAM.Game.Resources.Download;
            this._DownloadStatusLabel.Name = "_DownloadStatusLabel";
            this._DownloadStatusLabel.Size = new System.Drawing.Size(207, 28);
            this._DownloadStatusLabel.Text = "Download status";
            this._DownloadStatusLabel.Visible = false;
            // 
            // _CallbackTimer
            // 
            this._CallbackTimer.Enabled = true;
            this._CallbackTimer.Tick += new System.EventHandler(this.OnTimer);
            // 
            // _MainTabControl
            // 
            this._MainTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._MainTabControl.Controls.Add(this._AchievementsTabPage);
            this._MainTabControl.Controls.Add(this._StatisticsTabPage);
            this._MainTabControl.Location = new System.Drawing.Point(19, 66);
            this._MainTabControl.Margin = new System.Windows.Forms.Padding(8, 6, 8, 6);
            this._MainTabControl.Name = "_MainTabControl";
            this._MainTabControl.SelectedIndex = 0;
            this._MainTabControl.Size = new System.Drawing.Size(1741, 669);
            this._MainTabControl.TabIndex = 5;
            // 
            // _AchievementsTabPage
            // 
            this._AchievementsTabPage.Controls.Add(this._AchievementListView);
            this._AchievementsTabPage.Controls.Add(this._AchievementsToolStrip);
            this._AchievementsTabPage.Location = new System.Drawing.Point(4, 36);
            this._AchievementsTabPage.Margin = new System.Windows.Forms.Padding(8, 6, 8, 6);
            this._AchievementsTabPage.Name = "_AchievementsTabPage";
            this._AchievementsTabPage.Padding = new System.Windows.Forms.Padding(8, 6, 8, 6);
            this._AchievementsTabPage.Size = new System.Drawing.Size(1733, 629);
            this._AchievementsTabPage.TabIndex = 0;
            this._AchievementsTabPage.Text = "Achievements";
            this._AchievementsTabPage.UseVisualStyleBackColor = true;
            // 
            // _AchievementListView
            // 
            this._AchievementListView.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this._AchievementListView.BackColor = System.Drawing.Color.Black;
            this._AchievementListView.BackgroundImageTiled = true;
            this._AchievementListView.CheckBoxes = true;
            this._AchievementListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this._AchievementNameColumnHeader,
            this._AchievementDescriptionColumnHeader,
            this._AchievementUnlockTimeColumnHeader,
            this._AchievementIDColumnHeader,
            this._AchievementTimerColumnHeader});
            this._AchievementListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this._AchievementListView.ForeColor = System.Drawing.Color.White;
            this._AchievementListView.FullRowSelect = true;
            this._AchievementListView.GridLines = true;
            this._AchievementListView.HideSelection = false;
            this._AchievementListView.LargeImageList = this._AchievementImageList;
            this._AchievementListView.Location = new System.Drawing.Point(8, 51);
            this._AchievementListView.Margin = new System.Windows.Forms.Padding(8, 6, 8, 6);
            this._AchievementListView.Name = "_AchievementListView";
            this._AchievementListView.Size = new System.Drawing.Size(1717, 572);
            this._AchievementListView.SmallImageList = this._AchievementImageList;
            this._AchievementListView.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this._AchievementListView.TabIndex = 4;
            this._AchievementListView.UseCompatibleStateImageBehavior = false;
            this._AchievementListView.View = System.Windows.Forms.View.Details;
            this._AchievementListView.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.OnCheckAchievement);
            // 
            // _AchievementNameColumnHeader
            // 
            this._AchievementNameColumnHeader.Text = "Name";
            this._AchievementNameColumnHeader.Width = 200;
            // 
            // _AchievementDescriptionColumnHeader
            // 
            this._AchievementDescriptionColumnHeader.Text = "Description";
            this._AchievementDescriptionColumnHeader.Width = 300;
            // 
            // _AchievementUnlockTimeColumnHeader
            // 
            this._AchievementUnlockTimeColumnHeader.Text = "Unlock Time";
            this._AchievementUnlockTimeColumnHeader.Width = 160;
            // 
            // _AchievementIDColumnHeader
            // 
            this._AchievementIDColumnHeader.Text = "ID";
            this._AchievementIDColumnHeader.Width = 5;
            // 
            // _AchievementTimerColumnHeader
            // 
            this._AchievementTimerColumnHeader.Text = "Commit Timer";
            this._AchievementTimerColumnHeader.Width = 208;
            // 
            // _AchievementsToolStrip
            // 
            this._AchievementsToolStrip.Font = new System.Drawing.Font("Microsoft JhengHei UI", 11.14286F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this._AchievementsToolStrip.ImageScalingSize = new System.Drawing.Size(28, 28);
            this._AchievementsToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._LockAllButton,
            this._InvertAllButton,
            this._UnlockAllButton,
            _ToolStripSeparator1,
            this._DisplayLabel,
            this._DisplayLockedOnlyButton,
            this._DisplayUnlockedOnlyButton,
            _ToolStripSeparator2,
            this._MatchingStringLabel,
            this._MatchingStringTextBox,
            this._LanguageLabel,
            this._LanguageComboBox,
            this.AddTimerLabel,
            this._AddTimerTextBox,
            this._AddTimerButton,
            this._TimerSwitchButton});
            this._AchievementsToolStrip.Location = new System.Drawing.Point(8, 6);
            this._AchievementsToolStrip.Name = "_AchievementsToolStrip";
            this._AchievementsToolStrip.Padding = new System.Windows.Forms.Padding(0, 0, 5, 0);
            this._AchievementsToolStrip.Size = new System.Drawing.Size(1717, 45);
            this._AchievementsToolStrip.TabIndex = 5;
            // 
            // _LockAllButton
            // 
            this._LockAllButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._LockAllButton.Image = global::SAM.Game.Resources.Lock;
            this._LockAllButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._LockAllButton.Name = "_LockAllButton";
            this._LockAllButton.Size = new System.Drawing.Size(40, 39);
            this._LockAllButton.Text = "Lock All";
            this._LockAllButton.ToolTipText = "Lock all achievements.";
            this._LockAllButton.Click += new System.EventHandler(this.OnLockAll);
            // 
            // _InvertAllButton
            // 
            this._InvertAllButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._InvertAllButton.Image = global::SAM.Game.Resources.Invert;
            this._InvertAllButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._InvertAllButton.Name = "_InvertAllButton";
            this._InvertAllButton.Size = new System.Drawing.Size(40, 39);
            this._InvertAllButton.Text = "Invert All";
            this._InvertAllButton.ToolTipText = "Invert all achievements.";
            this._InvertAllButton.Click += new System.EventHandler(this.OnInvertAll);
            // 
            // _UnlockAllButton
            // 
            this._UnlockAllButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._UnlockAllButton.Image = global::SAM.Game.Resources.Unlock;
            this._UnlockAllButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._UnlockAllButton.Name = "_UnlockAllButton";
            this._UnlockAllButton.Size = new System.Drawing.Size(40, 39);
            this._UnlockAllButton.Text = "Unlock All";
            this._UnlockAllButton.ToolTipText = "Unlock all achievements.";
            this._UnlockAllButton.Click += new System.EventHandler(this.OnUnlockAll);
            // 
            // _DisplayLabel
            // 
            this._DisplayLabel.Name = "_DisplayLabel";
            this._DisplayLabel.Size = new System.Drawing.Size(90, 39);
            this._DisplayLabel.Text = "Show:";
            // 
            // _DisplayLockedOnlyButton
            // 
            this._DisplayLockedOnlyButton.CheckOnClick = true;
            this._DisplayLockedOnlyButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._DisplayLockedOnlyButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._DisplayLockedOnlyButton.Name = "_DisplayLockedOnlyButton";
            this._DisplayLockedOnlyButton.Size = new System.Drawing.Size(102, 39);
            this._DisplayLockedOnlyButton.Text = "locked";
            this._DisplayLockedOnlyButton.Click += new System.EventHandler(this.OnDisplayCheckedOnly);
            // 
            // _DisplayUnlockedOnlyButton
            // 
            this._DisplayUnlockedOnlyButton.CheckOnClick = true;
            this._DisplayUnlockedOnlyButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._DisplayUnlockedOnlyButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._DisplayUnlockedOnlyButton.Name = "_DisplayUnlockedOnlyButton";
            this._DisplayUnlockedOnlyButton.Size = new System.Drawing.Size(134, 39);
            this._DisplayUnlockedOnlyButton.Text = "unlocked";
            this._DisplayUnlockedOnlyButton.Click += new System.EventHandler(this.OnDisplayUncheckedOnly);
            // 
            // _MatchingStringLabel
            // 
            this._MatchingStringLabel.Name = "_MatchingStringLabel";
            this._MatchingStringLabel.Size = new System.Drawing.Size(78, 39);
            this._MatchingStringLabel.Text = "Filter";
            // 
            // _MatchingStringTextBox
            // 
            this._MatchingStringTextBox.Font = new System.Drawing.Font("Microsoft JhengHei UI", 11.14286F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this._MatchingStringTextBox.Name = "_MatchingStringTextBox";
            this._MatchingStringTextBox.Size = new System.Drawing.Size(180, 45);
            this._MatchingStringTextBox.ToolTipText = "Type at least 3 characters that must appear in the name or description";
            this._MatchingStringTextBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.OnFilterUpdate);
            // 
            // _LanguageLabel
            // 
            this._LanguageLabel.Name = "_LanguageLabel";
            this._LanguageLabel.Size = new System.Drawing.Size(140, 39);
            this._LanguageLabel.Text = "Language";
            // 
            // _LanguageComboBox
            // 
            this._LanguageComboBox.Font = new System.Drawing.Font("Microsoft JhengHei UI", 11.14286F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this._LanguageComboBox.Items.AddRange(new object[] {
            "english",
            "tchinese",
            "japanese",
            "arabic",
            "brazilian",
            "bulgarian",
            "czech",
            "danish",
            "dutch",
            "finnish",
            "french",
            "german",
            "greek",
            "hungarian",
            "indonesian",
            "italian",
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
            "th",
            "turkish",
            "ukrainian",
            "vietnamese"});
            this._LanguageComboBox.Name = "_LanguageComboBox";
            this._LanguageComboBox.Size = new System.Drawing.Size(160, 45);
            // 
            // AddTimerLabel
            // 
            this.AddTimerLabel.Name = "AddTimerLabel";
            this.AddTimerLabel.Size = new System.Drawing.Size(123, 39);
            this.AddTimerLabel.Text = "Counter:";
            // 
            // _AddTimerTextBox
            // 
            this._AddTimerTextBox.Font = new System.Drawing.Font("Microsoft JhengHei UI", 11.14286F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this._AddTimerTextBox.Name = "_AddTimerTextBox";
            this._AddTimerTextBox.Size = new System.Drawing.Size(120, 45);
            this._AddTimerTextBox.Text = "600";
            this._AddTimerTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this._AddTimerTextBox_KeyPress);
            this._AddTimerTextBox.TextChanged += new System.EventHandler(this._AddTimerTextBox_TextChanged);
            // 
            // _AddTimerButton
            // 
            this._AddTimerButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._AddTimerButton.Image = ((System.Drawing.Image)(resources.GetObject("_AddTimerButton.Image")));
            this._AddTimerButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._AddTimerButton.Name = "_AddTimerButton";
            this._AddTimerButton.Size = new System.Drawing.Size(181, 39);
            this._AddTimerButton.Text = "Add Counter";
            this._AddTimerButton.Click += new System.EventHandler(this._AddTimerButton_Click);
            // 
            // _TimerSwitchButton
            // 
            this._TimerSwitchButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._TimerSwitchButton.Image = ((System.Drawing.Image)(resources.GetObject("_TimerSwitchButton.Image")));
            this._TimerSwitchButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._TimerSwitchButton.Name = "_TimerSwitchButton";
            this._TimerSwitchButton.Size = new System.Drawing.Size(158, 39);
            this._TimerSwitchButton.Text = "Start Timer";
            this._TimerSwitchButton.ToolTipText = "Start countdown timer";
            this._TimerSwitchButton.Click += new System.EventHandler(this._TimerSwitchButton_Click);
            // 
            // _StatisticsTabPage
            // 
            this._StatisticsTabPage.Controls.Add(this._EnableStatsEditingCheckBox);
            this._StatisticsTabPage.Controls.Add(this._StatisticsDataGridView);
            this._StatisticsTabPage.Location = new System.Drawing.Point(4, 36);
            this._StatisticsTabPage.Margin = new System.Windows.Forms.Padding(8, 6, 8, 6);
            this._StatisticsTabPage.Name = "_StatisticsTabPage";
            this._StatisticsTabPage.Padding = new System.Windows.Forms.Padding(8, 6, 8, 6);
            this._StatisticsTabPage.Size = new System.Drawing.Size(1616, 629);
            this._StatisticsTabPage.TabIndex = 1;
            this._StatisticsTabPage.Text = "Statistics";
            this._StatisticsTabPage.UseVisualStyleBackColor = true;
            // 
            // _EnableStatsEditingCheckBox
            // 
            this._EnableStatsEditingCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._EnableStatsEditingCheckBox.AutoSize = true;
            this._EnableStatsEditingCheckBox.Location = new System.Drawing.Point(14, 572);
            this._EnableStatsEditingCheckBox.Margin = new System.Windows.Forms.Padding(8, 6, 8, 6);
            this._EnableStatsEditingCheckBox.Name = "_EnableStatsEditingCheckBox";
            this._EnableStatsEditingCheckBox.Size = new System.Drawing.Size(1112, 31);
            this._EnableStatsEditingCheckBox.TabIndex = 1;
            this._EnableStatsEditingCheckBox.Text = "I understand by modifying the values of stats, I may screw things up and can\'t bl" +
    "ame anyone but myself.";
            this._EnableStatsEditingCheckBox.UseVisualStyleBackColor = true;
            this._EnableStatsEditingCheckBox.CheckedChanged += new System.EventHandler(this.OnStatAgreementChecked);
            // 
            // _StatisticsDataGridView
            // 
            this._StatisticsDataGridView.AllowUserToAddRows = false;
            this._StatisticsDataGridView.AllowUserToDeleteRows = false;
            this._StatisticsDataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._StatisticsDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._StatisticsDataGridView.Location = new System.Drawing.Point(14, 12);
            this._StatisticsDataGridView.Margin = new System.Windows.Forms.Padding(8, 6, 8, 6);
            this._StatisticsDataGridView.Name = "_StatisticsDataGridView";
            this._StatisticsDataGridView.RowHeadersWidth = 72;
            this._StatisticsDataGridView.Size = new System.Drawing.Size(1391, 546);
            this._StatisticsDataGridView.TabIndex = 0;
            this._StatisticsDataGridView.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.OnStatCellEndEdit);
            this._StatisticsDataGridView.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.OnStatDataError);
            // 
            // _TimeNowtimer
            // 
            this._TimeNowtimer.Enabled = true;
            this._TimeNowtimer.Interval = 500;
            this._TimeNowtimer.Tick += new System.EventHandler(this._TimeNowtimer_Tick);
            // 
            // _SumbitAchievementsTimer
            // 
            this._SumbitAchievementsTimer.Interval = 1000;
            this._SumbitAchievementsTimer.Tick += new System.EventHandler(this._SumbitAchievementsTimer_Tick);
            // 
            // Manager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(168F, 168F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1778, 784);
            this.Controls.Add(this._MainToolStrip);
            this.Controls.Add(this._MainTabControl);
            this.Controls.Add(this._MainStatusStrip);
            this.Font = new System.Drawing.Font("新細明體", 11.14286F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(8, 6, 8, 6);
            this.MinimumSize = new System.Drawing.Size(1461, 64);
            this.Name = "Manager";
            this.Text = "Steam Achievement ManagerX 7.0";
            this._MainToolStrip.ResumeLayout(false);
            this._MainToolStrip.PerformLayout();
            this._MainStatusStrip.ResumeLayout(false);
            this._MainStatusStrip.PerformLayout();
            this._MainTabControl.ResumeLayout(false);
            this._AchievementsTabPage.ResumeLayout(false);
            this._AchievementsTabPage.PerformLayout();
            this._AchievementsToolStrip.ResumeLayout(false);
            this._AchievementsToolStrip.PerformLayout();
            this._StatisticsTabPage.ResumeLayout(false);
            this._StatisticsTabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._StatisticsDataGridView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

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
        private System.Windows.Forms.ToolStripButton _ResetButton;
        private System.Windows.Forms.ToolStripStatusLabel _DownloadStatusLabel;
        private System.Windows.Forms.ToolStripLabel _DisplayLabel;
        private System.Windows.Forms.ToolStripButton _DisplayUnlockedOnlyButton;
        private System.Windows.Forms.ToolStripButton _DisplayLockedOnlyButton;
        private System.Windows.Forms.ToolStripLabel _MatchingStringLabel;
        private System.Windows.Forms.ToolStripTextBox _MatchingStringTextBox;
        private System.Windows.Forms.ColumnHeader _AchievementUnlockTimeColumnHeader;
        private System.Windows.Forms.CheckBox _EnableStatsEditingCheckBox;
        private System.Windows.Forms.ToolStripLabel _LanguageLabel;
        private System.Windows.Forms.ToolStripComboBox _LanguageComboBox;
        private System.Windows.Forms.ColumnHeader _AchievementIDColumnHeader;
        private System.Windows.Forms.ColumnHeader _AchievementTimerColumnHeader;
        private System.Windows.Forms.ToolStripLabel _TimeNowLabel;
        private System.Windows.Forms.Timer _TimeNowtimer;
        private System.Windows.Forms.ToolStripLabel AddTimerLabel;
        private System.Windows.Forms.ToolStripTextBox _AddTimerTextBox;
        private System.Windows.Forms.ToolStripButton _AddTimerButton;
        private System.Windows.Forms.ToolStripButton _TimerSwitchButton;
        private System.Windows.Forms.Timer _SumbitAchievementsTimer;
        private System.Windows.Forms.ToolStripLabel _TimerLabel;
    }
}
