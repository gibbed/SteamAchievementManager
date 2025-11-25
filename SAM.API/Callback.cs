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
using System.Runtime.InteropServices;

namespace SAM.API
{
    public abstract class Callback : ICallback
    {
        public delegate void CallbackFunction(IntPtr param);

        public event CallbackFunction OnRun;

        public abstract int Id { get; }
        public abstract bool IsServer { get; }

        public void Run(IntPtr param)
        {
            // Validate pointer is not null
            if (param == IntPtr.Zero)
            {
                SecurityLogger.Log(LogLevel.Warning, LogContext.Callback, $"Null callback pointer received for ID {this.Id}");
                return;
            }

            try
            {
                // Check if event has subscribers before invoking
                OnRun?.Invoke(param);
            }
            catch (Exception ex)
            {
                SecurityLogger.Log(LogLevel.Error, LogContext.Callback, $"Callback {Id} execution failed: {ex.Message}");
            }
        }
    }

    public abstract class Callback<TParameter> : ICallback
        where TParameter : struct
    {
        public delegate void CallbackFunction(TParameter arg);

        public event CallbackFunction OnRun;

        public abstract int Id { get; }
        public abstract bool IsServer { get; }

        public void Run(IntPtr pvParam)
        {
            // Validate pointer is not null
            if (pvParam == IntPtr.Zero)
            {
                SecurityLogger.Log(LogLevel.Warning, LogContext.Callback, $"Null callback pointer received for ID {this.Id}");
                return;
            }

            // Validate struct size
            int structSize;
            try
            {
                structSize = Marshal.SizeOf<TParameter>();
                if (structSize <= 0)
                {
                    SecurityLogger.Log(LogLevel.Error, LogContext.Callback, $"Callback {Id} has invalid struct size: {structSize}");
                    return;
                }
            }
            catch (Exception ex)
            {
                SecurityLogger.Log(LogLevel.Error, LogContext.Callback, $"Callback {Id} failed to get struct size: {ex.Message}");
                return;
            }

            // Safely marshal with exception handling
            try
            {
                var data = (TParameter)Marshal.PtrToStructure(pvParam, typeof(TParameter));

                // Check if event has subscribers before invoking
                OnRun?.Invoke(data);
            }
            catch (Exception ex)
            {
                SecurityLogger.Log(LogLevel.Error, LogContext.Callback, $"Callback {Id} marshaling/execution failed: {ex.Message}");
            }
        }
    }
}
