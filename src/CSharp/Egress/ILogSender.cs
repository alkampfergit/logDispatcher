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
    }
}
