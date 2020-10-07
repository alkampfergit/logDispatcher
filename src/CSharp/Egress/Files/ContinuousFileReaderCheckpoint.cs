using System;
using System.IO;

namespace Egress.Files
{
    public class ContinuousFileReaderCheckpoint
    {
        /// <summary>
        /// Create a continuous file reader from monitored file name.
        /// </summary>
        /// <param name="fileName"></param>
        public static ContinuousFileReaderCheckpoint CreateFromMonitoredFileName(string fileName)
        {
            var checkpoint = new ContinuousFileReaderCheckpoint();
            checkpoint.FileName = fileName; //Set file name because we could not have checkpoint file on disk.
            checkpoint._internalCheckpointFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.GetFileName(fileName)) + ".checkpoint";
           
            ParseCheckpointFile(checkpoint);
            return checkpoint;
        }

        public static ContinuousFileReaderCheckpoint CreateFromCheckpointFile(string fileName)
        {
            var checkpoint = new ContinuousFileReaderCheckpoint();
            checkpoint._internalCheckpointFileName = fileName;

            ParseCheckpointFile(checkpoint);
            return checkpoint;
        }

        private static void ParseCheckpointFile(ContinuousFileReaderCheckpoint checkpoint)
        {
            if (File.Exists(checkpoint._internalCheckpointFileName))
            {
                var content = File.ReadAllText(checkpoint._internalCheckpointFileName);
                var splittedContent = content.Split('|');
                checkpoint.FirstLineHash = splittedContent[0];
                checkpoint.Position = Int64.Parse(splittedContent[1]);
                checkpoint.FileName = splittedContent[2];
            }
        }

        private ContinuousFileReaderCheckpoint()
        {

        }

        private string _internalCheckpointFileName;

        public Int64 Position { get; private set; }
        public string? FirstLineHash { get; private set; }
        public string FileName { get; private set; }

        public void Write(Int64 newPosition) 
        {
            Position = newPosition;
            File.WriteAllText(_internalCheckpointFileName, $"{FirstLineHash}|{Position}|{FileName}");
        }

        internal void ResetPosition(string actualHash)
        {
            FirstLineHash = actualHash;
            Write(0);
        }
    }
}
