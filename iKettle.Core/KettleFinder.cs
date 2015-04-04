using System;
using System.Configuration;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using NLog;

namespace iKettle.Core
{
    public static class KettleFinder
    {
        private readonly static Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Find the iKettle
        /// </summary>
        /// <returns></returns>
        public static IKettle Find()
        {
            string ipAddressTemplate = ConfigurationManager.AppSettings["iKettle.NetworkTemplate"];
            int startAt = Convert.ToInt32(ConfigurationManager.AppSettings["iKettle.StartAt"]);
            Logger.Log(LogLevel.Info, "Using network template: " + ipAddressTemplate);

            for (int i = startAt; i < 254; i++)
            {
                string address = string.Format(ipAddressTemplate, i);
                IPAddress ipAddressStart = IPAddress.Parse(address);

                Logger.Log(LogLevel.Trace, "Looking for iKettle at: " + ipAddressStart + ": ");
                var pingReply = new Ping().Send(ipAddressStart);

                if (pingReply != null)
                {
                    Trace.Write(pingReply.Status + " ");

                    if (pingReply.Status == IPStatus.Success)
                    {
                        if (WiFiKettle.IsKettle(ipAddressStart))
                        {
                            Logger.Log(LogLevel.Info, "Found Kettle!" + ipAddressStart);
                            return new WiFiKettle(ipAddressStart);
                        }
                    }
                }

                Console.WriteLine();
            }

            Logger.Log(LogLevel.Warn, "Kettle not found, using NullKettle");
            return new NullKettle();
        }
    }
}