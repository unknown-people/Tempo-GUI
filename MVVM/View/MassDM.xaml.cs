using Discord;
using Discord.Gateway;
using Leaf.xNet;
using Microsoft.WindowsAPICodePack.Dialogs;
using Music_user_bot;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
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
    /// Interaction logic for MassDM.xaml
    /// </summary>
    public partial class MassDM : System.Windows.Window
    {
        public static List<ulong> members { get; set; }
        public static bool spamming { get; set; }
        public static bool setupCompleted { get; set; }
        public MassDM()
        {
            InitializeComponent();
            Debug.Log("Mass DM interface initialized");
            Set_Light(spamming);
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            RaidModel.massdmOn = false;
            this.Hide();
        }
        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (spamming)
                return;
            bool proxyOn = (bool)ProxyCB.IsChecked;
            setupCompleted = false;
            if (Proxies.freeProxies && proxyOn)
            {
                MessageBox.Show("Mass DM spam is currently not supported with free proxies");
                StartBtn.Cursor = Cursors.Arrow;
                return;
            }
            StartBtn.Cursor = Cursors.AppStarting;
            var guild_id = GuildIn.Text;
            var channel_id = ChannelIn.Text;
            ulong channelId = 0;
            ulong guildId;
            if (guild_id == null || guild_id == "")
            {
                StartBtn.Cursor = Cursors.Arrow;
                return;
            }
            if (channel_id == null || channel_id == "")
            {
                channel_id = "0";
            }
            var message = MessageIn.Text;
            if (message == null || message == "")
            {
                StartBtn.Cursor = Cursors.Arrow;
                return;
            }
            if (!(ulong.TryParse(guild_id, out guildId) && ulong.TryParse(channel_id, out channelId)))
            {
                MessageBox.Show("Please use a valid guild/channel ID");
                return;
            }
            if (!float.TryParse(DelayIn.Text, out var delay))
            {
                delay = 10000;
            }
            bool embedOn = (bool)EmbedCB.IsChecked;
            string json = null;
            if (embedOn)
            {
                json = MessageIn.Text;
            }
            var max_tokens = TokensIn.Text;

            int max = 0;
            if (!(max_tokens == null || max_tokens.ToString() == ""))
            {
                if (!int.TryParse(max_tokens, out max))
                {
                    MessageBox.Show("Please insert a valid value for max tokens");
                    StartBtn.Cursor = Cursors.Arrow;
                    return;
                }
            }
            bool useFile = (bool)FileCB.IsChecked;

            delay = (int)delay;
            spamming = true;
            int sent = 0;
            StatusLight.Fill = Brushes.Green;
            StartBtn.Cursor = Cursors.Arrow;
            try
            {
                MainViewModel.log.Show();
            }
            catch (Exception ex)
            {
                MainViewModel.log.Visibility = Visibility.Visible;
            }
            Random random = new Random();
            var proxies_list = Proxy.working_proxies;
            if (Proxies.paidProxies)
                proxies_list = Proxy.working_proxies_paid;
            List<DiscordClient> clients = new List<DiscordClient>();
            var rnd = new Random();

            Thread spam = new Thread(() =>
            {
                var original_proxies = proxies_list;
                CustomMessageBox popup = null;
                var token_list = new List<string>() { };

                foreach (var tk in tokens._tokens)
                {
                    if (tk.Active)
                        token_list.Add(tk.Token);
                }
                Dispatcher.Invoke(() =>
                {
                    App.mainView.logPrint($"Loading {token_list.Count} clients into memory");
                });
                foreach (var token in token_list)
                {
                    try
                    {
                        clients.Add(new DiscordClient(token));
                    }
                    catch (Exception ex) { }
                    if (max > 0)
                        if (clients.Count >= max)
                            break;
                }
                Dispatcher.Invoke(() =>
                {
                    App.mainView.logPrint($"Loaded {token_list.Count} valid clients into memory");
                });
                Proxy proxy = null;

                members = new List<ulong>() { };
                if (members == null || members.Count == 0 && !useFile)
                {
                    var client_s = new DiscordSocketClient(new DiscordSocketConfig() { ApiVersion = 9 });

                    while (spamming)
                    {
                        try
                        {
                            int tries = 0;
                            client_s.Login(token_list[rnd.Next(0, token_list.Count)]);
                            break;
                        }
                        catch (Exception ex)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                if (popup != null)
                                    return;
                                popup = new CustomMessageBox("Your IP is banned from this guild or from Discord. To fetch the member list Tempo cannot use proxies, therefore you should enable a VPN and try again");
                                popup.ShowDialog();
                                Set_Light(false);
                            });
                        }
                    }

                    while (client_s.State < GatewayConnectionState.Connected)
                        Thread.Sleep(100);
                    var members1 = new List<GuildMember>() { };
                    SocketGuild guild = null;
                    while (true)
                    {
                        try
                        {
                            guild = client_s.GetCachedGuild(guildId);
                            if(guild == null)
                                guild = (SocketGuild)client_s.GetGuild(guildId);

                            Dispatcher.Invoke(() =>
                            {
                                App.mainView.logPrint($"Fetching member list for guild: {guild.Name}");
                            });
                            if (channelId != 0)
                                members1 = guild.GetChannelMembersAsync(channelId).GetAwaiter().GetResult().ToList();
                            else
                            {
                                IReadOnlyList<GuildChannel> channels = null;
                                if (channelId == 0)
                                {
                                    channels = clients[0].GetGuildChannels(guildId);
                                }
                                members1 = guild.GetChannelMembersAsync(channels[(int)(channels.Count / 2)].Id).GetAwaiter().GetResult().ToList();
                            }
                            break;
                        }
                        catch (Exception ex) {
                            Dispatcher.Invoke(() =>
                            {
                                App.mainView.logPrint($"{ex.Message}");
                            });
                            Thread.Sleep(500); }
                    }
                    client_s.Dispose();
                    foreach(var member in members1)
                    {
                        members.Add(member.User.Id);
                    }
                }
                else
                {
                    string path = null;
                    Dispatcher.Invoke(() =>
                    {
                        path = FileIn.Text;
                    });
                    if (!File.Exists(path))
                    {
                        Dispatcher.Invoke(() =>
                        {
                            App.mainView.logPrint($"The specified file does not exist: " + path);
                        });
                        return;
                    }
                    using(StreamReader reader = new StreamReader(path))
                    {
                        var line = reader.ReadLine();
                        while (line != null)
                        {
                            line = line.Trim(' ').Trim('\n');
                            if (line == "")
                                break;
                            if(ulong.TryParse(line, out var userId))
                            {
                                members.Add(userId);
                            }
                            line = reader.ReadLine();
                        }
                    }
                }
                setupCompleted = true;
                Dispatcher.Invoke(() =>
                {
                    App.mainView.logPrint($"Loaded {members.Count} members to message.");
                });
                int i = 0;
                if(clients.Count == 0)
                {
                    Dispatcher.Invoke(() =>
                    {
                        App.mainView.logPrint($"There was an error initializing the clients");
                    });
                }
            });
            spam.Start();

            Thread spammer = new Thread(() =>
            {
                while (!setupCompleted)
                    Thread.Sleep(100);
                Dispatcher.Invoke(() =>
                {
                    App.mainView.logPrint($"Setup completed, starting to spam...");
                });
                foreach (var client in clients)
                {
                    if (proxyOn)
                    {
                        if (Proxy.working_proxies_paid.Count > 0)
                        {
                            var proxy = Proxy.working_proxies_paid[rnd.Next(0, Proxy.working_proxies_paid.Count)];
                            if (proxy._ip != "" && proxy != null)
                            {
                                HttpProxyClient proxies = null;
                                if (proxy._username != null)
                                    proxies = new HttpProxyClient(proxy._ip, int.Parse(proxy._port), proxy._username, proxy._password);
                                else
                                    proxies = new HttpProxyClient(proxy._ip, int.Parse(proxy._port));
                                client.Proxy = proxies;
                            }
                        }
                    }
                }
                Thread spam1 = new Thread(() => {
                    if (clients.Count == 0)
                        return;
                    var usedClients = new Dictionary<DiscordClient, DateTime> { };
                    string new_msg = null;
                    if (embedOn)
                    {
                        new_msg = json;
                    }
                    while (spamming)
                    {
                        if (members.Count == 0)
                            break;
                        if(clients.Count == 0)
                        {
                            clients = usedClients.Keys.ToList();
                            usedClients = new Dictionary<DiscordClient, DateTime> { };
                        }
                        var client = clients[random.Next(0, clients.Count - 1)];
                        while (usedClients.ContainsKey(client))
                        {
                            if ((int)(DateTime.Now - usedClients[client]).TotalMilliseconds < (int)delay)
                            {
                                client = clients[random.Next(0, clients.Count - 1)];
                                Thread.Sleep(100);
                            }
                            else
                                break;
                        }
                        clients.Remove(client);
                        try
                        {
                            usedClients.Add(client, DateTime.Now);
                        }
                        catch(Exception ex) { }
                        var userId = members[random.Next(0, members.Count)];

                        bool hasSent = false;
                        int c = 0;
                        while (!hasSent && c < 3)
                        {
                            try
                            {
                                var dm = client.CreateDM(userId);
                                //Thread.Sleep(random.Next(1000, 2000));
                                dm.TriggerTyping();
                                DiscordMessage msg = null;
                                if (embedOn)
                                    msg = dm.SendMessage(new_msg);
                                else
                                    msg = dm.SendMessage(message);
                                if (msg.Content == null)
                                {
                                    Dispatcher.Invoke(() =>
                                    {
                                        App.mainView.logPrint($"Token {client.Token} has been rate limited, or embed (if using it) malformed, waiting 5 minutes.");
                                    });
                                    //Thread.Sleep(360000);
                                    usedClients[client].AddMinutes(5); 
                                }
                                else
                                {
                                    if(msg == null)
                                    {
                                        Dispatcher.Invoke(() =>
                                        {
                                            App.mainView.logPrint($"Could not send message to {userId}, probably closed DMs");
                                        });
                                    }
                                    else
                                    {
                                        members = members.Where(o => o != userId).ToList();
                                        sent++;
                                        var sent_messages = sent;
                                        Dispatcher.Invoke(() =>
                                        {
                                            App.mainView.logPrint($"Sent message to user {userId}. Sent {sent_messages} messages");
                                        });
                                    }
                                }
                                hasSent = true;
                            }
                            catch (Exception ex)
                            {
                                c++;
                            }
                        }
                        //Thread.Sleep((int)delay);
                    }
                    Dispatcher.Invoke(() => StatusLight.Fill = Brushes.Red);
                    spamming = false;
                });
                spam1.Start();
            });
            spammer.Start();
        }
        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            StatusLight.Fill = Brushes.Red;
            spamming = false;
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

        private List<string> GetMembersFromFile(string path)
        {
            var ret = new List<string>() { };
            using(StreamReader reader = new StreamReader(path))
            {
                var line = reader.ReadLine();
                while(line != null)
                {
                    line = line.Trim('\n').Trim(' ');
                    if (line == "")
                        break;
                    ret.Add(line);
                }
            }
            return ret;
        }
        private void ExploreBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.Title = "Select your file";
            dialog.AddToMostRecentlyUsedList = true;
            dialog.EnsureFileExists = true;
            dialog.EnsurePathExists = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                var path = dialog.FileName;

                FileIn.Text = path;
            }
        }

        private void FileCB_Click(object sender, RoutedEventArgs e)
        {
            if(FileGrid.Visibility == Visibility.Visible)
            {
                FileGrid.Visibility = Visibility.Collapsed;
            }
            else
            {
                FileGrid.Visibility = Visibility.Visible;
            }
        }

        private void EmbedCB_Click(object sender, RoutedEventArgs e)
        {
            if(this.EmbedCB.IsChecked == true)
            {
                this.MessageLbl.Text = "Embed Json";
                string url = "https://glitchii.github.io/embedbuilder/";
                Process.Start(url);
            }
            else
            {
                this.MessageLbl.Text = "Message";
            }
        }
    }
}
