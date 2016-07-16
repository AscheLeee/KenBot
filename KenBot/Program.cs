using KenBot;
using System;
using System.IO;
using Newtonsoft.Json;
using System.Linq;

namespace Kenbot
{
    static class Program
    {
        public static MSettings Settings;
        public static MCore Core;
        public static string SettingsFileName = "Settings.json";
        public static string SettingsFileContent = string.Empty;
        public static string MCoreFileContent = string.Empty;
        public static string SongfileContent = string.Empty;
        public static FileStream SettingsFileStream;
        public static FileStream MCoreFileStream;
        public static FileStream SongFileStream;

        static void Main()
        {
            Initialize();

            Core.IO.MessageReceived += MessageReceivedCallback;

            Console.WriteLine(Core.IO.Init());
            Console.WriteLine(Core.ConnectToIRC());
            Console.WriteLine(Core.JoinChannel(Core.Channel.Name, Core.IO));
            Console.WriteLine(Core.SendMessage("Bot is on duty.", Core.Channel.Name));

            Core.IO.StartReading();

            Console.ReadKey();
        }

        public static void Initialize()
        {
            try
            {
                SettingsFileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, SettingsFileName));

                Settings = JsonConvert.DeserializeObject<MSettings>(SettingsFileContent);

                //SettingsFileStream = new FileStream(Path.Combine(Environment.CurrentDirectory, SettingsFileName), FileMode.Open);

