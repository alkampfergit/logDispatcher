using System.Collections.Generic;
using System.Threading.Tasks;

namespace Egress
{
    /// <summary>
    /// In our situation a log is mainly a json object that will be stored in 
    /// Mongodb directly or sending to the Python receiver.
    /// </summary>
    public interface ILogSender
    {
        /// <summary>
        /// Send a single log to the destination, the log is a single string
        /// serialized with json.
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        Task SendAsync(string destination, string log);

        /// <summary>
        /// Send a series of logs to the destination, all logs are simple serialized json string.
        /// </summary>
        /// <param name="logs"></param>
        /// <returns></returns>
        Task SendManyAsync(string destination, IEnumerable<string> logs);
    }
}
