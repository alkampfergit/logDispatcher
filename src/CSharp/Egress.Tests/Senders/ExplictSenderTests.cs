using Egress.Senders;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Egress.Tests.Senders
{
    [TestFixture]
    public class ExplictSenderTests
    {   
        [Test] // Uncomment to run the test, this is not supposed to run in a normal test run
        public async Task Send_to_local_receiver()
        {
            var client = new PythonReceiverSender("http://localhost:3000/upload");
            await client.SendAsync("DestCollection", "{\"log\":\"te♀st\"}").ConfigureAwait(false);
        }
    }
}
