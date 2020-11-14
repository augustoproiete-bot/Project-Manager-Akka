
using System;
using LanguageExt;

namespace AkkaTest
{
    internal static class Program
    {
        private static void Main()
        {
            //https://yoan-thirion.medium.com/functional-programming-made-easy-in-c-with-language-ext-c4e9d4a512ac
            //https://github.com/louthy/language-ext

            string? test = Console.ReadLine();

            test.ToTryOption()
        }
    }
}