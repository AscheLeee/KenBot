using System.Collections.Generic;

namespace KenBot
{
    public class MChannel
    {
        public string Name;
        public string OwnerNickname;

        public List<string> Viewers;
        public List<string> Moderators;

        public MChannel()
        {
            Viewers = new List<string>();
            Moderators = new List<string>();
        }
    }
}
