using System.Diagnostics.CodeAnalysis;

[assembly:
    SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Scope = "member",
        Target = "SAM.Game.KeyValue.#LoadAsBinary(System.String)")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Scope = "member",
        Target = "SAM.Game.KeyValue.#ReadAsBinary(System.IO.Stream)")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Scope = "member",
        Target = "SAM.Game.Manager.#LoadUserGameStatsSchema()")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Scope = "member",
        Target = "SAM.Game.Manager.#OnIconDownload(System.Object,System.Net.DownloadDataCompletedEventArgs)")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Scope = "member",
        Target = "SAM.Game.Manager.#OnUserStatsReceived(SAM.API.Types.UserStatsReceived)")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1064:ExceptionsShouldBePublic", Scope = "type",
        Target = "SAM.Game.Stats.StatIsProtectedException")]
[assembly: SuppressMessage("Microsoft.Design", "CA2210:AssembliesShouldHaveValidStrongNames")]
[assembly:
    SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
        MessageId = "System.Windows.Forms.ColumnHeader.set_Text(System.String)", Scope = "member",
        Target = "SAM.Game.Manager.#InitializeComponent()")]
[assembly:
    SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
        MessageId = "System.Windows.Forms.Control.set_Text(System.String)", Scope = "member",
        Target = "SAM.Game.Manager.#InitializeComponent()")]
[assembly:
    SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
        MessageId = "System.Windows.Forms.Form.set_Text(System.String)", Scope = "member",
        Target = "SAM.Game.Manager.#.ctor(System.Int64,SAM.API.Client)")]
[assembly:
    SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
        MessageId =
            "System.Windows.Forms.MessageBox.Show(System.String,System.String,System.Windows.Forms.MessageBoxButtons,System.Windows.Forms.MessageBoxIcon)",
        Scope = "member", Target = "SAM.Game.Manager.#OnResetAllStats(System.Object,System.EventArgs)")]
[assembly:
    SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
        MessageId =
            "System.Windows.Forms.MessageBox.Show(System.String,System.String,System.Windows.Forms.MessageBoxButtons,System.Windows.Forms.MessageBoxIcon)",
        Scope = "member", Target = "SAM.Game.Manager.#OnUserStatsReceived(SAM.API.Types.UserStatsReceived)")]
[assembly:
    SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
        MessageId =
            "System.Windows.Forms.MessageBox.Show(System.String,System.String,System.Windows.Forms.MessageBoxButtons,System.Windows.Forms.MessageBoxIcon)",
        Scope = "member", Target = "SAM.Game.Program.#Main(System.String[])")]
[assembly:
    SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
        MessageId =
            "System.Windows.Forms.MessageBox.Show(System.Windows.Forms.IWin32Window,System.String,System.String,System.Windows.Forms.MessageBoxButtons,System.Windows.Forms.MessageBoxIcon)",
        Scope = "member",
        Target = "SAM.Game.Manager.#OnCheckAchievement(System.Object,System.Windows.Forms.ItemCheckEventArgs)")]
[assembly:
    SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
        MessageId =
            "System.Windows.Forms.MessageBox.Show(System.Windows.Forms.IWin32Window,System.String,System.String,System.Windows.Forms.MessageBoxButtons,System.Windows.Forms.MessageBoxIcon)",
        Scope = "member", Target = "SAM.Game.Manager.#OnResetAllStats(System.Object,System.EventArgs)")]
[assembly:
    SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
        MessageId =
            "System.Windows.Forms.MessageBox.Show(System.Windows.Forms.IWin32Window,System.String,System.String,System.Windows.Forms.MessageBoxButtons,System.Windows.Forms.MessageBoxIcon)",
        Scope = "member", Target = "SAM.Game.Manager.#OnStore(System.Object,System.EventArgs)")]
[assembly:
    SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
        MessageId =
            "System.Windows.Forms.MessageBox.Show(System.Windows.Forms.IWin32Window,System.String,System.String,System.Windows.Forms.MessageBoxButtons,System.Windows.Forms.MessageBoxIcon)",
        Scope = "member", Target = "SAM.Game.Manager.#RefreshStats()")]
[assembly:
    SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
        MessageId =
            "System.Windows.Forms.MessageBox.Show(System.Windows.Forms.IWin32Window,System.String,System.String,System.Windows.Forms.MessageBoxButtons,System.Windows.Forms.MessageBoxIcon)",
        Scope = "member", Target = "SAM.Game.Manager.#Store()")]
