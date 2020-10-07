using System;
using System.Threading.Tasks;

namespace Egress.Files
{
    public class LogDirectoryPoller : IContentPoller
    {
        private readonly string _monitoredDirectory;

        public LogDirectoryPoller(string monitoredDirectory)
        {
            _monitoredDirectory = monitoredDirectory;
        }

        public event EventHandler<LinesPolledEventArgs> ContentChanged;

        public void StartMonitoring()
        {
            throw new NotImplementedException();
        }

        public Task StartMonitoringAsync()
        {
            throw new NotImplementedException();
        }

        public void StopMonitoring()
        {
            throw new NotImplementedException();
        }

        protected void OnContentChanged(string[] newLines)
        {
            ContentChanged?.Invoke(this, new LinesPolledEventArgs(newLines));
        }
    }
}
