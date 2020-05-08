using System;
using Autofac;

namespace AkkaTest
{
    public class Test : IDisposable
    {
        private readonly string _content;

        public Test(string content) 
            => _content = content;

        public void Dispose() 
            => Console.Write(_content);
    }

    public static class AutoFacTest
    {
        public static void TestAutofac()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<Test>().AsSelf();

            using var container = builder.Build();

            using (var test = container.BeginLifetimeScope())
            {
                var one = test.Resolve<Test>(TypedParameter.From("Hallo "));
                var two = test.Resolve<Test>(TypedParameter.From("Welt"));
            }

            Console.WriteLine();
        }
    }
}