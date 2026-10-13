using System;
using SAM.Picker;

internal static class UnlockAllProgressTests
{
    private static void Main()
    {
        var progress = new UnlockAllProgress(2, 3, false);
        progress.StartGame(1, 1);
        Assert(progress.GetStatus("Game") == "Loop 1 of 3 - ETA Calculating... - Processing 1/2 Game", "initial estimate");
        progress.CompleteGame(TimeSpan.FromSeconds(10));
        progress.StartGame(1, 2);
        Assert(progress.GetStatus("Game").StartsWith("Loop 1 of 3 - ETA ") && progress.GetStatus("Game").EndsWith(" - Processing 2/2 Game"), "normal status ordering");
        Assert(IsNear(progress.GetRemainingDuration(), TimeSpan.FromSeconds(50)), "first-loop ETA");
        progress.CompleteGame(TimeSpan.FromSeconds(10));
        progress.StartGame(2, 1);
        Assert(IsNear(progress.GetRemainingDuration(), TimeSpan.FromSeconds(40)), "multi-loop continuity");
        Assert(UnlockAllProgress.FormatDuration(TimeSpan.FromHours(125) + TimeSpan.FromMinutes(2) + TimeSpan.FromSeconds(3)) == "125h:02m:03s", "long duration formatting");

        var infinite = new UnlockAllProgress(1, -1, true);
        infinite.StartGame(7, 1);
        Assert(infinite.GetStatus("Game") == "Loop 7 of ∞ - ETA N/A (Infinite) - Processing 1/1 Game", "infinite status");
        Assert(new UnlockAllProgress(0, 1, false).GetRemainingDuration() == null, "zero games");

        var reset = new UnlockAllProgress(1, 1, false);
        reset.StartGame(1, 1);
        Assert(reset.GetStatus("Game") == "Loop 1 of 1 - ETA Calculating... - Processing 1/1 Game", "new run resets estimate");
    }

    private static void Assert(bool condition, string name)
    {
        if (!condition)
        {
            throw new InvalidOperationException(name);
        }
    }

    private static bool IsNear(TimeSpan? actual, TimeSpan expected)
    {
        return actual.HasValue && Math.Abs((actual.Value - expected).TotalSeconds) < 1;
    }
}
