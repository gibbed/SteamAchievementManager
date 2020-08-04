using System.Diagnostics.CodeAnalysis;

[assembly:
    SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Scope = "member",
        Target = "SAM.API.NativeWrapper`1.#Call`1(System.IntPtr,System.Object[])")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Scope = "member",
        Target = "SAM.API.NativeWrapper`1.#Call`2(System.IntPtr,System.Object[])")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Scope = "member",
        Target = "SAM.API.NativeWrapper`1.#GetDelegate`1(System.IntPtr)")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly", Scope = "member",
        Target = "SAM.API.Callback.#OnRun")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly", Scope = "member",
        Target = "SAM.API.Callback`1.#OnRun")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace",
        Target = "SAM.API.Callbacks")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#", Scope = "member",
        Target = "SAM.API.Steam.#GetCallback(System.Int32,SAM.API.Types.CallbackMessage&,System.Int32&)")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#", Scope = "member",
        Target = "SAM.API.Wrappers.SteamUserStats007.#GetAchievementState(System.String,System.Boolean&)")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#", Scope = "member",
        Target = "SAM.API.Wrappers.SteamUserStats007.#GetStatValue(System.String,System.Int32&)")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#", Scope = "member",
        Target = "SAM.API.Wrappers.SteamUserStats007.#GetStatValue(System.String,System.Single&)")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#", Scope = "member",
        Target = "SAM.API.Wrappers.SteamUtils005.#GetImageSize(System.Int32,System.Int32&,System.Int32&)")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#", Scope = "member",
        Target = "SAM.API.Steam.#GetCallback(System.Int32,SAM.API.Types.CallbackMessage&,System.Int32&)")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#", Scope = "member",
        Target = "SAM.API.Wrappers.SteamUtils005.#GetImageSize(System.Int32,System.Int32&,System.Int32&)")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Scope = "member",
        Target = "SAM.API.Steam.#GetInstallPath()")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Scope = "member",
        Target = "SAM.API.Wrappers.SteamApps003.#GetCurrentGameLanguage()")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Scope = "member",
        Target = "SAM.API.Wrappers.SteamUser012.#GetSteamId()")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Scope = "member",
        Target = "SAM.API.Wrappers.SteamUtils005.#GetAppId()")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Scope = "member",
        Target = "SAM.API.Wrappers.SteamUtils005.#GetConnectedUniverse()")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Scope = "member",
        Target = "SAM.API.Wrappers.SteamUtils005.#GetIPCountry()")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Scope = "type",
        Target = "SAM.API.Callback`1+CallbackFunction")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Scope = "type",
        Target = "SAM.API.Callback+CallbackFunction")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "0#", Scope = "member",
        Target = "SAM.API.Wrappers.SteamClient009.#CreateLocalUser(System.Int32&,SAM.API.Types.AccountType)")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Client.#SteamApps001")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Client.#SteamApps003")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Client.#SteamClient")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Client.#SteamUser")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Client.#SteamUserStats")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Client.#SteamUtils")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamApps001.#GetAppData")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamApps003.#GetAvailableGameLanguages")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamApps003.#GetCurrentGameLanguage")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamApps003.#IsCybercafe")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamApps003.#IsDlcInstalled")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamApps003.#IsLowViolence")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamApps003.#IsSubscribed")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamApps003.#IsSubscribedApp")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamApps003.#IsVACBanned")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamClient009.#ConnectToGlobalUser")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamClient009.#CreateLocalUser")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamClient009.#CreateSteamPipe")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamClient009.#GetIPCCallCount")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamClient009.#GetISteamApps")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamClient009.#GetISteamFriends")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamClient009.#GetISteamGameServer")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamClient009.#GetISteamGameServerStats")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamClient009.#GetISteamGenericInterface")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamClient009.#GetISteamMasterServerUpdater")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamClient009.#GetISteamMatchmaking")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamClient009.#GetISteamMatchmakingServers")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamClient009.#GetISteamNetworking")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamClient009.#GetISteamRemoteStorage")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamClient009.#GetISteamUser")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamClient009.#GetISteamUserStats")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamClient009.#GetISteamUtils")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamClient009.#ReleaseSteamPipe")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamClient009.#ReleaseUser")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamClient009.#RunFrame")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamClient009.#SetLocalIPBinding")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamClient009.#SetWarningMessageHook")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUser012.#BeginAuthSession")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUser012.#CancelAuthTicket")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUser012.#DecompressVoice")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUser012.#EndAuthSession")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUser012.#GetAuthSessionTicket")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUser012.#GetCompressedVoice")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUser012.#GetHSteamUser")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUser012.#GetSteamID")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUser012.#GetUserDataFolder")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUser012.#InitiateGameConnection")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUser012.#LoggedOn")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUser012.#StartVoiceRecording")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUser012.#StopVoiceRecording")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUser012.#TerminateGameConnection")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUser012.#TrackAppUsageEvent")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUser012.#UserHasLicenseForApp")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUserStats007.#ClearAchievement")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUserStats007.#DownloadLeaderboardEntries")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUserStats007.#FindLeaderboard")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUserStats007.#FindOrCreateLeaderboard")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUserStats007.#GetAchievement")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUserStats007.#GetAchievementAndUnlockTime")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUserStats007.#GetAchievementDisplayAttribute")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUserStats007.#GetAchievementIcon")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUserStats007.#GetDownloadedLeaderboardEntry")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUserStats007.#GetLeaderboardDisplayType")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUserStats007.#GetLeaderboardEntryCount")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUserStats007.#GetLeaderboardName")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUserStats007.#GetLeaderboardSortMethod")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUserStats007.#GetNumberOfCurrentPlayers")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUserStats007.#GetStatFloat")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUserStats007.#GetStatInteger")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUserStats007.#GetUserAchievement")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUserStats007.#GetUserAchievementAndUnlockTime")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUserStats007.#GetUserStatFloat")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUserStats007.#GetUserStatInt")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUserStats007.#IndicateAchievementProgress")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUserStats007.#RequestCurrentStats")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUserStats007.#RequestUserStats")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUserStats007.#ResetAllStats")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUserStats007.#SetAchievement")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUserStats007.#SetStatFloat")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUserStats007.#SetStatInteger")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUserStats007.#StoreStats")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUserStats007.#UpdateAvgRateStat")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUserStats007.#UploadLeaderboardScore")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUtils005.#GetAPICallFailureReason")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUtils005.#GetAPICallResult")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUtils005.#GetAppID")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUtils005.#GetConnectedUniverse")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUtils005.#GetCSERIPPort")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUtils005.#GetCurrentBatteryPower")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUtils005.#GetImageRGBA")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUtils005.#GetImageSize")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUtils005.#GetIPCCallCount")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUtils005.#GetIPCountry")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUtils005.#GetSecondsSinceAppActive")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUtils005.#GetSecondsSinceComputerActive")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUtils005.#GetServerRealTime")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUtils005.#IsAPICallCompleted")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUtils005.#IsOverlayEnabled")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUtils005.#OverlayNeedsPresent")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUtils005.#RunFrame")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUtils005.#SetOverlayNotificationPosition")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUtils005.#SetWarningMessageHook")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.NativeWrapper`1.#Functions")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.NativeWrapper`1.#ObjectAddress")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Types.AppDataChanged.#Id")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Types.AppDataChanged.#Result")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Types.CallbackMessage.#Id")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Types.CallbackMessage.#ParamPointer")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Types.CallbackMessage.#ParamSize")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Types.CallbackMessage.#User")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Types.UserItemsReceived.#GameId")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Types.UserItemsReceived.#ItemCount")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Types.UserItemsReceived.#Unknown")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Types.UserStatsReceived.#GameId")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Types.UserStatsReceived.#Result")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Types.UserStatsStored.#GameId")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope = "member",
        Target = "SAM.API.Types.UserStatsStored.#Result")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass", Scope = "member",
        Target = "SAM.API.Steam+Native.#GetProcAddress(System.IntPtr,System.String)")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass", Scope = "member",
        Target = "SAM.API.Steam+Native.#LoadLibraryEx(System.String,System.IntPtr,System.UInt32)")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass", Scope = "member",
        Target = "SAM.API.Steam+Native.#SetDllDirectory(System.String)")]
