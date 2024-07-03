/* Copyright (c) 2019 Rick (rick 'at' gibbed 'dot' us)
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
using System.IO;
using System.Linq;

namespace SAM.Game
{
    internal class KeyValue
    {
        private static readonly KeyValue _Invalid = new KeyValue();
        public string Name = "<root>";
        public KeyValueType Type = KeyValueType.None;
        public object Value;
        public bool Valid;

        public List<KeyValue> Children = null;

        public KeyValue this[string key]
        {
            get
            {
                if (Children == null)
                {
                    return _Invalid;
                }

                var child = Children.SingleOrDefault(
                    c => string.Compare(c.Name, key, StringComparison.InvariantCultureIgnoreCase) == 0);

                if (child == null)
                {
                    return _Invalid;
                }

                return child;
            }
        }

        public string AsString(string defaultValue)
        {
            if (Valid == false)
            {
                return defaultValue;
            }

            if (Value == null)
            {
                return defaultValue;
            }

            return Value.ToString();
        }

        public int AsInteger(int defaultValue)
        {
            if (Valid == false)
            {
                return defaultValue;
            }

            switch (Type)
            {
                case KeyValueType.String:
                case KeyValueType.WideString:
                    {
                        int value;
                        if (int.TryParse((string)Value, out value) == false)
                        {
                            return defaultValue;
                        }
                        return value;
                    }

                case KeyValueType.Int32:
                    {
                        return (int)Value;
                    }

                case KeyValueType.Float32:
                    {
                        return (int)((float)Value);
                    }

                case KeyValueType.UInt64:
                    {
                        return (int)((ulong)Value & 0xFFFFFFFF);
                    }
            }

            return defaultValue;
        }

        public float AsFloat(float defaultValue)
        {
            if (Valid == false)
            {
                return defaultValue;
            }

            switch (Type)
            {
                case KeyValueType.String:
                case KeyValueType.WideString:
                    {
                        float value;
                        if (float.TryParse((string)Value, out value) == false)
                        {
                            return defaultValue;
                        }
                        return value;
                    }

                case KeyValueType.Int32:
                    {
                        return (int)Value;
                    }

                case KeyValueType.Float32:
                    {
                        return (float)Value;
                    }

                case KeyValueType.UInt64:
                    {
                        return (ulong)Value & 0xFFFFFFFF;
                    }
            }

            return defaultValue;
        }

        public bool AsBoolean(bool defaultValue)
        {
            if (Valid == false)
            {
                return defaultValue;
            }

            switch (Type)
            {
                case KeyValueType.String:
                case KeyValueType.WideString:
                    {
                        int value;
                        if (int.TryParse((string)Value, out value) == false)
                        {
                            return defaultValue;
                        }
                        return value != 0;
                    }

                case KeyValueType.Int32:
                    {
                        return ((int)Value) != 0;
                    }

                case KeyValueType.Float32:
                    {
                        return ((int)((float)Value)) != 0;
                    }

                case KeyValueType.UInt64:
                    {
                        return ((ulong)Value) != 0;
                    }
            }

            return defaultValue;
        }

        public override string ToString()
        {
            if (Valid == false)
            {
                return "<invalid>";
            }

            if (Type == KeyValueType.None)
            {
                return Name;
            }

            return string.Format(
                System.Globalization.CultureInfo.CurrentCulture,
                "{0} = {1}",
                Name,
                Value);
        }

        public static KeyValue LoadAsBinary(string path)
        {
            if (File.Exists(path) == false)
            {
                return null;
            }

            try
            {
                using (var input = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var kv = new KeyValue();
                    if (kv.ReadAsBinary(input) == false)
                    {
                        return null;
                    }
                    return kv;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public bool ReadAsBinary(Stream input)
        {
            Children = new List<KeyValue>();

            try
            {
                while (true)
                {
                    var type = (KeyValueType)input.ReadValueU8();

                    if (type == KeyValueType.End)
                    {
                        break;
                    }

                    var current = new KeyValue
                    {
                        Type = type,
                        Name = input.ReadStringUnicode(),
                    };

                    switch (type)
                    {
                        case KeyValueType.None:
                            {
                                current.ReadAsBinary(input);
                                break;
                            }

                        case KeyValueType.String:
                            {
                                current.Valid = true;
                                current.Value = input.ReadStringUnicode();
                                break;
                            }

                        case KeyValueType.WideString:
                            {
                                throw new FormatException("wstring is unsupported");
                            }

                        case KeyValueType.Int32:
                            {
                                current.Valid = true;
                                current.Value = input.ReadValueS32();
                                break;
                            }

                        case KeyValueType.UInt64:
                            {
                                current.Valid = true;
                                current.Value = input.ReadValueU64();
                                break;
                            }

                        case KeyValueType.Float32:
                            {
                                current.Valid = true;
                                current.Value = input.ReadValueF32();
                                break;
                            }

                        case KeyValueType.Color:
                            {
                                current.Valid = true;
                                current.Value = input.ReadValueU32();
                                break;
                            }

                        case KeyValueType.Pointer:
                            {
                                current.Valid = true;
                                current.Value = input.ReadValueU32();
                                break;
                            }

                        default:
                            {
                                throw new FormatException();
                            }
                    }

                    if (input.Position >= input.Length)
                    {
                        throw new FormatException();
                    }

                    Children.Add(current);
                }

                Valid = true;
                return input.Position == input.Length;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}