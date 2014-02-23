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
        private static List<Period> GetPeriods()
        {
            return new List<Period>
            {
                new Period { Name = "ticks", Description = "Tick data", ParameterId = "1", DataFormat = "9", HowMuchDaysToDownloadAtTime = 1 },
                new Period { Name = "M1", Description = "1 minute", ParameterId = "2", DataFormat = "5", HowMuchDaysToDownloadAtTime = 5 },
                new Period { Name = "M5", Description = "5 minutes", ParameterId = "3", DataFormat = "5", HowMuchDaysToDownloadAtTime = 7 },
                new Period { Name = "M10", Description = "10 minutes", ParameterId = "4", DataFormat = "5", HowMuchDaysToDownloadAtTime = 10 },
                new Period { Name = "M15", Description = "15 minutes", ParameterId = "5", DataFormat = "5", HowMuchDaysToDownloadAtTime = 15 },
                new Period { Name = "M30", Description = "30 minutes", ParameterId = "6", DataFormat = "5", HowMuchDaysToDownloadAtTime = 30 }
            };
        }

        static void Main(string[] args)
        {
            var periods = GetPeriods();
            Period period = null;

            if(args.Length != 4)
            {
                Console.WriteLine("Usage: FinamTicksDownloader.exe ticker period start_date end_date");
                Console.WriteLine("Example 1:");
                Console.WriteLine("\tFinamTicksDownloader.exe RTS ticks 2013.03.12 2013.05.13");
                Console.WriteLine("\twill download RTS tick data since 12 March 2013 till 13 May 2013");
                Console.WriteLine("Example 2:");
                Console.WriteLine("\tFinamTicksDownloader.exe Si M5 2013.03.12 2013.05.13");
                Console.WriteLine("\twill download Si 5 minute tick data since 12 March 2013 till 13 May 2013");
                Console.WriteLine("Available periods:");
                foreach(var p in periods)
                {
                    Console.WriteLine("\t{0}\t-\t{1}", p.Name, p.Description);
                }
                return;
            }

            period = periods.Where(p => p.Name.ToLower() == args[1].ToLower()).FirstOrDefault();
            if(period == null)
            {
                Console.WriteLine("Period with name " + args[1] + " not found");
                return;
            }

            DateTime startDate = DateTime.Parse(args[2]);
            DateTime endDate = DateTime.Parse(args[3]);
            Console.WriteLine("Downloading since " + startDate.ToDayString() + " till " + endDate.ToDayString());

            Console.Write("Loading tickers list...");
            EmitentHelper.UpdateEmitents();
            Console.WriteLine("OK");

            var ticker = EmitentHelper.EmitentList.Where(x => x.Name == args[0] && x.Market == 14).FirstOrDefault();
            if(ticker == null)
            {
                Console.WriteLine("Ticker with name " + args[0] + " not found");
                return;
            }

            Console.WriteLine("Using ticker " + ticker);

            string fileName = String.Format("{0}-{1}-from-{2:D4}-{3:D2}-{4:D2}-to-{5:D4}-{6:D2}-{7:D2}.txt",
                ticker.Name,
                period.Name,
                startDate.Year,
                startDate.Month,
                startDate.Day,
                endDate.Year,
                endDate.Month,
                endDate.Day
                );

            DateTime currentDateFrom = startDate;

            while (currentDateFrom <= endDate)
            {
                DateTime currentDateTo = getCurrentDateTo(currentDateFrom, period, endDate);
                
                WebDownload webClient = new WebDownload(5 * 60 * 1000);
                webClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.31 (KHTML, like Gecko) Chrome/26.0.1410.64 Safari/537.31");
                webClient.Headers.Add("Referer", "http://www.finam.ru/analysis/profile041CA00007/default.asp");
                webClient.Headers.Add("Accept", "text/html, application/xml;q=0.9, application/xhtml+xml, image/png, image/webp, image/jpeg, image/gif, image/x-xbitmap, */*;q=0.1");
                webClient.Headers.Add("Accept-Language", "en-US,en;q=0.9");
                webClient.Headers.Add("Accept-Encoding", "gzip, deflate"); 
                
                string tempFileName = Path.GetTempPath() + Guid.NewGuid().ToString() + ".txt";

                string chunkFrom = String.Format("{0:D2}{1:D2}{2:D2}",
                    currentDateFrom.Year % 100,
                    currentDateFrom.Month,
                    currentDateFrom.Day);

                string chunkTo = String.Format("{0:D2}{1:D2}{2:D2}",
                    currentDateTo.Year % 100,
                    currentDateTo.Month,
                    currentDateTo.Day);

                string url = String.Format(
                    "http://195.128.78.52/{6}_{0}_{0}.txt?" +
                    "market={8}&em={7}&code={6}&df={1}&mf={2}&yf={3}&dt={10}&mt={11}&yt={12}&p={4}&" +
                    "f={6}_{0}_{9}&e=.txt&cn={6}&dtf=1&tmf=1&MSOR=0&mstime=on&" +
                    "mstimever=1&sep=1&sep2=1&datf={5}",
                    chunkFrom,
                    currentDateFrom.Day,
                    currentDateFrom.Month - 1,
                    currentDateFrom.Year,
                    period.ParameterId,
                    period.DataFormat,
                    ticker.Code,
                    ticker.ID,
                    ticker.Market,
                    chunkTo,
                    currentDateTo.Day,
                    currentDateTo.Month - 1,
                    currentDateTo.Year
                    );

                if (File.Exists(tempFileName))
                    File.Delete(tempFileName);

                Console.WriteLine("Downloading from " + currentDateFrom.ToDayString() + " to " +
                    currentDateTo.ToDayString());

                try
                {
                    webClient.DownloadFile(url, tempFileName);
                    long size = new FileInfo(tempFileName).Length;
                    Console.WriteLine("Downloaded " + size + " bytes");

                    if (size == 0)
                    {
                        Console.WriteLine("Skipping " + currentDateFrom.ToDayString());
                        currentDateFrom = currentDateFrom.AddDays(period.HowMuchDaysToDownloadAtTime);
                        Thread.Sleep(5000);
                        continue;
                    }

                    if (!fileContainsStockData(tempFileName))
                    {
                        Console.WriteLine("File doesn't contain stock data, trying again (file size: " + size + " bytes)");
                        if (size > 0 && size < 300)
                        {
                            Console.WriteLine("Message: ");
                            Console.WriteLine(File.ReadAllText(tempFileName, Encoding.GetEncoding(1251)));
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

                appendAndDelete(tempFileName, fileName);

                currentDateFrom = currentDateFrom.AddDays(period.HowMuchDaysToDownloadAtTime);
            }
        }

        private static DateTime getCurrentDateTo(DateTime currentDateFrom, Period period, DateTime endDate)
        {
            DateTime currentDateTo = currentDateFrom.AddDays(period.HowMuchDaysToDownloadAtTime);
            if (currentDateTo > endDate)
                currentDateTo = endDate;

            return currentDateTo;
        }

        private static void appendAndDelete(string inputFileName, string outputFileName)
        {
            using (Stream input = File.OpenRead(inputFileName))
            using (Stream output = new FileStream(outputFileName, FileMode.Append,
                                                  FileAccess.Write, FileShare.None))
            {
                input.CopyTo(output);
            }
            File.Delete(inputFileName);
        }

        private static bool fileContainsStockData(string filename)
        {
            using(StreamReader reader = new StreamReader(File.Open(filename, FileMode.Open, FileAccess.Read)))
            {
                while(!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    string[] ss = line.Split(new char[] { ',' });

                    if (ss.Length >= 4)
                        return true;
                }
            }

            return false;
        }
    }
}
