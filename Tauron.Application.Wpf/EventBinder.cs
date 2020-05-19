﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using JetBrains.Annotations;
using Serilog;
using Tauron.Akka;
using Tauron.Application.Wpf.Commands;
using Tauron.Application.Wpf.Helper;
using Tauron.Application.Wpf.ModelMessages;

namespace Tauron.Application.Wpf
{
    [PublicAPI]
    public static class EventBinder
    {
        private const string EventBinderPrefix = "EventBinder.";

        public static readonly DependencyProperty EventsProperty =
            DependencyProperty.RegisterAttached("Events", typeof(string), typeof(EventBinder), new UIPropertyMetadata(null, OnEventsChanged));

        [NotNull]
        public static string GetEvents(DependencyObject obj) => (string) Argument.NotNull(obj, nameof(obj)).GetValue(EventsProperty);

        public static void SetEvents(DependencyObject obj, string value)
        {
            Argument.NotNull(obj, nameof(obj)).SetValue(EventsProperty, Argument.NotNull(value, nameof(value)));
        }

        private static void OnEventsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(d)) return;

            var root = ControlBindLogic.FindRoot(d);
            if (root == null)
            {
                if (d is FrameworkElement element)
                    ControlBindLogic.MakeLazy(element, e.NewValue as string, e.OldValue as string, BindInternal);
                return;
            }

            BindInternal(e.OldValue as string, e.NewValue as string, root, d);
        }

        private static void BindInternal(string? oldValue, string? newValue, IBinderControllable binder, DependencyObject affectedPart)
        {
            if (oldValue != null)
                binder.CleanUp(EventBinderPrefix + oldValue);

            if (newValue == null) return;

            binder.Register(EventBinderPrefix + newValue, new EventLinker {Commands = newValue}, affectedPart);
        }

        private sealed class EventLinker : ControlBindableBase
        {
            private readonly List<InternalEventLinker> _linkers = new List<InternalEventLinker>();

            public string? Commands { get; set; }

            protected override void CleanUp()
            {
                foreach (var linker in _linkers) linker.Dispose();
                _linkers.Clear();
            }

            protected override void Bind(object context)
            {
                if (Commands == null)
                {
                    Log.Logger.Error("EventBinder: No Command Setted: {Context}", context ?? "Unkowen");

                    return;
                }

                var vals = Commands.Split(':');
                var events = new Dictionary<string, string>();

                try
                {
                    for (var i = 0; i < vals.Length; i++) events[vals[i]] = vals[++i];
                }
                catch (IndexOutOfRangeException)
                {
                    Log.Logger.Error("EventBinder: EventPairs not Valid: {Commands}", Commands);
                }

                if (context == null) return;

                var dataContext = context as IViewModel;
                var host = AffectedObject;
                if (host == null || dataContext == null) return;

                var hostType = host.GetType();

                foreach (var pair in events)
                {
                    var info = hostType.GetEvent(pair.Key);
                    if (info == null)
                    {
                        Log.Logger.Error("EventBinder: No event Found: {HostType}|{Key}", hostType, pair.Key);
                        return;
                    }

                    _linkers.Add(new InternalEventLinker(info, dataContext, pair.Value, host));
                }
            }


            private class InternalEventLinker : IDisposable
            {
                private static readonly MethodInfo Method = typeof(InternalEventLinker).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                   .First(m => m.Name == "Handler");

                private readonly IViewModel _dataContext;

                private readonly EventInfo? _event;
                private readonly DependencyObject? _host;
                private readonly string _targetName;
                private Delegate? _delegate;
                private Action<EventData>? _command;
                private bool _isDirty;

                public InternalEventLinker(EventInfo? @event, IViewModel dataContext,  string targetName, DependencyObject? host)
                {
                    _isDirty = (@event == null);

                    _host = host;
                    _event = @event;
                    _dataContext = dataContext;
                    _targetName = targetName;

                    Initialize();
                }

                public void Dispose()
                {
                    if (_host == null || _delegate == null) return;

                    _event?.RemoveEventHandler(_host, _delegate);
                    _delegate = null;
                }

                private bool EnsureCommandStade()
                {
                    if (_command != null) return true;

                    if (_dataContext == null) return false;

                    _command = d => _dataContext.Tell(new ExecuteEventExent(d, _targetName));


                    return _command != null && !_isDirty;
                }


                [UsedImplicitly]
                private void Handler(object sender, EventArgs e)
                {
                    if (!_isDirty && !EnsureCommandStade())
                    {
                        Dispose();
                        return;
                    }

                    try
                    {
                        _command?.Invoke(new EventData(sender, e));
                    }
                    catch (ArgumentException)
                    {
                        _isDirty = true;
                    }
                }

                private void Initialize()
                {
                    if (_isDirty || _event == null) return;

                    var eventTyp = _event?.EventHandlerType;
                    if (_host == null || eventTyp == null) return;

                    _delegate = Delegate.CreateDelegate(eventTyp, this, Method);
                    _event?.AddEventHandler(_host, _delegate);
                }
            }
        }
    }
}