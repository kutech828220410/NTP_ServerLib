using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NTP_ServerLib;
namespace test
{
    class Program
    {
        static void Main(string[] args)
        {
            NTPServerClass nTPServerClass = new NTPServerClass();
            nTPServerClass.getWebTime("220.135.128.247");
        }
    }
}
