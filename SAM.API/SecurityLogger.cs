/* Copyright (c) 2024 Rick (rick 'at' gibbed 'dot' us)
 *
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 *
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 *
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 */

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;

namespace SAM.API
{
    public enum LogLevel
    {
        Error,
        Warning,
        Info
    }

    public enum LogContext
    {
        Init,
        Native,
        HTTP,
        Parse,
        Callback,
        Validation
    }

    public static class SecurityLogger
    {
        private static readonly object _fileLock = new object();
        private static readonly ConcurrentDictionary<string, DateTime> _rateLimitCache = new();
        private static readonly TimeSpan _rateLimitInterval = TimeSpan.FromSeconds(60);
        private static string _logFilePath;
        private static bool _fileLoggingEnabled;

        static SecurityLogger()
        {
            try
            {
                // Try to set up file logging in user's local app data
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string logDir = Path.Combine(appDataPath, "SAM");

                if (!Directory.Exists(logDir))
                {
                    Directory.CreateDirectory(logDir);
                }

                _logFilePath = Path.Combine(logDir, "security.log");
                _fileLoggingEnabled = true;
            }
            catch
            {
                // If file logging setup fails, continue with Debug-only logging
                _fileLoggingEnabled = false;
            }
        }

        public static void Log(LogLevel level, LogContext context, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            try
            {
                // Format log entry
                string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
                string levelStr = level.ToString().ToUpperInvariant();
                string contextStr = context.ToString().ToUpperInvariant();
                string logEntry = $"[{timestamp}][{levelStr}][{contextStr}] {message}";

                // Always write to Debug output
                Debug.WriteLine(logEntry);

                // Write to file if enabled (non-blocking, swallow all exceptions)
                if (_fileLoggingEnabled)
                {
                    WriteToFileAsync(logEntry);
                }
            }
            catch
            {
                // Never let logging failures bubble up - swallow all exceptions
            }
        }

        public static void LogWithRateLimit(LogLevel level, LogContext context, string key, string message)
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            try
            {
                DateTime now = DateTime.UtcNow;

                // Check rate limit cache
                if (_rateLimitCache.TryGetValue(key, out DateTime lastLogged))
                {
                    if (now - lastLogged < _rateLimitInterval)
                    {
                        // Rate limited - skip this log
                        return;
                    }
                }

                // Update rate limit cache
                _rateLimitCache[key] = now;

                // Clean up old entries periodically (simple heuristic: if cache > 1000 entries)
                if (_rateLimitCache.Count > 1000)
                {
                    CleanupRateLimitCache(now);
                }

                // Log the message
                Log(level, context, message);
            }
            catch
            {
                // Never let logging failures bubble up
            }
        }

        private static void WriteToFileAsync(string logEntry)
        {
            try
            {
                // Use lock to ensure thread-safe file writes
                // Note: This is lock-based but should NOT be called during critical sections like callback frees
                lock (_fileLock)
                {
                    File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
                }
            }
            catch
            {
                // Disable file logging on failure to avoid repeated exceptions
                _fileLoggingEnabled = false;
            }
        }

        private static void CleanupRateLimitCache(DateTime now)
        {
            try
            {
                // Remove entries older than 2x the rate limit interval
                DateTime cutoff = now - TimeSpan.FromSeconds(_rateLimitInterval.TotalSeconds * 2);

                foreach (var kvp in _rateLimitCache)
                {
                    if (kvp.Value < cutoff)
                    {
                        _rateLimitCache.TryRemove(kvp.Key, out _);
                    }
                }
            }
            catch
            {
                // Swallow cleanup failures
            }
        }
    }
}
