using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Egress.Files
{
    public class ContinuousFileReader
    {
        private readonly string _monitoredFileName;
        private FileSystemWatcher _fileSystemWatcher;

        public ContinuousFileReader(string fileToMonitor)
        {
            _monitoredFileName = new FileInfo(fileToMonitor).FullName;
            ReadCheckpoint();
        }

        public event EventHandler<ContentChangedEventArgs> ContentChanged;

        protected void OnContentChanged(string[] newLines)
        {
            ContentChanged?.Invoke(this, new ContentChangedEventArgs(newLines));
        }

        private Int32 _readGate = 0;
        private Int64 _lastReadPosition = 0;

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

            //need to read block by block until we finish the content, then we can sleep.
            //we can simply read line by line with a stream reader.
            fr.Seek(_lastReadPosition, SeekOrigin.Begin);
            var sr = new StreamReader(fr);

            string line = null;
            while ((line = sr.ReadLine()) != null)
            {
                if (linesBuffer == null)
                {
                    linesBuffer = new List<string>();
                }
                linesBuffer.Add(line);
            }

            _lastReadPosition = fr.Position;
            WriteCheckpoint();
            if (linesBuffer?.Count > 0)
            {
                OnContentChanged(linesBuffer.ToArray());
            }
        }

        public void StartMonitoring()
        {
            //read immediately, then start monitor the file.
            Read();

            _fileSystemWatcher = new FileSystemWatcher();
            _fileSystemWatcher.Path = Path.GetDirectoryName(_monitoredFileName);
            _fileSystemWatcher.Filter = Path.GetFileName(_monitoredFileName);
            _fileSystemWatcher.NotifyFilter = NotifyFilters.LastWrite;
            _fileSystemWatcher.Changed += FileChanged;
            _fileSystemWatcher.EnableRaisingEvents = true;
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

        private void WriteCheckpoint()
        {
            var fileName = GetCheckpointFileName();
            File.WriteAllText(fileName, $"{_lastReadPosition}|{_monitoredFileName}");
        }

        private string GetCheckpointFileName()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.GetFileName(_monitoredFileName)) + ".checkpoint";
        }

        private void ReadCheckpoint()
        {
            var fileName = GetCheckpointFileName();
            if (File.Exists(fileName))
            {
                var content = File.ReadAllText(fileName);
                _lastReadPosition = Int32.Parse(content.Split('|')[0]);
            }
        }
    }
}
