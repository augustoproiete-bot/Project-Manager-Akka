using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Tauron.Application.Wpf.UI
{
    public sealed class ViewManager
    {
        //private readonly Dictionary<string, Dictionary<long, IView>> _views = new Dictionary<string, Dictionary<long, IView>>();
        private ImmutableDictionary<string, ViewConnector> _models = ImmutableDictionary<string, ViewConnector>.Empty;

        public static ViewManager Manager { get; } = new ViewManager();

        private ViewManager()
        {
            
        }

        public void RegisterConnector(string key, ViewConnector connector)
        {
            if (!ImmutableInterlocked.TryAdd(ref _models, key, connector))
                throw new InvalidOperationException("Key is Already Set: " + key);
        }

        public void UnregisterConnector(string key)
            => ImmutableInterlocked.TryRemove(ref _models, key, out _);

        //public void Register(IViewModel model, IView view, IView root)
        //{
        //    lock (_views)
        //    {
        //        if (!model.IsInitialized)
        //            return;

        //        if (!_views.TryGetValue(root.Key, out var rootViews))
        //        {
        //            rootViews = new Dictionary<long, IView>();
        //            _views[root.Key] = rootViews;
        //        }

        //        var id = model.Actor.Path.Uid;
        //        if (rootViews.ContainsKey(id))
        //            throw new InvalidOperationException("Model is Registrated");

        //        rootViews[id] = view;

        //        void OnViewOnControlUnload()
        //        {
        //            rootViews.Remove(id);
        //            view.ControlUnload -= OnViewOnControlUnload;
        //        }

        //        view.ControlUnload += OnViewOnControlUnload;

        //        //_models[view.Key] = model;
        //    }
        //}

        //public IView? Get(IViewModel model, IView root)
        //{
        //    if(_views.TryGetValue(root.Key, out var views))
        //        return !model.IsInitialized ? null : views.GetValueOrDefault(model.Actor.Path.Uid);
        //    return null;
        //}
    }
}