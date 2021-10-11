using Discord;
using Discord.Gateway;
using Discord.Media;
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
using YoutubeExplode;

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
            if (!float.TryParse(DelayIn.Text, out var delay))
            {
                delay = 250;
            }
            var path = FileIn.Text;
            if (!path.StartsWith("http"))
            {
                if (path.StartsWith("https://youtu.be/"))
                    path = path.Split('/').Last();
                if (!File.Exists(path) && playMusic)
                {
                    MessageBox.Show("Please insert a valid file path");
                    StartBtn.Cursor = Cursors.Arrow;
                    return;
                }
            }
            else
            {
                var videoId = path;
                if(path.StartsWith("http"))
                    videoId = path.Split('?').Last().Split('&')[0].Remove(0, 2);
                try
                {
                    path = TrackQueue.GetAudioUrl(videoId);
                }
                catch
                {
                    MessageBox.Show("Please insert a valid url/file");
                    StartBtn.Cursor = Cursors.Arrow;
                    return;
                }
            }
            isJoined = true;
            spamJoin = (bool)SpamCB.IsChecked;
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
            Thread spam = new Thread(() =>
            {
                var token_list = new List<string>() { };
                foreach (var tk in tokens._tokens)
                {
                    if (tk.Active)
                        token_list.Add(tk.Token);
                }
                List<DiscordSocketClient> clients = new List<DiscordSocketClient>();
                foreach (var token in token_list)
                {
                    var client = new DiscordSocketClient(new DiscordSocketConfig()
                    {
                        ApiVersion = 9,
                        HandleIncomingMediaData = false,
                        Intents = DiscordGatewayIntent.Guilds | DiscordGatewayIntent.GuildMessages | DiscordGatewayIntent.GuildVoiceStates
                    }, false);
                    try
                    {
                        client.Login(token);
                        clients.Add(client);
                    }
                    catch { }
                    if (max > 0)
                        if (clients.Count == max)
                            break;
                }
                int i = 0;
                DiscordVoiceClient voiceClient;
                Parallel.ForEach(clients, client =>
                {
                    int c = 0;
                    while(c<3 && client.State < GatewayConnectionState.Connected)
                    {
                        Thread.Sleep(1000);
                        c++;
                    }
                    if (client.State == GatewayConnectionState.Connected)
                    {
                        voiceClient = ((DiscordSocketClient)client).GetVoiceClient(guild_id);
                        voiceClient.Connect(channel_id);
                    }
                    
                });
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
                            voiceClient = ((DiscordSocketClient)client).GetVoiceClient(guild_id);
                            if (playMusic && !spamJoin)
                            {
                                path = path.Replace('\\', '/');

                                var task = Task.Run(() => {
                                    while (voiceClient.Microphone == null)
                                        Thread.Sleep(100);
                                    voiceClient.Microphone.CopyFromRaid(path, 3600);
                                    while (isJoined)
                                        Thread.Sleep(100);
                                    int s = 0;
                                    while (s < 5)
                                    {
                                        try
                                        {
                                            voiceClient.Disconnect();
                                            break;
                                        }
                                        catch { Thread.Sleep(200); s++; }
                                    }
                                    Dispatcher.Invoke(() => Set_Light(false));
                                    clients.Remove(client);
                                    client.Dispose();
                                    isJoined = false;
                                });
                            }
                            else
                            {
                                Task.Run(() =>
                                {
                                    while(isJoined)
                                        Thread.Sleep(100);
                                    try
                                    {
                                        voiceClient.Disconnect();
                                    }
                                    catch { }
                                    Dispatcher.Invoke(() => Set_Light(false));
                                    clients.Remove(client);
                                    client.Dispose();
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
                if (spamJoin)
                {
                    foreach(var client in clients)
                    {
                        voiceClient = ((DiscordSocketClient)client).GetVoiceClient(guild_id);

                        while (isJoined)
                        {
                            voiceClient = ((DiscordSocketClient)client).GetVoiceClient(guild_id);
                            try
                            {
                                voiceClient.Disconnect();

                                voiceClient.Connect(channel_id);

                                while (voiceClient.State < Discord.Media.MediaConnectionState.Ready)
                                    Thread.Sleep(100);
                                break;
                            }
                            catch (Exception ex)
                            {  }
                            Thread.Sleep((int)delay);
                        }
                        Dispatcher.Invoke(() => Set_Light(false));
                        clients.Remove(client);
                        client.Dispose();
                    };
                }
            });
            spam.Start();
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
