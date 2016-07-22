using System.Diagnostics;

namespace KenBot
{
    public class MCommand
    {
        public int Frequency; //In seconds
        private Stopwatch CooldownTimer = new Stopwatch();
        public bool IsAvailable = true;

        public void RunCooldown()
        {
            CooldownTimer.Start();
            IsAvailable = false;
            while (CooldownTimer.Elapsed.Seconds < Frequency)
            { }
            IsAvailable = true;
            CooldownTimer.Stop();
        }
    }
}
