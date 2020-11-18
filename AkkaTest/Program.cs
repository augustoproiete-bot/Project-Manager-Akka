using System;
using System.Collections.Generic;
using System.Linq;

namespace AkkaTest
{
    internal static class Program
    {
        private static void Main()
        {
            IEnumerable<object> test = new object[] {"Hallo Welt", 10, "Hallo Welt 2"};

            foreach (var te in from ele in test
                               where ele is string
                               select (string) ele)
                Console.WriteLine(te);
        }
    }
}