[assembly: SuppressMessage("Microsoft.Design", "CA2210:AssembliesShouldHaveValidStrongNames")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Auth",
        Scope = "member", Target = "SAM.API.Interfaces.ISteamUser012.#BeginAuthSession")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Auth",
        Scope = "member", Target = "SAM.API.Interfaces.ISteamUser012.#CancelAuthTicket")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Auth",
        Scope = "member", Target = "SAM.API.Interfaces.ISteamUser012.#EndAuthSession")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Auth",
        Scope = "member", Target = "SAM.API.Interfaces.ISteamUser012.#GetAuthSessionTicket")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Avg",
        Scope = "member", Target = "SAM.API.Interfaces.ISteamUserStats007.#UpdateAvgRateStat")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Cybercafe",
        Scope = "member", Target = "SAM.API.Interfaces.ISteamApps003.#IsCybercafe")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Dlc",
        Scope = "member", Target = "SAM.API.Interfaces.ISteamApps003.#IsDlcInstalled")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multiset",
        Scope = "member", Target = "SAM.API.Types.AccountType.#Multiset")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "param",
        Scope = "member", Target = "SAM.API.ICallback.#Run(System.IntPtr)")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Param",
        Scope = "member", Target = "SAM.API.Types.CallbackMessage.#ParamPointer")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Param",
        Scope = "member", Target = "SAM.API.Types.CallbackMessage.#ParamSize")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "param",
        Scope = "type", Target = "SAM.API.Callback+CallbackFunction")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Utils",
        Scope = "member", Target = "SAM.API.Client.#SteamUtils")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Utils",
        Scope = "member", Target = "SAM.API.Interfaces.ISteamClient009.#GetISteamUtils")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Utils",
        Scope = "member", Target = "SAM.API.Wrappers.SteamClient009.#GetISteamUtils`1(System.Int32,System.String)")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Utils",
        Scope = "member", Target = "SAM.API.Wrappers.SteamClient009.#GetSteamUtils004(System.Int32)")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Utils",
        Scope = "type", Target = "SAM.API.Interfaces.ISteamUtils005")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Utils",
        Scope = "type", Target = "SAM.API.Wrappers.SteamUtils005")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "API")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "API",
        Scope = "namespace", Target = "SAM.API")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "API",
        Scope = "namespace", Target = "SAM.API.Callbacks")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "API",
        Scope = "namespace", Target = "SAM.API.Interfaces")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "API",
        Scope = "namespace", Target = "SAM.API.Types")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "API",
        Scope = "namespace", Target = "SAM.API.Wrappers")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "OK", Scope = "member",
        Target = "SAM.API.Types.ItemRequestResult.#OK")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "RGBA",
        Scope = "member", Target = "SAM.API.Wrappers.SteamUtils005.#GetImageRGBA(System.Int32,System.Byte[])")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "SAM")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "SAM",
        Scope = "namespace", Target = "SAM.API")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "SAM",
        Scope = "namespace", Target = "SAM.API.Callbacks")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "SAM",
        Scope = "namespace", Target = "SAM.API.Interfaces")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "SAM",
        Scope = "namespace", Target = "SAM.API.Types")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "SAM",
        Scope = "namespace", Target = "SAM.API.Wrappers")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Scope = "member",
        Target = "SAM.API.Callback.#OnRun")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Scope = "member",
        Target = "SAM.API.Callback`1.#OnRun")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "int",
        Scope = "member", Target = "SAM.API.Interfaces.ISteamUserStats007.#GetUserStatInt")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "integer",
        Scope = "member", Target = "SAM.API.Interfaces.ISteamUserStats007.#GetStatInteger")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "integer",
        Scope = "member", Target = "SAM.API.Interfaces.ISteamUserStats007.#SetStatInteger")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "pointer",
        Scope = "member", Target = "SAM.API.NativeWrapper`1.#Call`1(System.IntPtr,System.Object[])")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "pointer",
        Scope = "member", Target = "SAM.API.NativeWrapper`1.#Call`2(System.IntPtr,System.Object[])")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "pointer",
        Scope = "member", Target = "SAM.API.NativeWrapper`1.#GetDelegate`1(System.IntPtr)")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "pointer",
        Scope = "member", Target = "SAM.API.NativeWrapper`1.#GetFunction`1(System.IntPtr)")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "0#",
        Scope = "member", Target = "SAM.API.Callback`1.#Run(System.IntPtr)")]
