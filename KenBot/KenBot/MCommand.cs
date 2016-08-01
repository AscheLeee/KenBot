using System.Diagnostics;
using System.Threading;

namespace KenBotClient
{
    public class MCommand
    {
        public int Frequency; //In seconds
        private Stopwatch CooldownTimer = new Stopwatch();
        public bool IsAvailable = true;
        public Thread CooldownThread;

        private void RunCooldown()
        {
            CooldownTimer.Start();
            IsAvailable = false;
            while (CooldownTimer.Elapsed.Seconds < Frequency)
            { }
            IsAvailable = true;
            CooldownTimer.Stop();
        }

        public void AttemptRunCooldown()
        {
            CooldownThread = new Thread(RunCooldown);
            CooldownThread.Start();
        }
    }
}
