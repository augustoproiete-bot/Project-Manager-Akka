
using System;
using Functional.Maybe;
using Newtonsoft.Json;
using Tauron;

namespace AkkaTest
{
    internal static class Program
    {
        private static void Main()
        {
            var test = "Hallo Welt".ToMaybe();

            var testData = JsonConvert.SerializeObject(test);
            test = JsonConvert.DeserializeObject<Maybe<string>>(testData);
        }
    }
}