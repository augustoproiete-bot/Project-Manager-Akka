using System;
using System.IO;
using System.Threading;
using JetBrains.Annotations;
using ServiceManager.ProjectRepository.Core;

namespace ServiceManager.ProjectRepository.Old
{
    [PublicAPI]
    public sealed class ClusterRepository : SharedObject<ClusterRepository>
    {
        private Mutex? _lock;

        private UnpackManager _unpacker = null!;
        private GitUpdater _updater = null!;

        public ProjectFinder ProjectFinder { get; } = new ProjectFinder();

        protected override void Init(RepositoryConfiguration configuration)
        {
            string mutexId = RepositoryConfiguration.RepositoryLockName + configuration.SourcePath.Replace(Path.DirectorySeparatorChar, '_');

            _lock = new Mutex(false, mutexId);

            try
            {
                // note, you may want to time out here instead of waiting forever
                // edited by acidzombie24
                // mutex.WaitOne(Timeout.Infinite, false);
                var hasHandle = _lock.WaitOne(TimeSpan.FromMinutes(2), false);
                if (hasHandle == false)
                    throw new TimeoutException("Timeout waiting for exclusive access");
            }
            catch (AbandonedMutexException)
            {
            }

            _unpacker = UnpackManager.GetOrNew(configuration);
            _updater = GitUpdater.GetOrNew(configuration);

            base.Init(configuration);
        }

        public string PrepareRepository(string trackingId)
        {
            var repoSource = _unpacker.UnpackRepo(trackingId);

            var source = _updater.RunUpdate(repoSource);

            ProjectFinder.Init(source, Configuration.Solotion, Configuration.Logger ?? ((s, objects) => { }));

            return source;
        }

        protected override void InternalDispose()
        {
            _updater.Dispose();
            _unpacker.Dispose();

            _lock?.ReleaseMutex();
            _lock?.Dispose();
            base.InternalDispose();
        }
    }
}