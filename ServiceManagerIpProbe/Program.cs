using System;
using System.Threading;

namespace ServiceManagerIpProbe
{
    class Program
    {
        [STAThread]
        static void Main()
        {
            Console.Title = "Host Installer";

            Action destroySelf = () => { };

            try
            {
                using (var context = new OperationContext())
                {
                    Operation.Start(context);
                    destroySelf = context.DestroySelf;

                    context.GlobalTimeout.Token.ThrowIfCancellationRequested();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error:");
                Console.WriteLine(e);
            }
            
            Thread.Sleep(4000);

            destroySelf();
        }
    }
}
