using System;
using System.Threading;
using Serilog;

namespace Tauron.Host
{
    public class ApplicationLifetime : IHostApplicationLifetime, IApplicationLifetime, IDisposable
    {
        private readonly CancellationTokenSource _startedSource = new CancellationTokenSource();

        private readonly CancellationTokenSource _stoppingSource = new CancellationTokenSource();

        private readonly CancellationTokenSource _stoppedSource = new CancellationTokenSource();

        private readonly ILogger _logger = Log.ForContext<ApplicationLifetime>();

        public CancellationToken ApplicationStarted => _startedSource.Token;

        public CancellationToken ApplicationStopping => _stoppingSource.Token;

        public CancellationToken ApplicationStopped => _stoppedSource.Token;

        public void StopApplication()
        {
            lock (_stoppingSource)
            {
                try
                {
                    ExecuteHandlers(_stoppingSource);
                }
                catch (Exception exception)
                {
                    _logger.Error(exception, "An error occurred stopping the application");
                }
            }
        }

        public void NotifyStarted()
        {
            try
            {
                ExecuteHandlers(_startedSource);
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "An error occurred starting the application");
            }
        }

        public void NotifyStopped()
        {
            try
            {
                ExecuteHandlers(_stoppedSource);
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "An error occurred stopping the application");
            }
        }

        private void ExecuteHandlers(CancellationTokenSource cancel)
        {
            if (!cancel.IsCancellationRequested) 
                cancel.Cancel(false);
        }

        public void Shutdown(int exitCode)
        {
            Environment.ExitCode = exitCode;
            StopApplication();
        }

        public void Dispose()
        {
            _startedSource.Dispose();
            _stoppingSource.Dispose();
            _stoppedSource.Dispose();
        }
    }
}