﻿/* Copyright (c) 2019 Rick (rick 'at' gibbed 'dot' us)
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

        public abstract int Id { get; }
        public abstract bool IsServer { get; }

        public void Run(IntPtr param)
        {
            OnRun(param);
        }

        public event CallbackFunction OnRun;
    }

    public abstract class Callback<TParameter> : ICallback
        where TParameter : struct
    {
        public delegate void CallbackFunction(TParameter arg);

        public abstract int Id { get; }
        public abstract bool IsServer { get; }

        public void Run(IntPtr pvParam)
        {
            var data = (TParameter)Marshal.PtrToStructure(pvParam, typeof(TParameter));
            OnRun(data);
        }

        public event CallbackFunction OnRun;
    }
}