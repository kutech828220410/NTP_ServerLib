using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace NTP_ServerLib
{
    public class NTPServerClass
    {
        public void getWebTime(string server)
        {
            try
            {
                // NTP服务端地址
                string ntpServer = $"{server}";

                // NTP message size - 16 bytes of the digest (RFC 2030)
                byte[] ntpData = new byte[48];
                // Setting the Leap Indicator, Version Number and Mode values
                ntpData[0] = 0x1B; // LI = 0 (no warning), VN = 3 (IPv4 only), Mode = 3 (Client Mode)

                IPAddress ip = IPAddress.Parse(ntpServer);

                // The UDP port number assigned to NTP is 123
                IPEndPoint ipEndPoint = new IPEndPoint(ip, 123);//addresses[0]

                // NTP uses UDP
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                socket.Connect(ipEndPoint);
                // Stops code hang if NTP is blocked
                socket.ReceiveTimeout = 3000;
                socket.Send(ntpData);
                socket.Receive(ntpData);
                socket.Close();

                // Offset to get to the "Transmit Timestamp" field (time at which the reply 
                // departed the server for the client, in 64-bit timestamp format."
                const byte serverReplyTime = 40;
                // Get the seconds part
                ulong intPart = BitConverter.ToUInt32(ntpData, serverReplyTime);
                // Get the seconds fraction
                ulong fractPart = BitConverter.ToUInt32(ntpData, serverReplyTime + 4);
                // Convert From big-endian to little-endian
                intPart = swapEndian(intPart);
                fractPart = swapEndian(fractPart);
                ulong milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000UL);

                // UTC time
                DateTime webTime = (new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddMilliseconds(milliseconds);
                string localTime = DateTime.Now.ToString("yyyyMMddHHmmss");
                // Local time
                DateTime dt = webTime.ToLocalTime();
                SyncTime(dt);

            }
            catch (Exception ex)
            {

            }


        

        }

        // 小端存储与大端存储的转换
        private uint swapEndian(ulong x)
        {
            return (uint)(((x & 0x000000ff) << 24) +
            ((x & 0x0000ff00) << 8) +
            ((x & 0x00ff0000) >> 8) +
            ((x & 0xff000000) >> 24));
        }
        public struct SystemTime
        {
            public ushort wYear;
            public ushort wMonth;
            public ushort wDayOfWeek;
            public ushort wDay;
            public ushort wHour;
            public ushort wMinute;
            public ushort wSecond;
            public ushort wMilliseconds;

            //利用System.DateTime设置SYSTEMTIME数据成员
            public void FromDateTime(DateTime time)
            {
                wYear = (ushort)time.Year;
                wMonth = (ushort)time.Month;
                wDayOfWeek = (ushort)time.DayOfWeek;
                wDay = (ushort)time.Day;
                wHour = (ushort)time.Hour;
                wMinute = (ushort)time.Minute;
                wSecond = (ushort)time.Second;
                wMilliseconds = (ushort)time.Millisecond;
            }
        }

        [DllImport("Kernel32.dll")]   //(2)
        public static extern bool SetLocalTime(ref SystemTime Time);  //(3)
        [DllImport("Kernel32.dll")]
        public static extern void GetLocalTime(ref SystemTime Time);

        /// <summary> 
        /// 设置系统时间 
        /// </summary> 
        public static bool SyncTime(DateTime currentTime)
        {
            bool flag = false;
            try
            {
                SystemTime sysTime = new SystemTime();
                sysTime.wYear = Convert.ToUInt16(currentTime.Year);
                sysTime.wMonth = Convert.ToUInt16(currentTime.Month);
                sysTime.wDay = Convert.ToUInt16(currentTime.Day);
                sysTime.wDayOfWeek = Convert.ToUInt16(currentTime.DayOfWeek);
                sysTime.wMinute = Convert.ToUInt16(currentTime.Minute);
                sysTime.wSecond = Convert.ToUInt16(currentTime.Second);
                sysTime.wMilliseconds = Convert.ToUInt16(currentTime.Millisecond);
                sysTime.wHour = Convert.ToUInt16(currentTime.Hour);

                SetLocalTime(ref sysTime);//设置本机时间
                flag = true;
            }
            catch (Exception)
            {
                flag = false;
            }
            return flag;
        }

    }
}
