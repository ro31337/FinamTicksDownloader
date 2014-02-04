using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinamTicksDownloader
{
    public class Emitent // Наш класс с информацией по эмитенту
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public int Market { get; set; }
    }
}
