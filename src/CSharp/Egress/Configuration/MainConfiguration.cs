using Microsoft.Extensions.Configuration;
using System;

namespace Egress.Configuration
{
    public class MainConfiguration
    {
        public static MainConfiguration Instance { get; private set; }

        static MainConfiguration() 
        {
            var builder = new ConfigurationBuilder();
            builder
               .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
               .AddJsonFile("config.json", optional: false, reloadOnChange: true);

            IConfiguration configuration = builder.Build();
            Instance = new MainConfiguration();
            configuration.Bind(Instance);
        }

        private MainConfiguration()
        {
        }

        public String DestinationCollection { get; set; }

        public String SourceName { get; set; }

        public ContentPollerConfiguration Poller { get; set; }

        public ParserConfiguration Parser { get; set; }

        public SenderConfiguration Sender { get; set; }
    }
}
