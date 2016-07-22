using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using KenBot;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

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

        public static FileStream SongFileStream;
        public static FileStream MSettingsFileStream;
        public static FileStream MCoreFileStream;
        public static FileStream CommandCollectionFileStream;

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
            StreamReader FileReader;
            try
            {
                MSettingsFileStream = new FileStream(Path.Combine(Environment.CurrentDirectory, "Data", MSettingsFileName), FileMode.Open);
                MSettingsFileStream.Lock(0, MSettingsFileStream.Length);
                FileReader = new StreamReader(MSettingsFileStream);
                MSettingsFileContent = FileReader.ReadToEnd();
                Settings = JsonConvert.DeserializeObject<MSettings>(MSettingsFileContent);
                SettingsJSON = JObject.Parse(MSettingsFileContent);

                Settings.MCoreFilePath = Path.Combine(Environment.CurrentDirectory, "Data", Settings.MCoreFileName);
                MCoreFileStream = new FileStream(Settings.MCoreFilePath, FileMode.Open);
                MCoreFileStream.Lock(0, MSettingsFileStream.Length);
                FileReader = new StreamReader(MCoreFileStream);
                MCoreFileContent = FileReader.ReadToEnd();
                Core = JsonConvert.DeserializeObject<MCore>(MCoreFileContent);
                MCoreJSON = JObject.Parse(MCoreFileContent);

                Settings.SongFilePath = Path.Combine(Environment.CurrentDirectory, "Data", Settings.SongFileName);
                SongFileStream = new FileStream(Settings.SongFilePath, FileMode.Open);
                SongFileStream.Lock(0, SongFileStream.Length);
                FileReader = new StreamReader(SongFileStream);
                SongFileContent = FileReader.ReadToEnd();

                Settings.CommandCollectionFilePath = Path.Combine(Environment.CurrentDirectory, "Data", Settings.CommandCollectionFileName);
                CommandCollectionFileStream = new FileStream(Settings.CommandCollectionFilePath, FileMode.Open);
                CommandCollectionFileStream.Lock(0, CommandCollectionFileStream.Length);
                FileReader = new StreamReader(CommandCollectionFileStream);
                CommandCollectionFileContent = FileReader.ReadToEnd(); FileReader.Close();
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
                SongFileStream = new FileStream(Settings.SongFilePath, FileMode.Open);
                using (StreamReader FileReader = new StreamReader(SongFileStream))
                {
                    SongFileContent = FileReader.ReadToEnd();
                }
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
                MCoreFileStream = new FileStream(Settings.MCoreFilePath, FileMode.Open);
                using (StreamReader FileReader = new StreamReader(MCoreFileStream))
                {
                    MCoreFileContent = FileReader.ReadToEnd();
                }
            }
            catch (FileNotFoundException)
            {
                string Message = string.Empty;
                if (string.IsNullOrWhiteSpace(SongFileContent))
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
                CommandCollectionFileStream = new FileStream(Settings.CommandCollectionFilePath, FileMode.Open);
                using (StreamReader FileReader = new StreamReader(CommandCollectionFileStream))
                {
                    CommandCollectionFileContent = FileReader.ReadToEnd();
                }
            }
            catch (FileNotFoundException)
            {
                string Message = string.Empty;
                if (string.IsNullOrWhiteSpace(SongFileContent))
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
                int FirstSpace = _IRCMessage.IndexOf(' ');

                if (_IRCMessage.Length > 4)
                {
                    if (_IRCMessage.Substring(0, 4).Equals("PING"))
                    {
                        Response = _IRCMessage.Replace("PING", "PONG");
                        Core.IO.AttemptWrite(Response);
                        return;
                    }
                }
                if (!FirstSpace.Equals(-1)
                   && _IRCMessage.Length > FirstSpace + 8
                   && _IRCMessage.Substring(FirstSpace + 1, 7).Equals("PRIVMSG"))
                {
                    #region Chat Message Received
                    string[] IRCMessageArgs = _IRCMessage.Split(' ');
                    int ExclamationPoint = _IRCMessage.IndexOf('!');
                    int LastSemiColon = _IRCMessage.LastIndexOf(':');
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
                                            MCoreFileStream.SetLength(0);
                                            using (StreamWriter FileWriter = new StreamWriter(MCoreFileStream))
                                            {
                                                FileWriter.Write(JsonConvert.SerializeObject(MCoreJSON));
                                            }
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
                                                    return;
                                                }
                                                MCoreFileStream.SetLength(0);
                                                using (StreamWriter FileWriter = new StreamWriter(MCoreFileStream))
                                                {
                                                    FileWriter.Write(JsonConvert.SerializeObject(MCoreJSON));
                                                }
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
                #endregion
            }
        }
    }
}