using System;

namespace Egress.Files
{
    public class ContentChangedEventArgs : EventArgs
    {
        public ContentChangedEventArgs(string[] newLines)
        {
            NewLines = newLines;
        }

        /// <summary>
        /// This property contains new lines read on the file.
        /// </summary>
        public String[] NewLines { get; private set; }
    }
}
