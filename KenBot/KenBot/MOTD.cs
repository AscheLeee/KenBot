using System.Threading;

namespace KenBot
{
    public class MOTD
    {
        public string Message = "I hope you're enjoying the stream! :-)";
        //Prefer to not need reference to the channel name.
        public string ChannelName = string.Empty;
        //Prefer to have Frequency in TimeSpan type.
        public int Frequency = 300; //In seconds
        public bool Enabled;
        public Thread PostThread;
        public delegate void SendMessageDel(string _Message, string _ChannelName);
        public SendMessageDel SendMessageMethod;

        private void StartPosting()
        {
            while (Enabled)
            {
                SendMessageMethod(Message, ChannelName);
                Thread.Sleep(Frequency * 1000);
            }
        }

        public void Update()
        {
            if (Enabled)
            {
                AttemptStopPosting();
                AttemptStartPosting();
            }
            else if (!Enabled)
            {
                AttemptStopPosting();
            }
        }

        public void AttemptStartPosting()
        {
            if (Enabled && !SendMessageMethod.Equals(null))
            {
                PostThread = new Thread(StartPosting);
                PostThread.IsBackground = true;
                PostThread.Start();
            }
        }

        public void AttemptStopPosting()
        {
            if (!PostThread.Equals(null) &&
                PostThread.IsAlive)
            {
                try
                {
                    PostThread.Abort();
                }
                catch (ThreadAbortException)
                {
                    PostThread = null;
                }
            }
        }
    }
}
