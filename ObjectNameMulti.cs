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

public class ObjectNameMulti : ObjectName
{
    private readonly String _format;
    private readonly String[] _singularNames;
    private readonly String[] _pluralNames;

    private String FormattedJoin(String? format, String[] items)
    {
        return String.IsNullOrWhiteSpace(format)
            ? String.Join(String.Empty, items)
            // ReSharper disable once CoVariantArrayConversion
            : String.Format(_format, items);
    }

    public ObjectNameMulti(String? format, String[]? names)
    {
        ArgumentNullException.ThrowIfNull(format, nameof(format));
        ArgumentNullException.ThrowIfNull(names, nameof(names));

        if(names.Length < 2)
            throw new ArgumentOutOfRangeException(nameof(names));

        _format = format;

        var names1 = new String[names.Length];
        _singularNames = new String[names.Length];
        _pluralNames = new String[names.Length];

        for (var index = 0; index < names.Length; index++)
        {
            var name = Simplify(names[index]);

            names1[index] = name;
            _singularNames[index] = name.ToSingular();
            _pluralNames[index] = name.ToPlural();
        }

        Original = FormattedJoin(_format, names1);
    }

    public override String Original { get; }
    public override String Plural => PluralForm ??= FormattedJoin(_format, _pluralNames);
    public override String Singular => SingularForm ??= FormattedJoin(_format, _singularNames);
}