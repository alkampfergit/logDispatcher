using Egress.Configuration;
using Egress.Helpers;
using System;

namespace Egress
{
    public static class Program
    {
        private static IContentPoller _poller;
        private static IParser _parser;
        private static ILogSender _sender;

        private static void Main(string[] args)
        {
            //ok I want to create a configuration
            _poller = (IContentPoller)ComponentFactory.Create(MainConfiguration.Instance.Poller.Name, MainConfiguration.Instance.Poller.Configuration);
            _parser = (IParser)ComponentFactory.Create(MainConfiguration.Instance.Parser.Name, MainConfiguration.Instance.Parser.Configuration);
            _sender = (ILogSender)ComponentFactory.Create(MainConfiguration.Instance.Sender.Name, MainConfiguration.Instance.Sender.Configuration);

            //ok for this simple example we simple run until someone press a key to stop
            _poller.ContentChanged += ContentChanged;

            _poller.StartMonitoring();
            Console.WriteLine("Press a key to stop");
            Console.ReadKey();
            _poller.StopMonitoring();
        }

        private static void ContentChanged(object? _, LinesPolledEventArgs e)
        {
            foreach (var line in e.NewLines)
            {
                var logJson = _parser.Parse(MainConfiguration.Instance.SourceName, line);
                _sender.SendAsync(MainConfiguration.Instance.DestinationCollection, logJson).Wait();
            }
        }
    }
}
