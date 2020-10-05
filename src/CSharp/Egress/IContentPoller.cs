using System;

namespace Egress
{
    public interface IContentPoller
    {
        event EventHandler<LinesPolledEventArgs> ContentChanged;
    }
}
