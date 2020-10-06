using System;

namespace Egress
{
    public class LinesPolledEventArgs : EventArgs
    {
        public LinesPolledEventArgs(string[] newLines)
        {
            NewLines = newLines;
        }

        /// <summary>
        /// This property contains new lines read on the file.
        /// </summary>
        public String[] NewLines { get; private set; }
    }
}
