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
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SAM.API
{
    public abstract class NativeWrapper<TNativeFunctions> : INativeWrapper
    {
        protected IntPtr ObjectAddress;
        protected TNativeFunctions Functions;

        public override string ToString()
        {
            return string.Format(
                System.Globalization.CultureInfo.CurrentCulture,
                "Steam Interface<{0}> #{1:X8}",
                typeof(TNativeFunctions),
                ObjectAddress.ToInt32());
        }

        public void SetupFunctions(IntPtr objectAddress)
        {
            ObjectAddress = objectAddress;
            var iface = Marshal.PtrToStructure<NativeClass>(ObjectAddress);
            Functions = Marshal.PtrToStructure<TNativeFunctions>(iface.VirtualTable);
        }

        private readonly Dictionary<IntPtr, Delegate> _FunctionCache = [];

        protected Delegate GetDelegate<TDelegate>(IntPtr pointer)
        {
            Delegate function;

            if (_FunctionCache.ContainsKey(pointer) == false)
            {
                function = Marshal.GetDelegateForFunctionPointer(pointer, typeof(TDelegate));
                _FunctionCache[pointer] = function;
            }
            else
            {
                function = _FunctionCache[pointer];
            }

            return function;
        }

        protected TDelegate GetFunction<TDelegate>(IntPtr pointer)
            where TDelegate : class
        {
            return (TDelegate)((object)GetDelegate<TDelegate>(pointer));
        }

        protected void Call<TDelegate>(IntPtr pointer, params object[] args)
        {
            GetDelegate<TDelegate>(pointer).DynamicInvoke(args);
        }

        protected TReturn Call<TReturn, TDelegate>(IntPtr pointer, params object[] args)
        {
            return (TReturn)GetDelegate<TDelegate>(pointer).DynamicInvoke(args);
        }
    }
}