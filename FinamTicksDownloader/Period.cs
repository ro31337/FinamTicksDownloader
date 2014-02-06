using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinamTicksDownloader
{
    public class Period
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParameterId { get; set; }
        public string DataFormat { get; set; }
        public int MinimumFileSizeBytes { get; set; }
    }
}