[assembly:
    SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member",
        Target = "SAM.API.NativeStrings.#PointerToString(System.Byte*)")]
[assembly:
    SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member",
        Target = "SAM.API.NativeStrings.#PointerToString(System.Byte*,System.Int32)")]
[assembly:
    SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Scope = "type",
        Target = "SAM.API.Interfaces.ISteamApps001")]
[assembly:
    SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Scope = "type",
        Target = "SAM.API.Interfaces.ISteamApps003")]
[assembly:
    SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Scope = "type",
        Target = "SAM.API.Interfaces.ISteamClient009")]
[assembly:
    SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Scope = "type",
        Target = "SAM.API.Interfaces.ISteamUser012")]
[assembly:
    SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Scope = "type",
        Target = "SAM.API.Interfaces.ISteamUtils005")]
[assembly:
    SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Scope = "type",
        Target = "SAM.API.Types.AppDataChanged")]
[assembly:
    SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Scope = "type",
        Target = "SAM.API.Types.CallbackMessage")]
[assembly:
    SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Scope = "type",
        Target = "SAM.API.Types.UserItemsReceived")]
[assembly:
    SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Scope = "type",
        Target = "SAM.API.Types.UserStatsReceived")]
[assembly:
    SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Scope = "type",
        Target = "SAM.API.Types.UserStatsStored")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamApps001.#GetAppData")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamApps003.#GetAvailableGameLanguages")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamApps003.#GetCurrentGameLanguage")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamApps003.#IsCybercafe")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamApps003.#IsDlcInstalled")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamApps003.#IsLowViolence")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamApps003.#IsSubscribed")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamApps003.#IsSubscribedApp")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamApps003.#IsVACBanned")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamClient009.#ConnectToGlobalUser")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamClient009.#CreateLocalUser")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamClient009.#CreateSteamPipe")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamClient009.#GetIPCCallCount")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamClient009.#GetISteamApps")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamClient009.#GetISteamFriends")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamClient009.#GetISteamGameServer")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamClient009.#GetISteamGameServerStats")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamClient009.#GetISteamGenericInterface")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamClient009.#GetISteamMasterServerUpdater")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamClient009.#GetISteamMatchmaking")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamClient009.#GetISteamMatchmakingServers")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamClient009.#GetISteamNetworking")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamClient009.#GetISteamRemoteStorage")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamClient009.#GetISteamUser")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamClient009.#GetISteamUserStats")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamClient009.#GetISteamUtils")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamClient009.#ReleaseSteamPipe")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamClient009.#ReleaseUser")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamClient009.#RunFrame")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamClient009.#SetLocalIPBinding")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamClient009.#SetWarningMessageHook")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUser012.#BeginAuthSession")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUser012.#CancelAuthTicket")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUser012.#DecompressVoice")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUser012.#EndAuthSession")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUser012.#GetAuthSessionTicket")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUser012.#GetCompressedVoice")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUser012.#GetHSteamUser")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUser012.#GetSteamID")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUser012.#GetUserDataFolder")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUser012.#InitiateGameConnection")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUser012.#LoggedOn")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUser012.#StartVoiceRecording")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUser012.#StopVoiceRecording")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUser012.#TerminateGameConnection")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUser012.#TrackAppUsageEvent")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUser012.#UserHasLicenseForApp")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUserStats007.#ClearAchievement")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUserStats007.#DownloadLeaderboardEntries")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUserStats007.#FindLeaderboard")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUserStats007.#FindOrCreateLeaderboard")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUserStats007.#GetAchievement")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUserStats007.#GetAchievementAndUnlockTime")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUserStats007.#GetAchievementDisplayAttribute")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUserStats007.#GetAchievementIcon")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUserStats007.#GetDownloadedLeaderboardEntry")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUserStats007.#GetLeaderboardDisplayType")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUserStats007.#GetLeaderboardEntryCount")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUserStats007.#GetLeaderboardName")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUserStats007.#GetLeaderboardSortMethod")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUserStats007.#GetNumberOfCurrentPlayers")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUserStats007.#GetStatFloat")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUserStats007.#GetStatInteger")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUserStats007.#GetUserAchievement")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUserStats007.#GetUserAchievementAndUnlockTime")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUserStats007.#GetUserStatFloat")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUserStats007.#GetUserStatInt")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUserStats007.#IndicateAchievementProgress")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUserStats007.#RequestCurrentStats")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUserStats007.#RequestUserStats")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUserStats007.#ResetAllStats")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUserStats007.#SetAchievement")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUserStats007.#SetStatFloat")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUserStats007.#SetStatInteger")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUserStats007.#StoreStats")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUserStats007.#UpdateAvgRateStat")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUserStats007.#UploadLeaderboardScore")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUtils005.#GetAPICallFailureReason")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUtils005.#GetAPICallResult")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUtils005.#GetAppID")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUtils005.#GetConnectedUniverse")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUtils005.#GetCSERIPPort")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUtils005.#GetCurrentBatteryPower")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUtils005.#GetImageRGBA")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUtils005.#GetImageSize")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUtils005.#GetIPCCallCount")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUtils005.#GetIPCountry")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUtils005.#GetSecondsSinceAppActive")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUtils005.#GetSecondsSinceComputerActive")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUtils005.#GetServerRealTime")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUtils005.#IsAPICallCompleted")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUtils005.#IsOverlayEnabled")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUtils005.#OverlayNeedsPresent")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUtils005.#RunFrame")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUtils005.#SetOverlayNotificationPosition")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Interfaces.ISteamUtils005.#SetWarningMessageHook")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.NativeWrapper`1.#ObjectAddress")]
[assembly:
    SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible", Scope = "member",
        Target = "SAM.API.Types.CallbackMessage.#ParamPointer")]