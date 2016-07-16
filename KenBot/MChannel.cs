using System.Collections.Generic;
namespace KenBot
{
    class MChannel
    {
        public string Name;
        public string OwnerNickname;

        //public List<string> Viewers;
        //public List<string> Moderators;

        public HashSet<string> Viewers;
        public HashSet<string> Moderators;

        public MChannel()
        {
            Viewers = new HashSet<string>();
            Moderators = new HashSet<string>();
        }
    }
}
