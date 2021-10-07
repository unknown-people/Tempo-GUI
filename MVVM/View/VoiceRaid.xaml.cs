using Discord;
using Discord.Gateway;
using Leaf.xNet;
using Music_user_bot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TempoWithGUI.MVVM.ViewModel;

namespace TempoWithGUI.MVVM.View
{
    /// <summary>
    /// Interaction logic for VoiceRaid.xaml
    /// </summary>
    public partial class VoiceRaid : Window
    {
        public static bool playMusic { get; set; } = false;
        public static bool spamJoin { get; set; } = false;
        public static bool isJoined { get; set; } = false;
        public VoiceRaid()
        {
            InitializeComponent();
            Set_Light(isJoined);
            SpamCB.IsChecked = spamJoin;
            playCB.IsChecked = playMusic;
            if (playMusic)
            {
                FileIn.Visibility = Visibility.Visible;
                FileLabel.Visibility = Visibility.Visible;
            }
            else
            {
                FileIn.Visibility = Visibility.Collapsed;
                FileLabel.Visibility = Visibility.Collapsed;
            }
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            StatusLight.Fill = Brushes.Red;
            isJoined = false;
            RaidModel.voiceOn = false;
            this.Close();
        }
        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (isJoined)
                return;
            StartBtn.Cursor = Cursors.AppStarting;
            var channelId = ChannelIn.Text;
            var guildId = GuildIn.Text;
            if ((channelId == null || channelId == "") || (guildId == null || guildId == ""))
            {
                StartBtn.Cursor = Cursors.Arrow;
                return;
            }
            ulong channel_id = 0;
            ulong guild_id = 0;
            if(!ulong.TryParse(channelId, out channel_id))
            {
                MessageBox.Show("Please insert a valid channel ID");
                StartBtn.Cursor = Cursors.Arrow;
                return;
            }
            if (!ulong.TryParse(guildId, out guild_id))
            {
                MessageBox.Show("Please insert a valid guild ID");
                StartBtn.Cursor = Cursors.Arrow;
                return;
            }
            if (!float.TryParse(DelayIn.Text, out var delay) || (!int.TryParse(TokensIn.Text, out var tokens)))
            {
                delay = 250;
                tokens = 0;
            }
            var path = FileIn.Text;
            if (!File.Exists(path) && playMusic)
            {
                MessageBox.Show("Please insert a valid file path");
                StartBtn.Cursor = Cursors.Arrow;
                return;
            }
            isJoined = true;
            var max_tokens = TokensIn.Text;
            int max = 0;
            if (!(max_tokens == null || max_tokens.ToString().Trim('\n') == ""))
                if(!int.TryParse(max_tokens, out max))
                {
                    MessageBox.Show("Please insert a valid value for max tokens");
                    StartBtn.Cursor = Cursors.Arrow;
                    return;
                }
            StatusLight.Fill = Brushes.Green;
            StartBtn.Cursor = Cursors.Arrow;
            Task.Run(() =>
            {
                var token_list = new List<string>() { };
                using (StreamReader reader = new StreamReader(App.strWorkPath + "\\tokens\\tokens.txt"))
                {
                    var line = reader.ReadLine();
                    while (line != null && line.Trim('\n') != "")
                    {
                        var token_arr = line.Split(':');
                        if (token_arr.Length == 3)
                        {
                            token_list.Add(token_arr[0]);
                        }
                        else
                        {
                            token_list.Add(token_arr[1]);
                        }
                        line = reader.ReadLine();
                    }
                }
                List<DiscordSocketClient> clients = new List<DiscordSocketClient>();
                foreach (var token in token_list)
                {
                    var client = new DiscordSocketClient(new DiscordSocketConfig()
                    {
                        ApiVersion = 9,
                        HandleIncomingMediaData = false,
                        Intents = DiscordGatewayIntent.Guilds | DiscordGatewayIntent.GuildMessages | DiscordGatewayIntent.GuildVoiceStates
                    });
                    client.Login(token);
                    clients.Add(client);
                    if (max > 0)
                        if (clients.Count == max)
                            break;
                }
                int i = 0;
                foreach (var client in clients)
                {
                    if (!isJoined)
                        break;

                    bool hasJoined = false;
                    int c = 0;
                    while (!hasJoined && c < 3)
                    {
                        try
                        {
                            var voiceClient = ((DiscordSocketClient)client).GetVoiceClient(guild_id);
                            voiceClient.Connect(channel_id);
                            if (playMusic)
                            {
                                path = path.Replace('\\', '/');

                                var task = Task.Run(() => {
                                    while (voiceClient.Microphone == null)
                                        Thread.Sleep(100);
                                    voiceClient.Microphone.CopyFromRaid(path, 3600);
                                    int s = 0;
                                    while (s < 5)
                                    {
                                        try
                                        {
                                            voiceClient.Disconnect();
                                        }
                                        catch { Thread.Sleep(500); s++; }
                                    }
                                    Dispatcher.Invoke(() => Set_Light(false));
                                    client.Logout();
                                });
                            }
                            hasJoined = true;
                        }
                        catch (Exception ex)
                        {
                            c++;
                            continue;
                        }
                    }
                    Thread.Sleep((int)delay);
                }
            });
        }
        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            StatusLight.Fill = Brushes.Red;
            isJoined = false;
        }
        private void Set_Light(bool status)
        {
            if (status)
            {
                StatusLight.Fill = Brushes.Green;
            }
            else
            {
                StatusLight.Fill = Brushes.Red;
            }
        }

        private void playCB_Click(object sender, RoutedEventArgs e)
        {
            playMusic = !playMusic;
            if (playMusic)
            {
                FileIn.Visibility = Visibility.Visible;
                FileLabel.Visibility = Visibility.Visible;
            }
            else
            {
                FileIn.Visibility = Visibility.Collapsed;
                FileLabel.Visibility = Visibility.Collapsed;
            }
        }
    }
}
