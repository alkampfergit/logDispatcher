using System;
using System.IO;

namespace Egress.Files
{
    public static class ContinuousFileReaderHelper
    {
        public static void Cleanup()
        {
            var files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.checkpoint");
            foreach (var file in files)
            {
                var checkpoint = ContinuousFileReaderCheckpoint.CreateFromCheckpointFile(file);

                var fileInfo = new FileInfo(checkpoint.FileName);
                if (!fileInfo.Exists)
                {
                    File.Delete(file);
                }
            }
        }
    }
}
