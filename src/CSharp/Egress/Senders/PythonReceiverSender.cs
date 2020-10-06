using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Egress.Senders
{
    public class PythonReceiverSender : ILogSender
    {
        public Uri DestinationAddress { get; private set; }

        public PythonReceiverSender(string destinationAddress)
        {
            DestinationAddress = new Uri(destinationAddress);
        }

        public async Task SendAsync(string destination, string log) 
        {
            using (var wc = new WebClient()) 
            {
                var jsonData = $"{{\"destination\":\"{destination}\",\"logs\":[{log}]}}";
                var payload = Encoding.UTF8.GetBytes(jsonData);

                wc.Headers[HttpRequestHeader.ContentType] = "application/json";
                await wc.UploadDataTaskAsync(DestinationAddress, payload).ConfigureAwait(false);
            }
        }
    }
}
