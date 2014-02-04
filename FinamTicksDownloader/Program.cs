using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FinamTicksDownloader
{
    class Program
    {
        static void Main(string[] args)
        {
            //EmitentHelper.UpdateEmitents();
            //Emitent rts = EmitentHelper.EmitentList.Where(x => x.Name == "RTS").FirstOrDefault();
            //return;
            DateTime startDate = new DateTime(2013, 03,12);
            DateTime endDate = new DateTime(2013, 05, 13);

            DateTime currentDate = startDate;

            while (currentDate <= endDate)
            {
                WebDownload webClient = new WebDownload(5 * 60 * 1000);
                webClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.31 (KHTML, like Gecko) Chrome/26.0.1410.64 Safari/537.31");
                webClient.Headers.Add("Referer", "http://www.finam.ru/analysis/profile041CA00007/default.asp");
                webClient.Headers.Add("Accept", "text/html, application/xml;q=0.9, application/xhtml+xml, image/png, image/webp, image/jpeg, image/gif, image/x-xbitmap, */*;q=0.1");
                webClient.Headers.Add("Accept-Language", "en-US,en;q=0.9");
                webClient.Headers.Add("Accept-Encoding", "gzip, deflate"); 
                
                string fileName = String.Format("{0:D4}-{1:D2}-{2:D2}.txt",
                    currentDate.Year,
                    currentDate.Month,
                    currentDate.Day);

                string chunk = String.Format("{0:D2}{1:D2}{2:D2}",
                    currentDate.Year % 100,
                    currentDate.Month,
                    currentDate.Day);

                string url = String.Format(
                    "http://195.128.78.52/SPFB.RTS_{0}_{0}.txt?" +
                    "market=14&em=17455&code=SPFB.RTS&df={1}&mf={2}&yf={3}&dt={1}&mt={2}&yt={3}&p=1&" +
                    "f=SPFB.RTS_{0}_{0}&e=.txt&cn=SPFB.RTS&dtf=1&tmf=1&MSOR=0&mstime=on&" +
                    "mstimever=1&sep=1&sep2=1&datf=9",
                    chunk,
                    currentDate.Day,
                    currentDate.Month - 1,
                    currentDate.Year
                    );

                if (File.Exists(fileName))
                    File.Delete(fileName);

                Console.WriteLine("Downloading " + currentDate);

                try
                {
                    webClient.DownloadFile(url, fileName);
                    long size = new FileInfo(fileName).Length;
                    Console.WriteLine("Downloaded " + size + " bytes");

                    if (size == 0)
                    {
                        Console.WriteLine("Skipping " + currentDate.ToString());
                        currentDate = currentDate.AddDays(1);
                        Thread.Sleep(5000);
                        continue;
                    }

                    if (size < 5 * 100 * 1024)
                    {
                        Console.WriteLine("File size less than 500KB, trying again");
                        if (size > 0 && size < 300)
                        {
                            Console.WriteLine("Message: ");
                            Console.WriteLine(File.ReadAllText(fileName, Encoding.GetEncoding(1251)));
                        }
                        Thread.Sleep(5 * 1000);
                        continue;
                    }
                    Thread.Sleep(5 * 1000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception: " + ex.Message);
                    Console.WriteLine("Trying again");
                    Thread.Sleep(5 * 1000);
                    continue;
                }
                currentDate = currentDate.AddDays(1);
            }
        }
    }
}
