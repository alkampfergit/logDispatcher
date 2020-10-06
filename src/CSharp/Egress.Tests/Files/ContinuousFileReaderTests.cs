using Egress.Files;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Egress.Tests.Files
{
    [TestFixture]
    public class ContinuousFileReaderTests
    {
        private string _testFileName;

        private readonly List<String> _readLines = new List<string>();

        [SetUp]
        public void SetUp()
        {
            _testFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()) + ".log";
            _readLines.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(_testFileName))
            {
                File.Delete(_testFileName);
            }

            var checkpointFiles = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.checkpoint");
            foreach (var file in checkpointFiles)
            {
                File.Delete(file);
            }
        }

        [Test]
        public void Can_read_existing_files()
        {
            File.WriteAllText(_testFileName, "Hello world\n");
            ContinuousFileReader sut = CreateSut();

            sut.Read();
            //we expect something to be generated
            Assert.That(_readLines, Is.EquivalentTo(new[] { "Hello world" }));
        }

        [Test]
        public void Can_manually_poll_file()
        {
            File.WriteAllText(_testFileName, "Hello world\n");
            ContinuousFileReader sut = CreateSut();

            sut.Read();
            //we expect something to be generated
            Assert.That(_readLines, Is.EquivalentTo(new[] { "Hello world" }));

            _readLines.Clear();
            File.AppendAllLines(_testFileName, new[] { "Another Line" });
            sut.Read();
            Assert.That(_readLines, Is.EquivalentTo(new[] { "Another Line" }));
        }

        [Test]
        public void Can_monitor_file()
        {
            File.WriteAllText(_testFileName, "Hello world\n");
            ContinuousFileReader sut = CreateSut();

            sut.StartMonitoring();

            //we expect first version of file to be generated
            Assert.That(_readLines, Is.EquivalentTo(new[] { "Hello world" }));

            //now we add some content, we expect the file to be updated even without read.
            _readLines.Clear();
            File.AppendAllLines(_testFileName, new[] { "Another Line" });

            //watcher is not instantaneous, we need to wait for a small amount of time for the data to be there.
            WaitForNumberOfRowToBeRead(1);

            Assert.That(_readLines, Is.EquivalentTo(new[] { "Another Line" }));
        }

        [Test]
        public void Monitoring_file_can_be_stopped()
        {
            File.WriteAllText(_testFileName, "Hello world\n");
            ContinuousFileReader sut = CreateSut();

            sut.StartMonitoring();

            //we expect first version of file to be generated
            Assert.That(_readLines, Is.EquivalentTo(new[] { "Hello world" }));
            //now we add some content, we expect the file to be updated even without read.
            _readLines.Clear();
            File.AppendAllLines(_testFileName, new[] { "Another Line" });

            //watcher is not instantaneous, we need to wait for a small amount of time for the data to be there.
            WaitForNumberOfRowToBeRead(1);
            sut.StopMonitoring();

            File.AppendAllLines(_testFileName, new[] { "The third Line" });

            Thread.Sleep(1000); //give eventually time to filesystem watcher to kick in.
            Assert.That(_readLines, Is.EquivalentTo(new[] { "Another Line" }));
        }

        [Test]
        public void Remove_checkpoint_when_file_does_not_exists()
        {
            File.WriteAllText(_testFileName, "Hello world\n");
            ContinuousFileReader sut = CreateSut();

            sut.Read();
            //we expect checkpoint to be created
            Assert.That(Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.checkpoint").Length, Is.GreaterThan(0));

            //call cleanup, nothing was to be cleaned.
            ContinuousFileReaderHelper.Cleanup();
            Assert.That(Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.checkpoint").Length, Is.GreaterThan(0));

            //ok now we want to cleanup all checkpoint file, delete the file than call cleanup 
            File.Delete(_testFileName);
            ContinuousFileReaderHelper.Cleanup();
            Assert.That(Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.checkpoint").Length, Is.EqualTo(0));
        }

        [Test]
        public void Can_persist_seek_data()
        {
            File.WriteAllText(_testFileName, "Hello world\n");
            ContinuousFileReader sut = CreateSut();

            sut.Read();
            _readLines.Clear();

            //now create another reader, this should be able to restart from the old position
            ContinuousFileReader anotherSut = CreateSut();
            File.AppendAllLines(_testFileName, new[] { "Another Line" });
            anotherSut.Read();
            Assert.That(_readLines, Is.EquivalentTo(new[] { "Another Line" }));
        }

        [Test]
        public void Does_nothing_if_file_does_not_exists()
        {
            ContinuousFileReader sut = CreateSut();

            sut.Read();
            //we expect something to be generated
            Assert.That(_readLines, Is.EquivalentTo(Array.Empty<String>()));
        }

        private ContinuousFileReader CreateSut()
        {
            var sut = new ContinuousFileReader(_testFileName);
            sut.ContentChanged += Sut_ContentChanged;
            return sut;
        }

        private void Sut_ContentChanged(object sender, LinesPolledEventArgs e)
        {
            _readLines.AddRange(e.NewLines);
        }

        private void WaitForNumberOfRowToBeRead(Int32 rowCount)
        {
            Stopwatch sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < 2000 && _readLines.Count != rowCount)
            {
                Thread.Sleep(50);
            }
            sw.Stop();
        }
    }
}
