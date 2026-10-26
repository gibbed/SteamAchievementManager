using Microsoft.UI.Dispatching;
using SAM.App.Localization;

namespace SAM.App.Steam;

/// <summary>Connection to the running Steam client including the callback pump (replaces the timers of the WinForms windows).</summary>
internal sealed class SteamSession : IDisposable
{
    private readonly DispatcherQueueTimer _callbackTimer;
    private bool _disposed;

    private SteamSession(API.Client client, uint appId, DispatcherQueue dispatcher)
    {
        Client = client;
        AppId = appId;

        _callbackTimer = dispatcher.CreateTimer();
        _callbackTimer.Interval = TimeSpan.FromMilliseconds(100);
        _callbackTimer.Tick += (_, _) =>
        {
            lock (Gate)
            {
                if (!_disposed)
                {
                    Client.RunCallbacks(false);
                }
            }
        };
        _callbackTimer.Start();
    }

    public API.Client Client { get; }

    public uint AppId { get; }

    public bool IsDisposed => _disposed;

    /// <summary>All calls into Steam go through this lock, including those from background threads.</summary>
    public object Gate { get; } = new();

    /// <summary>Connect to Steam. Throws <see cref="SteamConnectException"/> with a readable message.</summary>
    public static SteamSession Connect(uint appId, DispatcherQueue dispatcher)
    {
        var installPath = API.Steam.GetInstallPath();

        if (installPath is not null &&
            string.Equals(Path.TrimEndingDirectorySeparator(Path.GetFullPath(installPath)), Path.TrimEndingDirectorySeparator(AppContext.BaseDirectory), StringComparison.OrdinalIgnoreCase))
        {
            throw new SteamConnectException(Strings.ErrorSteamFolderTitle, Strings.ErrorSteamFolder, canRetry: false);
        }

        var client = new API.Client();

        try
        {
            client.Initialize(appId);
            return new SteamSession(client, appId, dispatcher);
        }
        catch (API.ClientInitializeException e)
        {
            client.Dispose();
            throw Describe(e, appId);
        }
        catch (Exception e) when (e is DllNotFoundException or BadImageFormatException or EntryPointNotFoundException)
        {
            client.Dispose();
            throw new SteamConnectException(Strings.ErrorSteamLoadTitle, Strings.ErrorSteamMismatch(e.Message), canRetry: true);
        }
    }

    private static SteamConnectException Describe(API.ClientInitializeException e, uint appId)
    {
        return e.Failure switch
        {
            API.ClientInitializeFailure.GetInstallPath => new SteamConnectException(Strings.ErrorNotFoundTitle, Strings.ErrorNotFound, canRetry: true),
            API.ClientInitializeFailure.Load => new SteamConnectException(Strings.ErrorSteamLoadTitle, Strings.ErrorSteamLoad, canRetry: true),
            API.ClientInitializeFailure.ConnectToGlobalUser when appId != 0 => new SteamConnectException(
                Strings.ErrorNoConnectionTitle,
                Strings.ErrorNotRunning + "\n\n" + Strings.ErrorFamilySharing,
                canRetry: true),
            API.ClientInitializeFailure.AppIdMismatch => new SteamConnectException(Strings.ErrorAppIdMismatchTitle, Strings.ErrorAppIdMismatch(appId), canRetry: true),
            _ => new SteamConnectException(Strings.ErrorNoConnectionTitle, Strings.ErrorNotRunning, canRetry: true),
        };
    }

    public void Dispose()
    {
        lock (Gate)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _callbackTimer.Stop();
            Client.Dispose();
        }
    }
}

internal sealed class SteamConnectException(string title, string message, bool canRetry) : Exception(message)
{
    public string Title { get; } = title;

    public bool CanRetry { get; } = canRetry;
}
