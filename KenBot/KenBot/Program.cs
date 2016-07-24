using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using KenBot;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kenbot
{
    static class Program
    {
        public static MSettings Settings;
        public static MCore Core;
        public static Dictionary<string, MCommand> CommandCollection;

        public static JObject MCoreJSON;
        public static JObject SettingsJSON;
        public static JObject CommandCollectionJSON;

        public static string MSettingsFileName = "MSettings.json";
        public static string MSettingsFileContent = string.Empty;
        public static string MCoreFileContent = string.Empty;
        public static string CommandCollectionFileContent = string.Empty;
        public static string SongFileContent = string.Empty;

        static void Main()
        {
            Initialize();

            Core.IO.StartReading();

            Console.ReadKey();
        }

        public static void Initialize()
        {
            try
            {
                MSettingsFileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Data", MSettingsFileName));
                Settings = JsonConvert.DeserializeObject<MSettings>(MSettingsFileContent);
                SettingsJSON = JObject.Parse(MSettingsFileContent);

                Settings.MCoreFilePath = Path.Combine(Environment.CurrentDirectory, "Data", Settings.MCoreFileName);
                MCoreFileContent = File.ReadAllText(Settings.MCoreFilePath);
                Core = JsonConvert.DeserializeObject<MCore>(MCoreFileContent);
                MCoreJSON = JObject.Parse(MCoreFileContent);

                Settings.SongFilePath = Path.Combine(Environment.CurrentDirectory, "Data", Settings.SongFileName);
                SongFileContent = File.ReadAllText(Settings.SongFilePath);

                Settings.CommandCollectionFilePath = Path.Combine(Environment.CurrentDirectory, "Data", Settings.CommandCollectionFileName);
                CommandCollectionFileContent = File.ReadAllText(Settings.CommandCollectionFilePath);
                CommandCollection = JsonConvert.DeserializeObject<Dictionary<string, MCommand>>(CommandCollectionFileContent);
                CommandCollectionJSON = JObject.Parse(CommandCollectionFileContent);

                Core.IO.MessageReceived += MessageReceivedCallback;

                Core.IO.Initialize();
                Core.ConnectToIRC();

                string JoinChannelResponse = Core.JoinChannel(Core.Channel.Name, Core.IO);
                Core.Channel.Viewers = MHelper.GetViewers(JoinChannelResponse);

                string ModsCommandResponse = Core.ListMods();
                Core.Channel.Moderators = MHelper.GetModerators(ModsCommandResponse);

                Core.SendMessage("Bot is on duty.", Core.Channel.Name, Core.Channel.OwnerNickname);
                Console.WriteLine("Bot is on duty.");

                Core.MessageOfTheDay.SendMessageMethod = Core.SendMessage;
                Core.MessageOfTheDay.ChannelName = Core.Channel.Name;
                Core.MessageOfTheDay.AttemptStartPosting();
            }
            catch (FileNotFoundException)
            {
                string Message = string.Empty;
                if (string.IsNullOrWhiteSpace(MSettingsFileContent))
                {
                    Message += string.Format("Settings file \"{0}\" was not found in the current directory.\r\n", MSettingsFileName);
                    Console.Write(Message);
                    Console.ReadKey();
                    Environment.Exit(0);
                }
                if (string.IsNullOrWhiteSpace(MCoreFileContent))
                {
                    Message += string.Format("MCore file \"{0}\" was not found.\r\n", Settings.MCoreFileName);
                }
                if (string.IsNullOrWhiteSpace(SongFileContent))
                {
                    Message += string.Format("Song file \"{0}\" was not found.\r\n", Settings.SongFileName);
                }
                if (string.IsNullOrWhiteSpace(CommandCollectionFileContent))
                {
                    Message += string.Format("Commands file \"{0}\" was not found.\r\n", Settings.CommandCollectionFileName);
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
                SongFileContent = File.ReadAllText(Settings.SongFilePath);
            }
            catch (FileNotFoundException)
            {
                string Message = string.Empty;
                if (string.IsNullOrWhiteSpace(SongFileContent))
                {
                    Message = string.Format("Song file \"{0}\" was not found.\r\n", Settings.SongFileName);
                }
                Console.Write(Message);
                Console.ReadKey();
                Environment.Exit(0);
            }
        }

        public static void ReloadCoreFile()
        {
            try
            {
                MCoreFileContent = File.ReadAllText(Settings.MCoreFilePath);
            }
            catch (FileNotFoundException)
            {
                string Message = string.Empty;
                if (string.IsNullOrWhiteSpace(MCoreFileContent))
                {
                    Message = string.Format("MCore file \"{0}\" was not found.\r\n", Settings.MCoreFileName);
                }
                Console.Write(Message);
                Console.ReadKey();
                Environment.Exit(0);
            }
        }

        public static void ReloadCommandsFile()
        {
            try
            {
                CommandCollectionFileContent = File.ReadAllText(Settings.CommandCollectionFilePath);
            }
            catch (FileNotFoundException)
            {
                string Message = string.Empty;
                if (string.IsNullOrWhiteSpace(CommandCollectionFileContent))
                {
                    Message = string.Format("Commands file \"{0}\" was not found.\r\n", Settings.CommandCollectionFileName);
                }
                Console.Write(Message);
                Console.ReadKey();
                Environment.Exit(0);
            }
        }

        public static void MessageReceivedCallback(string _IRCMessage)
        {
            Console.WriteLine(string.Concat("<", _IRCMessage));
            if (!string.IsNullOrWhiteSpace(_IRCMessage))
            {
                string Response = string.Empty;
                int IndexOfFirstSpace = _IRCMessage.IndexOf(' ');
                int IndexOfPRIVMSG = _IRCMessage.IndexOf("PRIVMSG");

                if (_IRCMessage.Length > 4)
                {
                    if (_IRCMessage.Substring(0, 4).Equals("PING"))
                    {
                        Response = _IRCMessage.Replace("PING", "PONG");
                        Core.IO.AttemptWrite(Response);
                        return;
                    }
                }
                if (!IndexOfFirstSpace.Equals(-1)
                   && _IRCMessage.Length > IndexOfFirstSpace + 8
                   && !IndexOfPRIVMSG.Equals(-1))
                {
                    string[] IRCMessageArgs = _IRCMessage.Split(' ');
                    int ExclamationPoint = _IRCMessage.IndexOf('!');
                    int LastSemiColon = _IRCMessage.IndexOf(':', IndexOfPRIVMSG);
                    int Hashtag = _IRCMessage.IndexOf('#');

                    string SenderNickname = _IRCMessage.Substring(1, ExclamationPoint - 1);
                    string SenderUsername = _IRCMessage.Substring(ExclamationPoint + 1, _IRCMessage.IndexOf('@') - ExclamationPoint - 1);
                    string UserMessage = _IRCMessage.Substring(LastSemiColon + 1);
                    UserMessage = UserMessage.Remove(UserMessage.Length - 2);
                    string[] UserMessageArgs = UserMessage.Split(' ');
                    string Receiver = _IRCMessage.Substring(Hashtag + 1, LastSemiColon - Hashtag - 2);

                    if (UserMessage[0].Equals('!'))
                    {
                        switch (UserMessageArgs[0])
                        {
                            case "!commands":
                                {
                                    if (CommandCollection.ContainsKey("!commands"))
                                    {
                                        if (CommandCollection["!commands"].IsAvailable)
                                        {
                                            Response = "Available commands: !commands, !song, !level [csgo/dota2], !rank [csgo/dota2]\r\n";
                                            Core.SendMessage(Response, Core.Channel.Name);
                                            Console.WriteLine(string.Concat(">", Response));
                                            Task.Run(new Action(CommandCollection["!commands"].AttemptRunCooldown));
                                            return;
                                        }
                                    }
                                }
                                break;

                            case "!song":
                                {
                                    if (CommandCollection.ContainsKey("!song"))
                                    {
                                        if (CommandCollection["!song"].IsAvailable)
                                        {
                                            ReloadSongFile();
                                            Response = string.Format("Current song is: {0}\r\n", SongFileContent);
                                            Core.SendMessage(Response, Core.Channel.Name);
                                            Console.WriteLine(string.Concat(">", Response));
                                            CommandCollection["!song"].AttemptRunCooldown();
                                            return;
                                        }
                                    }
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
                                                    if (CommandCollection.ContainsKey("!level dota2"))
                                                    {
                                                        if (CommandCollection["!level dota2"].IsAvailable)
                                                        {
                                                            Response = string.Format("The streamer's Dota2 Level is: {0}\r\n", Core.Streamer.Dota2Level);
                                                            Core.SendMessage(Response, Core.Channel.Name);
                                                            Console.WriteLine(string.Concat(">", Response));
                                                            Task.Run(new Action(CommandCollection["!commands"].AttemptRunCooldown));
                                                            return;
                                                        }
                                                    }
                                                }
                                                break;

                                            case "csgo":
                                                {
                                                    if (CommandCollection.ContainsKey("!level csgo"))
                                                    {
                                                        if (CommandCollection["!level csgo"].IsAvailable)
                                                        {
                                                            Response = string.Format("The streamer's CSGO Level is: {0}\r\n", Core.Streamer.CSGOLevel);
                                                            Core.SendMessage(Response, Core.Channel.Name);
                                                            Console.WriteLine(string.Concat(">", Response));
                                                            Task.Run(new Action(CommandCollection["!commands"].AttemptRunCooldown));
                                                            return;
                                                        }
                                                    }
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
                                                    if (CommandCollection.ContainsKey("!rank dota2"))
                                                    {
                                                        if (CommandCollection["!rank dota2"].IsAvailable)
                                                        {
                                                            Response = string.Format("The streamer's Dota2 MMR is: {0}\r\n", Core.Streamer.Dota2Rank);
                                                            Core.SendMessage(Response, Core.Channel.Name);
                                                            Console.WriteLine(string.Concat(">", Response));
                                                            Task.Run(new Action(CommandCollection["!commands"].AttemptRunCooldown));
                                                            return;
                                                        }
                                                    }
                                                }
                                                break;

                                            case "csgo":
                                                {
                                                    if (CommandCollection.ContainsKey("!rank csgo"))
                                                    {
                                                        if (CommandCollection["!rank csgo"].IsAvailable)
                                                        {
                                                            Response = string.Format("The streamer's CSGO MMR is: {0}\r\n", Core.Streamer.CSGORank);
                                                            Core.SendMessage(Response, Core.Channel.Name);
                                                            Console.WriteLine(string.Concat(">", Response));
                                                            Task.Run(new Action(CommandCollection["!commands"].AttemptRunCooldown));
                                                            return;
                                                        }
                                                    }
                                                }
                                                break;
                                        }
                                    }
                                }
                                break;

                            case "!automsg":
                                {
                                    if (Core.Channel.Moderators.Contains(SenderNickname.ToLower()))
                                    {
                                        if (UserMessageArgs.Length.Equals(2))
                                        {
                                            if (UserMessageArgs[1].ToLower().Equals("on"))
                                            {
                                                Core.MessageOfTheDay.Enabled = true;
                                                MCoreJSON["MessageOfTheDay"]["Enabled"] = true;
                                                Core.MessageOfTheDay.Update();
                                            }
                                            else if (UserMessageArgs[1].ToLower().Equals("off"))
                                            {
                                                Core.MessageOfTheDay.Enabled = false;
                                                MCoreJSON["MessageOfTheDay"]["Enabled"] = false;
                                                Core.MessageOfTheDay.Update();
                                            }
                                            else
                                            {
                                                Response = string.Format("Invalid argument \"{0}\" in command: {1}", UserMessageArgs[1], UserMessage);
                                                Core.SendMessage(Response, Core.Channel.Name, SenderNickname);
                                                Console.WriteLine(string.Concat(">", Response));
                                                return;
                                            }
                                            File.WriteAllText(Settings.MCoreFilePath, JsonConvert.SerializeObject(MCoreJSON));
                                            ReloadCoreFile();
                                            return;
                                        }
                                        else
                                        {
                                            string[] ReformedUserMessageArgs = new string[3];
                                            ReformedUserMessageArgs[0] = UserMessageArgs[0];
                                            ReformedUserMessageArgs[1] = string.Join(" ", UserMessageArgs.Skip(1).Take(UserMessageArgs.Length - 2));
                                            ReformedUserMessageArgs[2] = UserMessageArgs[UserMessageArgs.Length - 1];

                                            if (ReformedUserMessageArgs.Length.Equals(3))
                                            {
                                                string Message = ReformedUserMessageArgs[1];
                                                Core.MessageOfTheDay.Message = Message;
                                                MCoreJSON["MessageOfTheDay"]["Message"] = Message;

                                                int Frequency = 0;
                                                string FrequencyArg = UserMessageArgs.Last();
                                                if (int.TryParse(FrequencyArg, out Frequency))
                                                {
                                                    Core.MessageOfTheDay.Frequency = Frequency;
                                                    MCoreJSON["MessageOfTheDay"]["Frequency"] = Frequency;
                                                    Core.MessageOfTheDay.Update();
                                                }
                                                else
                                                {
                                                    Response = string.Format("Invalid argument \"{0}\" in command: {1}", FrequencyArg, UserMessage);
                                                    Core.SendMessage(Response, Core.Channel.Name, SenderNickname);
                                                    Console.WriteLine(string.Concat(">", Response));
                                                }
                                                File.WriteAllText(Settings.MCoreFilePath, JsonConvert.SerializeObject(MCoreJSON));
                                                ReloadCoreFile();
                                                return;
                                            }
                                        }
                                    }
                                }
                                break;
                        }
                    }
                }
            }
        }
    }
}