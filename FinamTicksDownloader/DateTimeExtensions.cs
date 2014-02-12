using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinamTicksDownloader
{
    public static class DateTimeExtensions
    {
        public static string ToDayString(this DateTime dateTime)
        {
            return
                String.Format("{0:D2}.{1:D2}.{2:D4}",
                dateTime.Day,
                dateTime.Month,
                dateTime.Year);
        }
    }
}
