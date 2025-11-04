using System.Buffers;

namespace SignalRGen.Shared.Common;

public static class CaseUtil
{
    public static string ToCamelCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        var span = input.AsSpan();
        
        var hasSep = false;
        foreach (var c in span)
        {
            if (c is '_' or '-' or ' ')
            {
                hasSep = true;
                break;
            }
        }

        if (!hasSep)
        {
            // Fast path: no separators. Just lower the leading uppercase run properly.
            var arr = span.ToArray();
            if (arr.Length == 0) return string.Empty;

            // If first two are uppercase, lowercase the leading acronym up to before next lowercase.
            var i = 0;
            while (i < arr.Length && char.IsUpper(arr[i]))
            {
                if (i + 1 < arr.Length && char.IsLower(arr[i + 1])) break;
                i++;
            }
            if (i == 0) i = 1; // normal PascalCase

            for (var j = 0; j < i; j++) arr[j] = char.ToLowerInvariant(arr[j]);
            return new string(arr);
        }

        // General path: remove separators, TitleCase words, then lower first char.
        var buffer = ArrayPool<char>.Shared.Rent(span.Length);
        try
        {
            var w = 0;
            var newWord = true;

            foreach (var c in span)
            {
                if (c is '_' or '-' or ' ')
                {
                    newWord = true;
                    continue;
                }

                if (newWord)
                {
                    buffer[w++] = char.ToUpperInvariant(c);
                    newWord = false;
                }
                else
                {
                    buffer[w++] = char.ToLowerInvariant(c);
                }
            }

            if (w == 0) return string.Empty;
            buffer[0] = char.ToLowerInvariant(buffer[0]);
            return new string(buffer, 0, w);
        }
        finally
        {
            ArrayPool<char>.Shared.Return(buffer);
        }
    }
}