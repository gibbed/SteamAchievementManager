using System;
using System.Diagnostics;
using System.Globalization;

namespace SAM.Picker
{
    internal sealed class UnlockAllProgress
    {
        private const double SmoothingFactor = 0.35;
        private readonly int _TotalGames;
        private readonly int _TotalLoops;
        private readonly bool _Infinite;
        private readonly Stopwatch _CurrentGameStopwatch = new();
        private double? _AverageGameSeconds;
        private int _CurrentLoop;
        private int _CurrentGameIndex;
        private int _CompletedGamesInCurrentLoop;

        public UnlockAllProgress(int totalGames, int totalLoops, bool infinite)
        {
            _TotalGames = totalGames;
            _TotalLoops = totalLoops;
            _Infinite = infinite;
        }

        public void StartGame(int currentLoop, int currentGameIndex)
        {
            _CurrentLoop = currentLoop;
            _CurrentGameIndex = currentGameIndex;
            _CompletedGamesInCurrentLoop = currentGameIndex - 1;
            _CurrentGameStopwatch.Restart();
        }

        public void CompleteGame()
        {
            _CurrentGameStopwatch.Stop();
            CompleteGame(_CurrentGameStopwatch.Elapsed);
        }

        internal void CompleteGame(TimeSpan elapsed)
        {
            double seconds = Math.Max(0, elapsed.TotalSeconds);
            _AverageGameSeconds = _AverageGameSeconds.HasValue
                ? (_AverageGameSeconds.Value * (1 - SmoothingFactor)) + (seconds * SmoothingFactor)
                : seconds;
            _CompletedGamesInCurrentLoop = _CurrentGameIndex;
        }

        public string GetStatus(string gameName)
        {
            string loopText = _Infinite ? $"Loop {_CurrentLoop} of ∞" : $"Loop {_CurrentLoop} of {_TotalLoops}";
            string prefix = $"{loopText} - ETA ";
            string processing = $" - Processing {_CurrentGameIndex}/{_TotalGames} {gameName}";
            if (_Infinite)
            {
                return prefix + "N/A (Infinite)" + processing;
            }

            TimeSpan? remaining = GetRemainingDuration();
            return prefix + (remaining.HasValue ? FormatDuration(remaining.Value) : "Calculating...") + processing;
        }

        public TimeSpan? GetRemainingDuration()
        {
            if (_Infinite || !_AverageGameSeconds.HasValue || _TotalGames <= 0 || _TotalLoops <= 0)
            {
                return null;
            }

            long totalWorkItems = (long)_TotalGames * _TotalLoops;
            long completedWorkItems = ((long)_CurrentLoop - 1) * _TotalGames + _CompletedGamesInCurrentLoop;
            long remainingWorkItems = Math.Max(0, totalWorkItems - completedWorkItems);
            if (remainingWorkItems == 0)
            {
                return TimeSpan.Zero;
            }

            double currentRemaining = Math.Max(0, _AverageGameSeconds.Value - _CurrentGameStopwatch.Elapsed.TotalSeconds);
            double seconds = currentRemaining + Math.Max(0, remainingWorkItems - 1) * _AverageGameSeconds.Value;
            return TimeSpan.FromSeconds(Math.Min(seconds, TimeSpan.MaxValue.TotalSeconds));
        }

        public static string FormatDuration(TimeSpan duration)
        {
            long totalHours = Math.Max(0, (long)duration.TotalHours);
            return string.Format(CultureInfo.InvariantCulture, "{0:D2}h:{1:D2}m:{2:D2}s", totalHours, duration.Minutes, duration.Seconds);
        }
    }
}
