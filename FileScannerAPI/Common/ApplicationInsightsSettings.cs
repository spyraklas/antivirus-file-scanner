using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileScannerAPI.Common
{
    public class ApplicationInsightsSettings
    {
        public string InstrumentationKey { get; set; }

        public string Layout { get; set; }

        public int MaximumQueueLimit { get; set; }
    }
}
