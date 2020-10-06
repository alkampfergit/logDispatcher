using Egress.Parsers;
using NUnit.Framework;
using System.Collections.Generic;

namespace Egress.Tests.Parsers
{
    [TestFixture]
    public class JsonParserTests
    {
        [TestCase("", "")]
        [TestCase(null, "")]
        public void Can_tolerate_null_string(string json, string expected)
        {
            var sut = new JsonParser();
            var parsed = sut.Parse("source1", json);
            Assert.That(parsed, Is.EqualTo(expected));
        }

        [Test]
        public void Verify_basic_parse()
        {
            var sut = new JsonParser();
            var parsed = sut.Parse("source1", "{'log' : 'blah'}");
            Assert.That(parsed, Is.EqualTo("{\"log\":\"blah\",\"source\":\"source1\"}"));
        }

        [Test]
        public void Can_filter_out_properties()
        {
            var sut = new JsonParser();
            sut.PropertiesToRemove = new HashSet<string>() { "log2" };
            var parsed = sut.Parse("source1", "{'log' : 'blah', 'log2' : 'bloh'}");
            Assert.That(parsed, Is.EqualTo("{\"log\":\"blah\",\"source\":\"source1\"}"));
        }

        [Test]
        public void Filtering_properties_that_does_not_exists()
        {
            var sut = new JsonParser();
            sut.PropertiesToRemove = new HashSet<string>() { "log2", "unexistingproperties" };
            var parsed = sut.Parse("source1", "{'log' : 'blah', 'log2' : 'bloh'}");
            Assert.That(parsed, Is.EqualTo("{\"log\":\"blah\",\"source\":\"source1\"}"));
        }

        [Test]
        public void Adding_properties()
        {
            var sut = new JsonParser();
            sut.PropertiesToRemove = new HashSet<string>() { "log2", "unexistingproperties" };
            sut.PropertiesToAdd = new Dictionary<string, object>()
            {
                ["other"] = 3
            };
            var parsed = sut.Parse("source1", "{'log' : 'blah'}");
            Assert.That(parsed, Is.EqualTo("{\"log\":\"blah\",\"source\":\"source1\",\"other\":3}"));
        }
    }
}
