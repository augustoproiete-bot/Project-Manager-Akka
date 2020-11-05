using Tauron.Application.ServiceManager.Core.Model;
using Tauron.Application.Workshop.StateManagement;
using Tauron.Application.Workshop.StateManagement.Attributes;
using Tauron.Application.Wpf.Model;

namespace Tauron.Application.ServiceManager.Core.Managment.Models
{
    [Processor]
    public class SeedProcessor : ActorModel
    {
        private readonly CommonAppInfo _app;

        public SeedProcessor(IActionInvoker actionInvoker, CommonAppInfo app) 
            : base(actionInvoker)
        {
            _app = app;
        }
    }
}