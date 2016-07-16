using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading;
namespace KenBot
{
    public delegate void OnMessageReceived(string Content);

    class MIO
    {
        public string IP { get; set; } = "irc.chat.twitch.tv";
        public int Port { get; set; } = 6667;
        public TcpClient TCPClient { get; private set; }
        public NetworkStream Stream { get; private set; }

        public TimeSpan ReadTimeoutDuration = new TimeSpan(0, 0, 5);

        private Thread ReadThread;

        public string Response = string.Empty;
        public byte[] Buffer = new byte[512];

        public event OnMessageReceived MessageReceived;

        public string Init()
        {
            TCPClient = new TcpClient(IP, Port);
            Stream = TCPClient.GetStream();
            string Response = string.Empty;
            string Command = "CAP REQ :twitch.tv/membership\r\n";
            AttemptWrite(Command);
            Response += string.Concat(Command, "\r\n", AttemptRead());
            Command = "CAP REQ :twitch.tv/commands\r\n";
            AttemptWrite(Command);
            Response += string.Concat(Command, "\r\n", AttemptRead());
            return Response;
        }

        public void AttemptWrite(string _Command)
        {
            if (Stream.CanWrite)
            {
                Buffer = Encoding.ASCII.GetBytes(_Command);
                Stream.Write(Buffer, 0, Buffer.Length);
            }
        }

        public string AttemptRead()
        {
            Stopwatch TimeoutTimer = new Stopwatch();
            TimeoutTimer.Start();
            while (TimeoutTimer.Elapsed.Seconds <= ReadTimeoutDuration.Seconds)
            {
                if (Stream.DataAvailable)
                {
                    Buffer = new byte[512];
                    int numOfBytes = Stream.Read(Buffer, 0, Buffer.Length);
                    Response = Encoding.ASCII.GetString(Buffer, 0, numOfBytes);
                    string Content = Response;
                    MessageReceived.Invoke(Content);
                    TimeoutTimer.Stop();
                    return Response;
                }
                else
                {
                    Response = "No data available in NetworkStream to read.";
                }
            }
            TimeoutTimer.Stop();
            return Response;
        }

        public void StartReading()
        {
            READ:
            while (Stream.DataAvailable)
            {
                Buffer = new byte[512];
                int numOfBytes = Stream.Read(Buffer, 0, Buffer.Length);
                Response = Encoding.ASCII.GetString(Buffer, 0, numOfBytes);
                string Content = Response;
                MessageReceived.Invoke(Content);
            }
            while (!Stream.DataAvailable)
                Thread.Sleep(100);
            goto READ;
        }

        public void AttemptStartReading()
        {
            ReadThread = new Thread(delegate ()
            {
                StartReading();
            });
            ReadThread.IsBackground = true;
            ReadThread.Start();
        }

        public void AttemptStopReading()
        {
            if (ReadThread != null &&
                ReadThread.IsAlive)
            {
                try
                {
                    ReadThread.Abort();
                }
                catch (ThreadAbortException)
                {
                    ReadThread = null;
                }
            }
        }
    }
}
