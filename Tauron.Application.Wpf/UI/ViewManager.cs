using System.Collections.Concurrent;

namespace Tauron.Application.Wpf.UI
{
    public static class ViewManager
    {
        private sealed class ModelViewPair
        {
            public IViewModel Model { get; }

            public IView View { get; }

            public ModelViewPair(IViewModel model, IView view)
            {
                Model = model;
                View = view;
            }
        }

        private static ConcurrentDictionary<string, ModelViewPair> _modelViewPairs = new ConcurrentDictionary<string, ModelViewPair>();
    }
}