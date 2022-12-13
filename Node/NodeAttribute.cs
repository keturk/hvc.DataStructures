// MIT License
//
// Copyright (c) 2022 Kamil Ercan Turkarslan
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Reflection;

namespace hvc.DataStructures.Node;

public abstract class NodeAttribute
{
    public Boolean IsSet { get; private protected set; }

    public abstract void Set(String? value = null);
    public abstract void Set(String[] value);

    protected NodeAttribute(Boolean isSet)
    {
        IsSet = isSet;
    }
}

public class NodeAttribute<T> : NodeAttribute
{
    public NodeAttribute(String? defaultValue = null)
        : base(false)
    {
        Value = default;
        _defaultValue = defaultValue;
    }

    private NodeAttribute(Boolean isSet, T? value, String? defaultValue = null)
        : base(isSet)

    {
        Value = value;
        _defaultValue = defaultValue;
    }

    public T? Value { get; private set; }

    private readonly String? _defaultValue;
    public String? DefaultValue => !String.IsNullOrWhiteSpace(_defaultValue) ? _defaultValue : null;

    public String? ValueOrDefault => IsSet ? Value?.ToString() : !String.IsNullOrWhiteSpace(_defaultValue) ? _defaultValue : String.Empty;

    public Boolean HasDefault => !String.IsNullOrWhiteSpace(_defaultValue);

    public NodeAttribute<T> Clone() => new(IsSet, Value);

    public override void Set(String? value = null)
    {
        if(IsSet)
            throw new InvalidOperationException("Attribute is already set!");

        if (typeof(T) == typeof(Boolean))
        {
            if (!String.IsNullOrWhiteSpace(value))
                throw new NotSupportedException("Boolean attributes can't have a parameter!");

            Value = (T)Convert.ChangeType("True", typeof(T));
        }
        else
        {
            if (value == null)
                throw new NotSupportedException($"{nameof(value)} parameter can't be null!");

            if (typeof(T) == typeof(Int32))
            {
                if(!Int32.TryParse(value, out var outputValue))
                    throw new NotSupportedException("Int32 value expected!");

                Value = (T)Convert.ChangeType(outputValue, typeof(T));
            }
            else if (typeof(T) == typeof(String))
            {
                Value = (T?)Convert.ChangeType(value, typeof(T));
            }
            else if (typeof(T).BaseType == typeof(Enum))
            {
                if (typeof(T).GetCustomAttribute(typeof(FlagsAttribute)) != null)
                    Set(new[] { value });
                else
                    Value = (T)Enum.Parse(typeof(T), value, true);

                return;
            }
            else
                throw new NotSupportedException($"Attribute type '{typeof(T).FullName}' is not supported!");
        }

        IsSet = true;
    }

    public override void Set(String[] values)
    {
        if (IsSet)
            throw new NotSupportedException("Attribute is already set!");

        if (typeof(T).BaseType != typeof(Enum))
            throw new InvalidOperationException("Enum type expected!");

        foreach (var value in values)
            if (Value != null)
            {
                var value1 = Value;
                SetFlag(ref value1, (T)Enum.Parse(typeof(T), value, true));
                Value = value1;
            }
            else
            {
                Value = (T)Enum.Parse(typeof(T), value, true);
            }

        IsSet = true;
    }

    private static void SetFlag(ref T value, T flag)
    {
        var numericValue = Convert.ToUInt64(value);
        numericValue |= Convert.ToUInt64(flag);
        value = (T)Enum.ToObject(typeof(T), numericValue);
    }
}