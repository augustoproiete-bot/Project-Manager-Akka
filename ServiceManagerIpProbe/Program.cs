using System;
using System.IO;
using ServiceManagerIpProbe.Phase;
using ServiceManagerIpProbe.UI;

namespace ServiceManagerIpProbe
{
    class Program
    {
        [STAThread]
        static void Main()
        {
            Action destroySelf = () => { };

            using (var testcontext = new OperationContext(s => {}))
            {
                var testPhase = new SelfDestroyPhase();
                testPhase.Run(testcontext, null);
                destroySelf = testcontext.DestroySelf;
            }

            UIStart.Run(log =>
            {
                using (var context = new OperationContext(log))
                {
                    try
                    {
                        Operation.Start(context);
                        destroySelf = context.DestroySelf;

                        context.GlobalTimeout.Token.ThrowIfCancellationRequested();

                    }
                    catch (Exception e)
                    {
                        context.WriteLine("Error:");
                        context.WriteLine(e.ToString());

                        using (var stream = new StreamWriter(new FileStream("errors.txt", FileMode.OpenOrCreate, FileAccess.Write)))
                            stream.WriteLine(e.ToString());
                    }
                }
            });

            destroySelf();
            Environment.Exit(0);
        }
    }
}
