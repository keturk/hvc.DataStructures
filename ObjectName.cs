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

using hvc.Extensions;

namespace hvc.DataStructures;

public abstract class ObjectName
{
    public abstract String Original { get; }
    public override String ToString() => Original;

    protected String? PluralForm;
    public abstract String Plural { get; }

    protected String? SingularForm;
    public abstract String Singular { get; }

    public virtual String CamelCase => Original.CamelCase();

    protected static String Simplify(String? name)
    {
        return String.IsNullOrWhiteSpace(name) ? String.Empty : name.AfterLastOccurrenceOf(".").AfterLastOccurrenceOf("+");
    }

    public Boolean Is(String name)
    {
        return String.Equals(Original, name, StringComparison.CurrentCultureIgnoreCase);
    }
}