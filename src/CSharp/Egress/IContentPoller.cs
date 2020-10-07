using System;
using System.Threading.Tasks;

namespace Egress
{
    public interface IContentPoller
    {
        event EventHandler<LinesPolledEventArgs> ContentChanged;

        /// <summary>
        /// Starts monitoring the files, and return a Task that will be 
        /// completed when the first read was executed. It is useful
        /// mainly for testing purpose.
        /// </summary>
        /// <returns></returns>
        Task StartMonitoringAsync();

        void StopMonitoring();
    }
}
