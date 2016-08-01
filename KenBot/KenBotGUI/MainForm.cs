using System;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using KenBotClient;
using System.IO;
using Newtonsoft.Json;

namespace KenBotConfig
{
    public partial class MainForm : Form
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

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Initialize();
        }

        public void Initialize()
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

                txtBoxBotUsername.Text = MCoreJSON["Bot"]["UserName"].ToString();
                txtBoxBotPassword.Text = MCoreJSON["Bot"]["OAuthToken"].ToString();
                nudCommandsFrequency.Value = decimal.Parse(CommandCollectionJSON["!commands"]["Frequency"].ToString());
                nudSongFrequency.Value = decimal.Parse(CommandCollectionJSON["!song"]["Frequency"].ToString());
                nudRankDota2Frequency.Value = decimal.Parse(CommandCollectionJSON["!rank dota2"]["Frequency"].ToString());
                nudRankCSGOFrequency.Value = decimal.Parse(CommandCollectionJSON["!rank csgo"]["Frequency"].ToString());
                nudLevelDota2Frequency.Value = decimal.Parse(CommandCollectionJSON["!level dota2"]["Frequency"].ToString());
                nudLevelCSGOFrequency.Value = decimal.Parse(CommandCollectionJSON["!level csgo"]["Frequency"].ToString());
                nudStreamerRankDota2.Value = decimal.Parse(MCoreJSON["Streamer"]["Dota2Rank"].ToString());
                nudStreamerRankCSGO.Value = decimal.Parse(MCoreJSON["Streamer"]["CSGORank"].ToString());
                nudStreamerLevelDota2.Value = decimal.Parse(MCoreJSON["Streamer"]["Dota2Level"].ToString());
                nudStreamerLevelCSGO.Value = decimal.Parse(MCoreJSON["Streamer"]["CSGOLevel"].ToString());
                txtBoxSettingsChannelName.Text = MCoreJSON["Channel"]["Name"].ToString();
                txtBoxSettingsOwnerDisplayName.Text = MCoreJSON["Channel"]["OwnerNickname"].ToString();
                txtBoxAutoMessageMessage.Text = MCoreJSON["MessageOfTheDay"]["Message"].ToString();
                nudAutoMessageFrequency.Value = decimal.Parse(MCoreJSON["MessageOfTheDay"]["Frequency"].ToString());
                chkBoxAutoMessageEnabled.Enabled = ((bool)MCoreJSON["MessageOfTheDay"]["Enabled"] == true);
                chkBoxAutoMessageEnabled.Checked = chkBoxAutoMessageEnabled.Enabled;
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

        private void btnSaveChanges_Click(object sender, EventArgs e)
        {
            MCoreJSON["Bot"]["UserName"].Replace(txtBoxBotUsername.Text);
            MCoreJSON["Bot"]["OAuthToken"].Replace(txtBoxBotPassword.Text);
            CommandCollectionJSON["!commands"]["Frequency"].Replace((int.Parse(nudCommandsFrequency.Text)));
            CommandCollectionJSON["!song"]["Frequency"].Replace(int.Parse(nudSongFrequency.Text));
            CommandCollectionJSON["!rank dota2"]["Frequency"].Replace(int.Parse(nudRankDota2Frequency.Text));
            CommandCollectionJSON["!rank csgo"]["Frequency"].Replace(int.Parse(nudRankCSGOFrequency.Text));
            CommandCollectionJSON["!level dota2"]["Frequency"].Replace(int.Parse(nudLevelDota2Frequency.Text));
            CommandCollectionJSON["!level csgo"]["Frequency"].Replace(int.Parse(nudLevelCSGOFrequency.Text));
            MCoreJSON["Streamer"]["Dota2Rank"].Replace(int.Parse(nudStreamerRankDota2.Text));
            MCoreJSON["Streamer"]["CSGORank"].Replace(int.Parse(nudStreamerRankCSGO.Text));
            MCoreJSON["Streamer"]["Dota2Level"].Replace(int.Parse(nudStreamerLevelDota2.Text));
            MCoreJSON["Streamer"]["CSGOLevel"].Replace(int.Parse(nudStreamerLevelCSGO.Text));
            MCoreJSON["Channel"]["Name"].Replace(txtBoxSettingsChannelName.Text);
            MCoreJSON["Channel"]["OwnerNickname"].Replace(txtBoxSettingsOwnerDisplayName.Text);
            MCoreJSON["MessageOfTheDay"]["Message"].Replace(txtBoxAutoMessageMessage.Text);
            MCoreJSON["MessageOfTheDay"]["Frequency"].Replace(int.Parse(nudAutoMessageFrequency.Text));
            MCoreJSON["MessageOfTheDay"]["Enabled"].Replace(chkBoxAutoMessageEnabled.Enabled);
            try
            {
                File.WriteAllText(Settings.MCoreFilePath, JsonConvert.SerializeObject(MCoreJSON));
                File.WriteAllText(Settings.CommandCollectionFilePath, JsonConvert.SerializeObject(CommandCollectionJSON));
                MessageBox.Show("Changes saved.");
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Can't access the file, it may be opened. Please close all the files related to this application. (MCore.json, MSettings.json, CommandCollection.json)");
            }
        }
    }
}
