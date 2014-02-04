using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinamTicksDownloader
{
    public static class EmitentHelper
    {
        public static List<Emitent> EmitentList = new List<Emitent>(); // Список всех эмитентов
        
        private static string[] clearBrakes(string[] subject) // Просто функция для удаления лишней информации в строках JS-присваивания переменных
        {
            int brakeIndex = subject[0].IndexOf('[');
            if (brakeIndex > 0)
            {
                subject[0] = subject[0].Substring(brakeIndex + 1, subject[0].Length - brakeIndex - 1);
            }
            brakeIndex = subject[subject.Count() - 1].IndexOf(']');
            if (brakeIndex > 0)
            {
                subject[subject.Count() - 1] = subject[subject.Count() - 1].Substring(0, brakeIndex);
            }
            return subject;
        }

        public static bool UpdateEmitents() // Функция по затягиванию информации об эмитентах
        {
            bool result = false;
            EmitentList.Clear();
            try
            {
                WebDownload downloadClient = new WebDownload(5 * 60 * 1000);
                string responseBody = downloadClient.DownloadString("http://www.finam.ru/cache/icharts/icharts.js");
                string[] responseVars = responseBody.Split('\n');
                string[] emitentIDs = { };
                string[] emitentNames = { };
                string[] emitentCodes = { };
                string[] emitentMarkets = { };
                for (int i = 0; i < responseVars.Count(); i++)
                {
                    string[] str = responseVars[i].Split('=');
                    switch (i)
                    {
                        case 0:
                            emitentIDs = responseVars[i].Split(',');
                            emitentIDs = clearBrakes(emitentIDs);
                            break;
                        case 1:
                            emitentNames = responseVars[i].Split(',');
                            emitentNames = clearBrakes(emitentNames);
                            break;
                        case 2:
                            emitentCodes = responseVars[i].Split(',');
                            emitentCodes = clearBrakes(emitentCodes);
                            break;
                        case 3:
                            emitentMarkets = responseVars[i].Split(',');
                            emitentMarkets = clearBrakes(emitentMarkets);
                            break;
                        default:
                            break;
                    }
                }
                for (int i = 0; i < emitentIDs.Count(); i++)
                {
                    Emitent emitent = new Emitent()
                    {
                        ID = int.Parse(emitentIDs[i]),
                        Name = emitentNames[i].Trim(new char[] {'\''}),
                        Code = emitentCodes[i].Trim(new char[] { '\'' }),
                        Market = int.Parse(emitentMarkets[i])
                    };
                    EmitentList.Add(emitent);
                }
                result = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return result;
        }
    }
}
