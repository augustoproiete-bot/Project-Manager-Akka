
using System;
using AkkaTest.Test;

namespace AkkaTest
{
    internal static class Program
    {
        private static void Main()
        {
            var test = Result.Create(10); //Maybe.Just(10);

            var result = from num in test
                         let num2 = num + 1
                         where num > 5
                         select num + num2;
            
        }
    }
}