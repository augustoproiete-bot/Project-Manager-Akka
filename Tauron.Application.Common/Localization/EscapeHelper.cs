using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace Tauron.Localization
{
    [PublicAPI]
    public class EscapeHelper
    {
        private static class Coder
        {
            private static readonly Dictionary<string, char> Parts
                = new()
                {
                    {"001", '\r'},
                    {"002", '\t'},
                    {"003", '\n'},
                    {"004", ':'}
                };

            private const char EscapeStart = '@';

            private static string? GetPartforChar(char @char) => Parts.FirstOrDefault(ep => ep.Value == @char).Key;

            private static char? GetPartforSequence(string @char)
            {
                if (Parts.TryGetValue(@char, out var escape))
                    return escape;

                return null;
            }

            public static string Encode(IEnumerable<char> toEncode)
            {
                var builder = new StringBuilder();
                foreach (var @char in toEncode)
                {
                    var part = GetPartforChar(@char);
                    if (part == null) builder.Append(@char);
                    else builder.Append(EscapeStart, 2).Append(part);
                }

                return builder.ToString();
            }

            public static string Decode(IEnumerable<char> toDecode)
            {
                var builder = new StringBuilder();

                var flag = false;
                var flag2 = false;
                var pos = 0;
                var sequence = string.Empty;
                var temp = string.Empty;

                foreach (var @char in toDecode)
                {
                    if (flag2)
                    {
                        sequence += @char;
                        pos++;

                        if (pos != 3) continue;

                        var part = GetPartforSequence(sequence);
                        if (part == null) builder.Append(temp).Append(sequence);
                        else builder.Append(part);

                        flag = false;
                        flag2 = false;
                        pos = 0;
                        sequence = string.Empty;
                        temp = string.Empty;
                    }
                    else if (flag)
                    {
                        flag2 = @char == EscapeStart;
                        if (!flag2)
                        {
                            builder.Append(EscapeStart).Append(@char);
                            flag = false;
                        }
                        else temp += @char;
                    }
                    else
                    {
                        flag = @char == EscapeStart;

                        if (!flag)
                        {
                            builder.Append(@char);
                        }
                        else temp += @char;
                    }
                }

                return builder.ToString();
            }
        }

        public static string Ecode(string input)
            => Coder.Encode(input);

        public static string Decode(string input)
            => Coder.Decode(input);
    }
}