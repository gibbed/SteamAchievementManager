/* Copyright (c) 2026 Rafael
 *
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 */

using System;

namespace SAM.Picker
{
    internal sealed class RunningSessionInfo
    {
        public readonly uint GameId;
        public readonly string GameName;
        public readonly DateTimeOffset StartedAt;

        public RunningSessionInfo(uint gameId, string gameName, DateTimeOffset startedAt)
        {
            this.GameId = gameId;
            this.GameName = gameName;
            this.StartedAt = startedAt;
        }
    }
}