[assembly:
    SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
        MessageId =
            "System.Windows.Forms.MessageBox.Show(System.Windows.Forms.IWin32Window,System.String,System.String,System.Windows.Forms.MessageBoxButtons,System.Windows.Forms.MessageBoxIcon)",
        Scope = "member", Target = "SAM.Game.Manager.#StoreAchievements()")]
[assembly:
    SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
        MessageId =
            "System.Windows.Forms.MessageBox.Show(System.Windows.Forms.IWin32Window,System.String,System.String,System.Windows.Forms.MessageBoxButtons,System.Windows.Forms.MessageBoxIcon)",
        Scope = "member", Target = "SAM.Game.Manager.#StoreStatistics()")]
[assembly:
    SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
        MessageId = "System.Windows.Forms.ToolStripItem.set_Text(System.String)", Scope = "member",
        Target = "SAM.Game.Manager.#DownloadNextIcon()")]
[assembly:
    SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
        MessageId = "System.Windows.Forms.ToolStripItem.set_Text(System.String)", Scope = "member",
        Target = "SAM.Game.Manager.#InitializeComponent()")]
[assembly:
    SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
        MessageId = "System.Windows.Forms.ToolStripItem.set_Text(System.String)", Scope = "member",
        Target = "SAM.Game.Manager.#OnUserStatsReceived(SAM.API.Types.UserStatsReceived)")]
[assembly:
    SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
        MessageId = "System.Windows.Forms.ToolStripItem.set_Text(System.String)", Scope = "member",
        Target = "SAM.Game.Manager.#RefreshStats()")]
[assembly:
    SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
        MessageId = "System.Windows.Forms.ToolStripItem.set_ToolTipText(System.String)", Scope = "member",
        Target = "SAM.Game.Manager.#InitializeComponent()")]
[assembly:
    SuppressMessage("Microsoft.Globalization", "CA1309:UseOrdinalStringComparison",
        MessageId = "System.String.StartsWith(System.String,System.StringComparison)", Scope = "member",
        Target = "SAM.Game.Manager.#GetAchievements()")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "wstring",
        Scope = "member", Target = "SAM.Game.KeyValue.#ReadAsBinary(System.IO.Stream)")]
[assembly:
    SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily", Scope = "member",
        Target = "SAM.Game.Manager.#GetStatistics()")]
[assembly:
    SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily", Scope = "member",
        Target = "SAM.Game.Manager.#StoreStatistics()")]
[assembly:
    SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member",
        Target = "SAM.Game.Stats.AchievementInfo.#ImageIndex")]
[assembly:
    SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member",
        Target = "SAM.Game.Stats.StatInfo.#DisplayName")]
[assembly:
    SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member",
        Target = "SAM.Game.Stats.StatInfo.#Extra")]
[assembly:
    SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member",
        Target = "SAM.Game.Stats.StatInfo.#IsIncrementOnly")]
[assembly:
    SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member",
        Target = "SAM.Game.Stats.StatIsProtectedException.#.ctor(System.String)")]
[assembly:
    SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member",
        Target = "SAM.Game.Stats.StatIsProtectedException.#.ctor(System.String,System.Exception)")]
[assembly:
    SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member",
        Target = "SAM.Game.StreamHelpers.#ReadStringAscii(System.IO.Stream)")]
[assembly:
    SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Scope = "member",
        Target = "SAM.Game.Stats.AchievementDefinition.#IsHidden")]
[assembly:
    SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Scope = "member",
        Target = "SAM.Game.Stats.FloatStatDefinition.#DefaultValue")]
[assembly:
    SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Scope = "member",
        Target = "SAM.Game.Stats.FloatStatDefinition.#MaxChange")]
[assembly:
    SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Scope = "member",
        Target = "SAM.Game.Stats.FloatStatDefinition.#MaxValue")]
[assembly:
    SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Scope = "member",
        Target = "SAM.Game.Stats.FloatStatDefinition.#MinValue")]
[assembly:
    SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Scope = "member",
        Target = "SAM.Game.Stats.IntegerStatDefinition.#DefaultValue")]
[assembly:
    SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Scope = "member",
        Target = "SAM.Game.Stats.IntegerStatDefinition.#MaxChange")]
[assembly:
    SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Scope = "member",
        Target = "SAM.Game.Stats.IntegerStatDefinition.#MaxValue")]
[assembly:
    SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Scope = "member",
        Target = "SAM.Game.Stats.IntegerStatDefinition.#MinValue")]
[assembly:
    SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Scope = "member",
        Target = "SAM.Game.Manager.#.ctor(System.Int64,SAM.API.Client)")]
[assembly:
    SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Scope = "member",
        Target = "SAM.Game.Manager.#InitializeComponent()")]