using System;
using System.Collections.Generic;

namespace ServiceManager.ProjectRepository.Core
{
    public abstract class SharedObject<TObject> : IDisposable
        where TObject : SharedObject<TObject>, new()
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

        protected static readonly object Lock = new object();

        private static readonly Dictionary<RepositoryConfiguration, ObjectEntry> SharedObjects = new Dictionary<RepositoryConfiguration, ObjectEntry>();

        public static TObject GetOrNew(RepositoryConfiguration configuration)
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
        
        protected RepositoryConfiguration Configuration
        {
            get => _configuration ?? new RepositoryConfiguration();
            private set => _configuration = value;
        }

        protected void LogMessage(string msg, params object[] parms) => Configuration.Logger?.Invoke(msg, parms);

        private bool _disposed;
        private RepositoryConfiguration? _configuration;

        protected virtual void Init(RepositoryConfiguration configuration) => Configuration = configuration; 

        protected virtual void InternalDispose()
        {

        }

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