using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NTP_ServerLib;
using System.IO;
using Basic;
using System.Reflection;
namespace test
{
    class Program
    {
        static public string currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        static private string DBConfigFileName = $"{currentDirectory}//DBConfig.txt";
        public class DBConfigClass
        {
            private string name = "";
            private string server_ip = "http://127.0.0.1:4433";
            public string Name { get => name; set => name = value; }
            public string Server_ip { get => server_ip; set => server_ip = value; }
        }
        static DBConfigClass dBConfigClass = new DBConfigClass();

        static public bool LoadDBConfig()
        {
            string jsonstr = MyFileStream.LoadFileAllText($"{DBConfigFileName}");
            Console.WriteLine($"路徑 : {DBConfigFileName} 開始讀取");
            Console.WriteLine($"-------------------------------------------------------------------------");
            if (jsonstr.StringIsEmpty())
            {
                jsonstr = Basic.Net.JsonSerializationt<DBConfigClass>(new DBConfigClass(), true);
                List<string> list_jsonstring = new List<string>();
                list_jsonstring.Add(jsonstr);
                if (!MyFileStream.SaveFile($"{DBConfigFileName}", list_jsonstring))
                {
                    Console.WriteLine($"建立{DBConfigFileName}檔案失敗!");
                    return false;
                }
                Console.WriteLine($"未建立參數文件!請至子目錄設定{DBConfigFileName}");
                return false;
            }
            else
            {
                dBConfigClass = Basic.Net.JsonDeserializet<DBConfigClass>(jsonstr);

                jsonstr = Basic.Net.JsonSerializationt<DBConfigClass>(dBConfigClass, true);
                List<string> list_jsonstring = new List<string>();
                list_jsonstring.Add(jsonstr);
                if (!MyFileStream.SaveFile($"{DBConfigFileName}", list_jsonstring))
                {
                    Console.WriteLine($"建立{DBConfigFileName}檔案失敗!");
                    return false;
                }

            }
            return true;

        }
        static void Main(string[] args)
        {
            LoadDBConfig();
            Logger.LogAddLine();
            Logger.Log($"url :" + $"{dBConfigClass.Server_ip}/api/time");
            string json = Basic.Net.WEBApiGet($"{dBConfigClass.Server_ip}/api/time");
            Logger.Log($"json :" + $"{json}");
            if(json.Check_Date_String() == false)
            {
                Logger.Log($"傳回字串為非法時間");
                return;
            }
            DateTime dateTime = json.StringToDateTime();
            NTPServerClass.SyncTime(dateTime);
            Logger.LogAddLine();
            //NTPServerClass nTPServerClass = new NTPServerClass();
            //nTPServerClass.getWebTime(dBConfigClass.Server_ip);


        }
    }
}
