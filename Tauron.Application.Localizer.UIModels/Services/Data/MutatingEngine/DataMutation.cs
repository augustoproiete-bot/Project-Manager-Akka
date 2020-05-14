using System;

namespace Tauron.Application.Localizer.UIModels.Services.Data.MutatingEngine
{
    public sealed class DataMutation<TData>
    {
        public string Name { get; }

        public Func<TData, TData> Mutatuion { get; }

        public Func<TData> Receiver { get; }

        public Action<TData> Responder { get; }

        public DataMutation(Func<TData, TData> mutatuion, Func<TData> receiver, Action<TData> responder, string name)
        {
            Mutatuion = mutatuion;
            Receiver = receiver;
            Responder = responder;
            Name = name;
        }
    }
}