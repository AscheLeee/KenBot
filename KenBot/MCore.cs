using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace KenBot
{
    class MCore
    {
        public MChannel Channel = new MChannel();
        public MUser User = new MUser();
        public MIO IO = new MIO();
        public MHelper Helper = new MHelper();

        public bool DebugMode = false;

        public string Command = string.Empty;

        public string ConnectToIRC()
        {
            if (string.IsNullOrEmpty(User.OAuthToken))
                return "'User.OAuthToken' in MCore.ConnectToIRC is NULL/empty.";
            if (string.IsNullOrEmpty(User.Name))
                return "'User.Name' in MCore.ConnectToIRC is NULL/empty.";

            Command = string.Format("PASS {0}\r\nNICK {1}\r\n", User.OAuthToken, User.Name);
            IO.AttemptWrite(Command);

            if (DebugMode)
            {
                return string.Concat(Command, "\n", IO.AttemptRead());
            }
            else
                return IO.AttemptRead();
        }

        public string JoinChannel(string _Channel, MIO IO)
        {
            if (_Channel[0] != '#')
                _Channel.Insert(0, "#");
            if (string.IsNullOrEmpty(_Channel))
                return "'_Channel' in MCore.JoinChannel is NULL/empty.";

            Channel.Name = _Channel;
            Command = string.Format("JOIN {0}\r\n", _Channel); //Channel.Name must be prefixed with '#'.
            IO.AttemptWrite(Command);
            //Update(Response);
            if (DebugMode)
            {
                return string.Concat(Command, "\n", IO.AttemptRead());
            }
            else
                return IO.AttemptRead();
        }

        public void UpdateChannel(string JoinChannelResponse)
        {
            string[] lines =
                //JoinChannelResponse.Remove(0, JoinChannelResponse.IndexOf("353:"))
                //.Remove(JoinChannelResponse.IndexOf("366:"))
                JoinChannelResponse.Split("\r\n".ToCharArray())
                .ToArray();
            //353: = #cn firstUserHere1
            //01234567           8
            int startIndex = 8 + Channel.Name.Length;
            for (int i = 1; i < lines.Length - 5; i++)
            {
                lines[i].Remove(0, 8 + Channel.Name.Length);
                string[] Usernames = lines[i].Split(' ').ToArray();
                for (int j = 0; j < Usernames.Length; j++)
                {
                    Channel.Viewers.Add(Usernames[j]);
                }
            }
        }

        public string ListViewersAndMods()
        {
            Command = string.Format("PRIVMSG {0} :{1}\r\n", Channel.Name, ".mods");
            IO.AttemptWrite(Command);

            System.Diagnostics.Stopwatch TimeTillTimeout = new System.Diagnostics.Stopwatch();
            TimeSpan TimeoutDuration = new TimeSpan(0, 0, 5);
            string Response = IO.AttemptRead();

            TimeTillTimeout.Start();

            while (!Response.Contains("The moderators of this room are")
                && TimeTillTimeout.Elapsed.Seconds <= TimeoutDuration.Seconds)
            {
                Response += IO.AttemptRead();
            }
            if (DebugMode)
                return string.Concat(Command, "\n", Response);
            return Response;
        }

        public string SendMessage(string _Message, string _ChannelName)
        {
            if (_ChannelName[0] != '#') //Channel.Name must be prefixed with '#'.
                _ChannelName.Insert(0, "#");
            if (string.IsNullOrEmpty(_Message))
                return "'_Message' in MCore.SendChannelMessage is NULL/empty.";
            if (string.IsNullOrEmpty(_ChannelName))
                return "'_Recipient' in MCore.SendChannelMessage is NULL/empty.";

            Command = string.Format("PRIVMSG {0} :{1}\r\n", _ChannelName, _Message);
            IO.AttemptWrite(Command);
            return Command;
        }

        public string SendMessage(string _Message, string _ChannelName, string _Recipient)
        {
            if (_ChannelName[0] != '#') //Channel.Name must be prefixed with '#'.
                _ChannelName.Insert(0, "#");
            if (string.IsNullOrEmpty(_Message))
                return "'_Message' in MCore.SendPrivateMessage is NULL/empty.";
            if (string.IsNullOrEmpty(_Recipient))
                return "'_Recipient' in MCore.SendPrivateMessage is NULL/empty.";

            Command = string.Format("PRIVMSG {0} :/w {1} {2}\r\n", _ChannelName, _Recipient, _Message);
            IO.AttemptWrite(Command);
            return Command;
        }

    }
}
