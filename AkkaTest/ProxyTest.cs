using System.Diagnostics;
using Castle.DynamicProxy;

namespace AkkaTest
{
    public static class ProxyTest
    {
        private class Interceptor : IInterceptor
        {
            public void Intercept(IInvocation invocation)
            {
                if (invocation.Method.ReturnType == typeof(void))
                    return;

                invocation.ReturnValue = invocation.Method.GetParameters().Length == 0 ? string.Empty : invocation.GetArgumentValue(0);
            }
        }

        public interface IGreter
        {
            string Hallo(string test);

            void Hello2();

            string Hello3(string test);
        }

        public static void TestProxy()
        {
            var builder = new ProxyGenerator(new DefaultProxyBuilder(new ModuleScope(true)));

            var timeMes = Stopwatch.StartNew();

            var test = (IGreter)builder.CreateInterfaceProxyWithoutTarget(typeof(IGreter), new Interceptor());
            string time = timeMes.Elapsed.ToString();

            var result = test.Hallo("Hallo");
        }
    }
}