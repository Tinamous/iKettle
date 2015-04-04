using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace iKettle.Core
{
    /// <summary>
    /// Represents the kettle.
    /// </summary>
    public class WiFiKettle : IKettle, IDisposable
    {
        public event EventHandler<KettleStatusEventArgs> StatusChanged;

        private const int Port = 2000;
        private const string GetSystemStatus = "get sys status";
        private NetworkStream _networkStream = null;
        private StreamWriter _streamWriter = null;
        private Task _watcherTask;
        private readonly IPEndPoint _endPoint;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public WiFiKettle(IPAddress address)
        {
            if (address == null) throw new ArgumentNullException("address");

            _endPoint = new IPEndPoint(address, Port);
        }

        public void Connect()
        {
            _logger.Log(LogLevel.Info, "Connecting to kettle");
            var tcpClient = new TcpClient();
            tcpClient.Connect(_endPoint);

            _networkStream = tcpClient.GetStream();
            _streamWriter = new StreamWriter(_networkStream, Encoding.ASCII, 100, true) { AutoFlush = true };

            OnStatusChanged(new KettleStatusEventArgs(-2, "Connected to iKettle on " + _endPoint.Address.ToString()));

            _logger.Log(LogLevel.Info, "Connected");

            _watcherTask = Task.Run(() => Watch());

            GetStatus();
        }

        protected virtual void OnStatusChanged(KettleStatusEventArgs e)
        {
            var handler = StatusChanged;
            if (handler != null) handler(this, e);
        }

        public void HelloKettle()
        {
            _logger.Log(LogLevel.Info, "Sending hello kettle");
            Send("HELLOKETTLE");
        }

        public void Off()
        {
            _logger.Log(LogLevel.Info, "Requesting Off");
            Send("set sys output 0x0");
        }

        public void Boil()
        {
            _logger.Log(LogLevel.Info, "Requesting boil");
            Send("set sys output 0x4");
        }

        public void GetStatus()
        {
            _logger.Log(LogLevel.Info, "Get staus");
            Send(GetSystemStatus);
        }

        private void Send(string message)
        {
            // Terminate the message to be sent with a \n
            _streamWriter.Write(message + "\n");
            _streamWriter.Flush();
        }

        private void Watch()
        {
            using (var streamReader = new StreamReader(_networkStream, Encoding.ASCII, true, 100, false))
            {
                string read = "";
                while (true)
                {
                    char[] characters = new char[1];
                    streamReader.Read(characters, 0, 1);
                    char readCharacter = characters[0];
                    Console.Write(readCharacter);

                    // Messages from kettle end in \r (even though the ones sent end in \n)
                    if (readCharacter == '\r')
                    {
                        _logger.Log(LogLevel.Debug, "Read from Kettle: " + read);
                        ProcessStatus(read);
                        read = "";
                    }
                    else
                    {
                        read += readCharacter;
                    }
                }
            }
        }

        private void ProcessStatus(string read)
        {
            if (read.StartsWith("sys status key"))
            {
                // From GetStats.
                ProcessStatusKey(read);
            }
            else
            {
                // Events from kettle for status change.
                // sys status 0x....
                string[] splitStatus = read.Split(' ');

                if (splitStatus.Length != 3)
                {
                    // Invalid message, might be HELLOAPP
                    OnStatusChanged(new KettleStatusEventArgs(-1, read));
                    return;
                }

                int code = Convert.ToInt32(splitStatus[2], 16);
                string message = GetStatusMessage(splitStatus[2], code);

                OnStatusChanged(new KettleStatusEventArgs(code, message));
            }
        }

        private string GetStatusMessage(string splitStatus, int code)
        {
            switch (splitStatus)
            {
                case "0x0":
                    return "Kettle off";
                case "0x1":
                    return "Kettle removed";
                case "0x2":
                    return "Kettle Overheated";
                case "0x3":
                    return "Kettle boiled";
                case "0x4":
                    return "Keep warm expired";
                case "0x5":
                    return "Kettle Boiling";
                case "0x10":
                    return "Keep warm disabled";
                case "0x11":
                    return "Keep warm enabled";
                case "0x65":
                    return "Temperature set to 65°C";
                case "0x80":
                    return "Temperature set to 80°C";
                case "0x95":
                    return "Temperature set to 95°C";
                case "0x100":
                    return "Temperature set to 100°C";
                case "0x8005":
                    // 5 * 60 * 1000
                    return "Keep warm tile set to 5 mins";
                case "0x8010":
                    return "Keep warm tile set to 10 mins";
                case "0x8020":
                    return "Keep warm tile set to 20 mins";
                default:
                    return "";
            }
        }

        private void ProcessStatusKey(string read)
        {
            // Handle the kettles current status: "sys status key"....
        }

        /// <summary>
        /// Determine if the thing at address is an iKettle by sending HELLOKETTLE
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static bool IsKettle(IPAddress address)
        {
            NetworkStream networkStream = null;
            StreamWriter streamWriter = null;
            StreamReader streamReader = null;

            try
            {
                IPEndPoint endPoint = new IPEndPoint(address, Port);

                var tcpClient = new TcpClient();
                tcpClient.Connect(endPoint);

                networkStream = tcpClient.GetStream();
                streamWriter = new StreamWriter(networkStream, Encoding.ASCII, 100, true);
                streamReader = new StreamReader(networkStream, Encoding.ASCII);

                streamWriter.Write("HELLOKETTLE\n");
                streamWriter.Flush();

                char[] buffer = new char[9];
                //string line = streamReader.ReadToEnd();
                int read = streamReader.Read(buffer, 0, 9);
                if (read >= 8)
                {
                    string line = new string(buffer);
                    Trace.WriteLine("Response: " + line);
                    if (!string.IsNullOrWhiteSpace(line) && line.StartsWith("HELLOAPP\r"))
                    {
                        // If the thing responded with HELLOAPP then we know it's the iKettle
                        return true;
                    }

                }

            }
            catch (SocketException ex)
            {
                Trace.WriteLine(ex);
            }
            finally
            {
                if (networkStream != null)
                {
                    networkStream.Dispose();
                }

                if (streamWriter != null)
                {
                    streamWriter.Dispose();
                }

                if (streamReader != null)
                {
                    streamReader.Dispose();
                }
            }
            return false;
        }

        public void Dispose()
        {
            if (_streamWriter != null)
            {
                _streamWriter.Close();
                _streamWriter = null;
            }

            if (_networkStream != null)
            {
                _networkStream.Close();
                _networkStream = null;
            }
        }
    }
}
