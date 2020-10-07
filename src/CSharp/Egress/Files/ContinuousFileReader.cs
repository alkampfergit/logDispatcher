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
        private readonly string _monitoredFileName;
        private FileSystemWatcher _fileSystemWatcher;

        public ContinuousFileReader(string fileToMonitor)
        {
            _monitoredFileName = new FileInfo(fileToMonitor).FullName;
            _checkpoint = ContinuousFileReaderCheckpoint.CreateFromMonitoredFileName(_monitoredFileName);
        }

        public event EventHandler<LinesPolledEventArgs> ContentChanged;

        protected void OnContentChanged(string[] newLines)
        {
            ContentChanged?.Invoke(this, new LinesPolledEventArgs(newLines));
        }

        private Int32 _readGate = 0;
        private ContinuousFileReaderCheckpoint _checkpoint;

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
            List<String> linesBuffer = null;
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
                    _checkpoint.ResetPosition( HashHelper.ToSha1(line));
                }
                if (linesBuffer == null)
                {
                    linesBuffer = new List<string>();
                }
                linesBuffer.Add(line);
            }

            if (linesBuffer?.Count > 0)
            {
                OnContentChanged(linesBuffer.ToArray());
                _checkpoint.Write(fr.Position);
            }
        }

        public Task StartMonitoringAsync()
        {
            // start monitor the file, then read immediately on another thread
            _fileSystemWatcher = new FileSystemWatcher();
            _fileSystemWatcher.Path = Path.GetDirectoryName(_monitoredFileName);
            _fileSystemWatcher.Filter = Path.GetFileName(_monitoredFileName);
            _fileSystemWatcher.NotifyFilter = NotifyFilters.LastWrite;
            _fileSystemWatcher.Changed += FileChanged;
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
            }
        }

        //private void WriteCheckpoint()
        //{
        //    var fileName = GetCheckpointFileName();
        //    File.WriteAllText(fileName, $"{_hashOfFirstLine}|{_lastReadPosition}|{_monitoredFileName}");
        //}

        //private string GetCheckpointFileName()
        //{
        //    return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.GetFileName(_monitoredFileName)) + ".checkpoint";
        //}

        ///// <summary>
        ///// Read the checkpoint and return the has of the first line to understand if the
        ///// file is really changed.
        ///// </summary>
        ///// <returns></returns>
        //private string ReadCheckpoint()
        //{
        //    var fileName = GetCheckpointFileName();
        //    if (File.Exists(fileName))
        //    {
        //        var content = File.ReadAllText(fileName);
        //        string[] splittedCheckpoint = content.Split('|');
        //        _lastReadPosition = int.Parse(splittedCheckpoint[1]);
        //        return splittedCheckpoint[0];
        //    }

        //    return String.Empty;
        //}
    }
}