                if (!string.IsNullOrWhiteSpace(Settings.MCoreFileLocation))
                {
                    MCoreFileContent = File.ReadAllText(Settings.MCoreFileLocation);
                }
                else if (!string.IsNullOrWhiteSpace(Settings.MCoreFileName))
                {
                    MCoreFileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, Settings.MCoreFileName));
                }

                Core = JsonConvert.DeserializeObject<MCore>(MCoreFileContent);

                if (!string.IsNullOrWhiteSpace(Settings.SongFileLocation))
                {
                    SongfileContent = File.ReadAllText(Settings.SongFileLocation);
                }
                else if (!string.IsNullOrWhiteSpace(Settings.SongFileName))
                {
                    SongfileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, Settings.SongFileName));
                }
            }
            catch (FileNotFoundException)
            {
                string Message = string.Empty;
                if (string.IsNullOrWhiteSpace(SettingsFileContent))
                {
                    Message = string.Format("Settings file \"{0}\" was not found in the current directory.\r\nPlease run the settings app to configure your bot.\r\n", SettingsFileName);
                    Console.Write(Message);
                    Console.ReadKey();
                    Environment.Exit(0);
                }
                if (string.IsNullOrWhiteSpace(MCoreFileContent))
                {
                    Message = string.Format("MCore file \"{0}\" was not found.\r\nPlease run the settings app to configure your bot.\r\n", Settings.MCoreFileName);
                }
                if (string.IsNullOrWhiteSpace(SongfileContent))
                {
                    Message = string.Format("Song file \"{0}\" was not found.\r\nPlease run the settings app to configure your bot.\r\n", Settings.SongFileName);
                }
                Console.Write(Message);
                Console.ReadKey();
                Environment.Exit(0);
            }
        }
        public static void ReloadSongFile()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(Settings.SongFileLocation))
                {
                    SongfileContent = File.ReadAllText(Settings.SongFileLocation);
                }
                else if (!string.IsNullOrWhiteSpace(Settings.SongFileName))
                {
                    SongfileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, Settings.SongFileLocation));
                }
            }
            catch (FileNotFoundException)
            {
                string Message = string.Empty;
                if (string.IsNullOrWhiteSpace(SongfileContent))
                {
                    Message = string.Format("Song file \"{0}\" was not found.\r\nPlease run the settings app to configure your bot.\r\n", Settings.SongFileName);
                }
                Console.Write(Message);
                Console.ReadKey();
                Environment.Exit(0);

            }
        }

        public static void MessageReceivedCallback(string IRCMessage)
        {
            if (!string.IsNullOrWhiteSpace(IRCMessage))
            {
                //:nickname!username@nickname.tmi.twitch.tv PRIVMSG #channel :message that was sent
                int FirstSpace = IRCMessage.IndexOf(' ');
                if (IRCMessage.Length > FirstSpace + 8
                    && IRCMessage.Substring(FirstSpace + 1, 7).Equals("PRIVMSG"))
                {
                    #region Chat Message Received
                    string[] IRCMessageArgs = IRCMessage.Split(' ');
                    int ExclamationPoint = IRCMessage.IndexOf('!');
                    int LastSemiColon = IRCMessage.LastIndexOf(':');
                    int Hashtag = IRCMessage.IndexOf('#');

                    string SenderNickname = IRCMessage.Substring(1, ExclamationPoint - 1);
                    string SenderUsername = IRCMessage.Substring(ExclamationPoint + 1, IRCMessage.IndexOf('@') - ExclamationPoint - 1);
                    string UserMessage = IRCMessage.Substring(LastSemiColon + 1);
                    string[] UserMessageArgs = UserMessage.Split(' ');
                    string Receiver = IRCMessage.Substring(Hashtag + 1, LastSemiColon - Hashtag - 2);
                    if (UserMessage[0].Equals('!'))
                    {
                        string Response = string.Empty;

                        switch (UserMessageArgs[0])
                        {
                            case "!commands":
                                {
                                    Response = "Available commands: !commands, !song, !level [csgo/dota2], !rank [csgo/dota2]\r\n";
                                    Core.SendMessage(Response, Core.Channel.Name);
                                }
                                break;

                            case "!song":
                                {
                                    ReloadSongFile();
                                    Response = string.Format("Current song is: {0}\r\n", SongfileContent);
                                    Core.SendMessage(Response, Core.Channel.Name);
                                }
                                break;

                            case "!level":
                                {
                                    if (UserMessageArgs.Length.Equals(2))
                                    {
                                        switch (UserMessageArgs[1])
                                        {
                                            case "dota2":
                                                {
                                                    Response = string.Format("The streamer's Dota2 Level is: {0}\r\n", Core.Streamer.Dota2Level);
                                                    Core.SendMessage(Response, Core.Channel.Name);
                                                }
                                                break;

                                            case "csgo":
                                                {
                                                    Response = string.Format("The streamer's CSGO Level is: {0}\r\n", Core.Streamer.CSGOLevel);
                                                    Core.SendMessage(Response, Core.Channel.Name);
                                                }
                                                break;
                                        }
                                    }
                                }
                                break;

                            case "!rank":
                                {
                                    if (UserMessageArgs.Length.Equals(2))
                                    {
                                        switch (UserMessageArgs[1])
                                        {
                                            case "dota2":
                                                {
                                                    Response = string.Format("The streamer's Dota2 MMR is: {0}\r\n", Core.Streamer.Dota2Rank);
                                                    Core.SendMessage(Response, Core.Channel.Name);
                                                }
                                                break;

                                            case "csgo":
                                                {
                                                    Response = string.Format("The streamer's CSGO MMR is: {0}\r\n", Core.Streamer.CSGORank);
                                                    Core.SendMessage(Response, Core.Channel.Name);
                                                }
                                                break;
                                        }
                                    }
                                }
                                break;

                            case "!automsg":
                                {
                                    if (Core.Channel.Moderators.Contains(SenderNickname))
                                    {
                                        if (UserMessageArgs.Length.Equals(2))
                                        {
                                            if (UserMessageArgs[1].ToLower().Equals("on"))
                                            {
                                                Core.MOTD.Enabled = true;
                                            }
                                            else if (UserMessageArgs[1].ToLower().Equals("off"))
                                            {
                                                Core.MOTD.Enabled = false;
                                            }
                                            else
                                            {
                                                Console.WriteLine(string.Format("Invalid argument \"{0}\" in command: {1}", UserMessageArgs[1], UserMessage));
                                            }
                                        }
                                        else if (UserMessageArgs.Length.Equals(3))
                                        {
                                            string Message = string.Join(" ", UserMessageArgs.Skip(1).Take(UserMessageArgs.Length - 1));
                                            Core.MOTD.Message = Message;
                                            int Frequency = 0;
                                            string FrequencyArg = UserMessageArgs.Last();
                                            if (int.TryParse(FrequencyArg, out Frequency))
                                            {
                                                Core.MOTD.Frequency = Frequency;
                                            }
                                            else
                                            {
                                                Console.WriteLine(string.Format("Invalid argument \"{0}\" in command: {1}", FrequencyArg, UserMessage));
                                            }
                                        }
                                    }
                                }
                                break;
                        }
                    }
                }
                #endregion
            }
        }
    }
}