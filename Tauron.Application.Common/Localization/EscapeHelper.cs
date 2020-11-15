using System.Collections.Generic;
using System.Text;
using Functional.Maybe;
using JetBrains.Annotations;
using static Tauron.Preload;

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

            private static Maybe<string> GetPartforChar(char @char) 
                => Parts.FirstMaybe(p => p.Value == @char).Select(c => c.Key);

            private static Maybe<char> GetPartforSequence(string @char) 
                => Parts.Lookup(@char);

            public static string Encode(IEnumerable<char> toEncode)
            {
                var builder = new StringBuilder();
                foreach (var @char in toEncode)
                {
                    Match(GetPartforChar(@char), 
                        c => builder.Append(EscapeStart, 2).Append(c), 
                        () => builder.Append(@char));
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
                        var temp1 = temp;
                        var sequence1 = sequence;
                        Match(part, p => builder.Append(p), () => builder.Append(temp1).Append(sequence1));
                        
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