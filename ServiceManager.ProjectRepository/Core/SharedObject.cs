using System;
using System.Collections.Generic;

namespace ServiceManager.ProjectRepository.Core
{
    public abstract class SharedObject<TObject, TConfiguration> : IDisposable
        where TObject : SharedObject<TObject, TConfiguration>, new()
        where TConfiguration : class, IReporterProvider, new()
    {
        private sealed class ObjectEntry
        {
            public int Count { get; set; }

            public TObject SharedObject { get; }

            public ObjectEntry(TObject sharedObject)
            {
                SharedObject = sharedObject;
                Count = 1;
            }
        }

        // ReSharper disable once StaticMemberInGenericType
        protected static readonly object Lock = new object();

        private static readonly Dictionary<TConfiguration, ObjectEntry> SharedObjects = new Dictionary<TConfiguration, ObjectEntry>();

        public static TObject GetOrNew(TConfiguration configuration)
        {
            lock (Lock)
            {
                if (SharedObjects.TryGetValue(configuration, out var obj))
                {
                    obj.Count++;
                    return obj.SharedObject;
                }
                
                var sharedObject = new TObject();
                sharedObject.Init(configuration);

                SharedObjects[configuration] = new ObjectEntry(sharedObject);

                return sharedObject;

            }
        }

        protected void SendMessage(string msg) => Configuration.SendMessage(msg);

        protected TConfiguration Configuration
        {
            get => _configuration ?? new TConfiguration();
            private set => _configuration = value;
        }
        
        private bool _disposed;
        private TConfiguration? _configuration;

        protected virtual void Init(TConfiguration configuration) => Configuration = configuration; 

        protected virtual void InternalDispose() { }

        public void Dispose()
        {
            if(_disposed) return;
            lock (Lock)
            {

                var target = SharedObjects[Configuration];
                target.Count--;

                if(target.Count != 0) return;

                _disposed = true;
                SharedObjects.Remove(Configuration);
                InternalDispose();
                GC.SuppressFinalize(this);
            }
        }

        ~SharedObject()
        {Dispose();}
    }
}