using System.Collections.Generic;

namespace Egress.Configuration
{
    public class ContentPollerConfiguration
    {
        /// <summary>
        /// This is the name of the concrete class that implements the 
        /// content poller.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Parameters that should be passed on construcor by reflection
        /// to create the object.
        /// </summary>
        public Dictionary<string, object> Configuration { get; set; }
    }
}
