using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace KenBot
{
    class Program
    {
        public static MCore Core = new MCore();
        static void Main(string[] args)
        {
            Core.User = new MUser()
            {
                //Name = "Moeyyas",
                //OAuthToken = "oauth:vkexns6vo2g92coxm5h8zw2hnvmly7"
                Name = "kenbawt",
                OAuthToken = "oauth:paoor2uuyod3k2csx0kv0zyv0ufou3"
            };
            Core.Channel = new MChannel()
            {
                Name = "#moeyyas",
                OwnerNickname = "Moeyyas"
            };
            Core.DebugMode = true;
            Core.IO.MessageReceived += MessageReceivedCallback;

            Console.WriteLine(Core.IO.Init());
            Console.WriteLine(Core.ConnectToIRC());
            Console.WriteLine(Core.JoinChannel(Core.Channel.Name, Core.IO));
            //Console.WriteLine(Core.SendMessage("BOT launched.", Core.Channel.Name, Core.Channel.OwnerNickname));
            Console.WriteLine(Core.ListViewersAndMods());

            Core.IO.StartReading();
            Console.ReadKey();
        }

        public static void MessageReceivedCallback(MessageReceivedArgs MRargs)
        {
            string Response = MRargs.Content;

            if (!string.IsNullOrEmpty(Response))
            {
                if (Response == "PING")
                if (Response.Substring(Response.IndexOf(' ') + 1, 7) == "PRIVMSG")
                {
                    //References used more than once to prevent repetitive calculations.
                    int ExclamationPoint = Response.IndexOf('!');
                    int LastSemiColon    = Response.LastIndexOf(':');
                    int Hashtag          = Response.IndexOf('#');

                    string SenderNickname = Response.Substring(1, ExclamationPoint - 1);
                    string SenderUsername = Response.Substring(ExclamationPoint + 1, Response.IndexOf('@') - ExclamationPoint - 1);
                    string Message        = Response.Substring(LastSemiColon + 1);
                    string Receiver       = Response.Substring(Hashtag + 1, LastSemiColon - Hashtag - 2);

                    string[] MessageArgs = Message.Split(' ');
                    switch (MessageArgs[0])
                    {
                        //Public commands (normal viewers with no special permission can access)
                        case "!song":
                            //Load song name from file, and send it to chat.
                            break;

                        case "!level":
                            break;

                        case "!rank":
                            break;

                        //Private commands (mods/admins/channel owner)
                        case "!automsg":

                            break;
                    }

                    if (SenderNickname.ToLower() == "moeyyas" &&
                        Message.Substring(0, Message.Length - 2).ToLower() == "greetings, bot.")
                        Core.SendMessage("Greetings, Master.", Core.Channel.Name);
                }
            }
        }
    }
}
