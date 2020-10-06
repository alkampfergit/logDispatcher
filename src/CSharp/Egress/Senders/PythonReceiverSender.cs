using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace Egress.Senders
{
    public class PythonReceiverSender : ILogSender
    {
        public Uri DestinationAddress { get; private set; }

        public PythonReceiverSender(string destinationAddress)
        {
            DestinationAddress = new Uri(destinationAddress);
        }

        public Task SendAsync(string destination, string log)
        {
            var jsonData = $"{{\"destination\":\"{destination}\",\"logs\":[{log}]}}";
            return SendPayload(jsonData);
        }

        public Task SendManyAsync(string destination, IEnumerable<string> logs)
        {
            if (logs.Any()) 
            {
                StringBuilder sb = new StringBuilder(8192);
                sb.Append($"{{\"destination\":\"{destination}\",\"logs\":[");
                foreach (var log in logs)
                {
                    sb.Append(log);
                    sb.Append(",");
                }
                sb.Length -= 1;
                sb.Append("]}");
                return SendPayload(sb.ToString()); 
            }

            return Task.CompletedTask;
        }

        private async Task SendPayload(string payload)
        {
            var binaryPayload = Encoding.UTF8.GetBytes(payload);
            using (var wc = new WebClient())
            {
                wc.Headers[HttpRequestHeader.ContentType] = "application/json";
                await wc.UploadDataTaskAsync(DestinationAddress, binaryPayload).ConfigureAwait(false);
            }
        }
    }
}
