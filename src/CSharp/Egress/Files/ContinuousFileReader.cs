using Egress.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Egress.Files
{
    public class ContinuousFileReader : IContentPoller
    {
        private readonly int _maximumLinesCountInEvent = 50;
        private readonly string _monitoredFileName;
        private readonly FileSystemWatcher _fileSystemWatcher;

        public ContinuousFileReader(string fileToMonitor, Int32 maximumLinesCountInEvent = 50)
        {
            if (fileToMonitor == null)
                throw new ArgumentNullException(nameof(fileToMonitor));

            _maximumLinesCountInEvent = maximumLinesCountInEvent;
            _monitoredFileName = new FileInfo(fileToMonitor).FullName;
            _checkpoint = ContinuousFileReaderCheckpoint.CreateFromMonitoredFileName(_monitoredFileName);

            _fileSystemWatcher = new FileSystemWatcher();
            _fileSystemWatcher.Path = Path.GetDirectoryName(_monitoredFileName);
            _fileSystemWatcher.Filter = Path.GetFileName(_monitoredFileName);
            _fileSystemWatcher.NotifyFilter = NotifyFilters.LastWrite;
            _fileSystemWatcher.Changed += FileChanged;
        }

        public event EventHandler<LinesPolledEventArgs>? ContentChanged;

        protected void OnContentChanged(string[] newLines)
        {
            ContentChanged?.Invoke(this, new LinesPolledEventArgs(newLines));
        }

        private Int32 _readGate = 0;
        private readonly ContinuousFileReaderCheckpoint _checkpoint;

        /// <summary>
        /// Try reading the file, generating event if something new happens.
        /// </summary>
        public void Read()
        {
            if (Interlocked.CompareExchange(ref _readGate, 1, 0) == 0)
            {
                try
                {
                    OnRead();
                }
                catch (Exception ex)
                {
                    //TODO: Handle error, retry, wait, etc.
                    throw;
                }
                finally
                {
                    Interlocked.Exchange(ref _readGate, 0);
                }
            }
        }

        private void OnRead()
        {
            List<String>? linesBuffer = null;
            if (!File.Exists(_monitoredFileName))
            {
                return;
            }

            using var fr = new FileStream(_monitoredFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 2048);

            if (_checkpoint.Position > 0)
            {
                //we have a valid checkpoint position, we just need to know if the first line is changed to handle
                //some tools that completely overwrite the file.
                var peekStreamReader = new StreamReader(fr);
                var firstLine = peekStreamReader.ReadLine();
                if (String.IsNullOrEmpty(firstLine))
                {
                    return; // file is empty, it was deleted and zeroed out ....
                }

                var actualHash = HashHelper.ToSha1(firstLine);
                if (actualHash != _checkpoint.FirstLineHash)
                {
                    _checkpoint.ResetPosition(actualHash); //file completely changed
                }

                //now seek in position.
                fr.Seek(_checkpoint.Position, SeekOrigin.Begin);
            }

            var sr = new StreamReader(fr);
            //need to read block by block until we finish the content, then we can sleep.
            //we can simply read line by line with a stream reader.
            string? line = null;
            while ((line = sr.ReadLine()) != null)
            {
                if (_checkpoint.Position == 0)
                {
                    //this is the very first line read, we need to grab the hash to check if file is overwritten
                    _checkpoint.ResetPosition(HashHelper.ToSha1(line));
                }
                if (linesBuffer == null)
                {
                    linesBuffer = new List<string>();
                }
                linesBuffer.Add(line);
                if (linesBuffer.Count == _maximumLinesCountInEvent) 
                {
                    //Avoid raising events with too much content.
                    RaiseContentChangedEventAndUpdateCheckpoint(linesBuffer, fr);
                }
            }

            RaiseContentChangedEventAndUpdateCheckpoint(linesBuffer, fr);
        }

        private void RaiseContentChangedEventAndUpdateCheckpoint(List<string>? linesBuffer, FileStream fr)
        {
            if (linesBuffer?.Count > 0)
            {
                OnContentChanged(linesBuffer.ToArray());
                _checkpoint.Write(fr.Position);
                linesBuffer.Clear();
            }
        }

        public Task StartMonitoringAsync()
        {
            // start monitor the file, then read immediately on another thread
            _fileSystemWatcher.EnableRaisingEvents = true;

            return Task.Factory.StartNew(() => Read());
        }

        private void FileChanged(object sender, FileSystemEventArgs e)
        {
            Read();
        }

        public void StopMonitoring()
        {
            if (_fileSystemWatcher != null)
            {
                _fileSystemWatcher.EnableRaisingEvents = false;
                _fileSystemWatcher.Dispose();
            }
        }
    }
}
