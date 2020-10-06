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
                var content = File.ReadAllText(file);
                var splitContent = content.Split('|');
                if (splitContent.Length > 1) 
                {
                    var fileInfo = new FileInfo(splitContent[1]);
                    if (!fileInfo.Exists)
                    {
                        File.Delete(file);
                    }
                }
            }
        }
    }
}
