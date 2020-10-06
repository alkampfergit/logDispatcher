using Egress.Files;
using Egress.Helpers;
using Egress.Parsers;
using NUnit.Framework;
using System.Collections.Generic;

namespace Egress.Tests.Helpers
{
    [TestFixture]
    public class ComponentFactoryTests
    {
        [Test]
        public void Verify_basic_construction()
        {
            var fileReader = ComponentFactory.Create(
                "Egress.Files.ContinuousFileReader, Egress",
                new Dictionary<string, object>()
                {
                    ["fileToMonitor"] = "X:\\Docker\\Cowrie\\Logs\\cowrie.json"
                });

            Assert.That(fileReader, Is.Not.Null);
            Assert.That(fileReader, Is.InstanceOf<ContinuousFileReader>());
        }

        [Test]
        public void Can_create_component_without_parameters()
        {
            var fileReader = ComponentFactory.Create(
                "Egress.Parsers.JsonParser, Egress",
                null);

            Assert.That(fileReader, Is.Not.Null);
            Assert.That(fileReader, Is.InstanceOf<JsonParser>());
        }
    }
}
