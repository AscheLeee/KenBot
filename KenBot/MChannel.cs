using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KenBot
{
    class MChannel
    {
        public string Name;
        public string OwnerNickname;

        public List<string> Viewers;
        public List<string> Moderators;

        public MChannel()
        {
            Viewers = new List<string>();
        }
    }
}
