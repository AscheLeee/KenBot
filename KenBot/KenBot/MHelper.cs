using System;
using System.Collections.Generic;
using System.Linq;

namespace KenBot
{
    public class MHelper
    {
        public static List<string> GetViewers(string _NoticeResponse)
        {
            List<string> Nicknames = new List<string>();
            string[] AllLines = _NoticeResponse.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            string[] NicknameLines = AllLines.Skip(1)
                .Where(i => !i.IndexOf(' ').Equals(-1)
                && i.Length > i.IndexOf(' ') + 4
                && i.Substring(i.IndexOf(' ') + 1, 3).Equals("353")).ToArray();

            if (!NicknameLines.Length.Equals(0))
                for (int i = 0; i < NicknameLines.Length; i++)
                {
                    Nicknames.AddRange(NicknameLines[i].Split(':')[2].Split(' '));
                }
            return Nicknames;
        }

        public static List<string> GetModerators(string _ModsCommandResponse)
        {
            List<string> Nicknames = new List<string>();
            string[] ModNicknames = _ModsCommandResponse.Split(':');
            string ChannelOwnerNickname = string.Empty;
            int IndexOfHashtag = _ModsCommandResponse.IndexOf('#');
            int IndexOfSemiColon = _ModsCommandResponse.IndexOf(':', IndexOfHashtag);

            if (!IndexOfHashtag.Equals(-1))
            {
                if (!IndexOfSemiColon.Equals(-1))
                {
                    ChannelOwnerNickname = _ModsCommandResponse.Substring(IndexOfHashtag + 1, IndexOfSemiColon - IndexOfHashtag - 2);
                    Nicknames.Add(ChannelOwnerNickname.ToLower());
                }
            }

            if (!_ModsCommandResponse.Length.Equals(0)
                && ModNicknames.Length > 2)
            {
                string NicknameLine = _ModsCommandResponse.Split(':')[3];
                Nicknames.AddRange(NicknameLine.Remove(0, 1).Remove(NicknameLine.Length - 4).Split(", ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries));
            }
            return Nicknames;
        }
    }
}