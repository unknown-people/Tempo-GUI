using Discord;
using Discord.Gateway;
using Leaf.xNet;
using Music_user_bot;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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
    public partial class MassDM : Window
    {
        public static IReadOnlyList<GuildMember> members { get; set; }
        public static bool spamming { get; set; }
        public MassDM()
        {
            InitializeComponent();
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

            delay = (int)delay;
            spamming = true;
            int sent = 0;
            StatusLight.Fill = Brushes.Green;
            StartBtn.Cursor = Cursors.Arrow;
            MainViewModel.log.Show();
            Thread spam = new Thread(() =>
            {
                var token_list = new List<string>() { };
                foreach (var tk in tokens._tokens)
                {
                    if (tk.Active)
                        token_list.Add(tk.Token);
                }
                List<DiscordClient> clients = new List<DiscordClient>();
                foreach (var token in token_list)
                {
                    try
                    {
                        clients.Add(new DiscordClient(token));
                    }
                    catch { }
                    if (max > 0)
                        if (clients.Count >= max)
                            break;
                }
                Random random = new Random();
                var rnd = new Random();
                Proxy proxy = null;
                IReadOnlyList<GuildChannel> channels = null;
                if (channelId == 0)
                {
                    channels = clients[0].GetGuildChannels(guildId);
                }
                if (members == null || members.Count == 0)
                {
                    var client_s = new DiscordSocketClient(new DiscordSocketConfig() { ApiVersion = 9, HandleIncomingMediaData = false });

                    while (spamming)
                    {
                        try
                        {
                            if (Proxy.working_proxies.Count > 0)
                            {
                                if (Proxy.working_proxies.Count == 1)
                                    proxy = Proxy.working_proxies[0];
                                else
                                    proxy = Proxy.working_proxies[rnd.Next(0, Proxy.working_proxies.Count)];
                                if (proxy._ip != "" && proxy != null)
                                {
                                    HttpProxyClient proxies = null;
                                    if (proxy._username != null)
                                        proxies = new HttpProxyClient(proxy._ip, int.Parse(proxy._port), proxy._username, proxy._password);
                                    else
                                        proxies = new HttpProxyClient(proxy._ip, int.Parse(proxy._port));
                                    client_s.Proxy = proxies;
                                }
                            }
                            client_s.Login(token_list[rnd.Next(0, token_list.Count)]);
                            break;
                        }
                        catch (Exception ex)
                        {
                            client_s = new DiscordSocketClient(new DiscordSocketConfig() { ApiVersion = 9, HandleIncomingMediaData = false });
                        }
                    }

                    while (client_s.State < GatewayConnectionState.Connected)
                        Thread.Sleep(100);
                    while (true)
                    {
                        try
                        {
                            if (channelId != 0)
                                members = client_s.GetGuildChannelMembers(guildId, channelId);
                            else
                                members = client_s.GetGuildChannelMembers(guildId, channels[(int)(channels.Count / 2)].Id);
                            break;
                        }
                        catch (Exception ex) { Thread.Sleep(500); }
                    }
                    client_s.Dispose();
                }
                int i = 0;
                var proxies_list = Proxy.working_proxies;
                if (Proxies.paidProxies)
                    proxies_list = Proxy.working_proxies_paid;
                var original_proxies = proxies_list;
                CustomMessageBox popup = null;
                foreach (var client in clients)
                {
                    Thread spam1 = new Thread(() => {
                        if (proxyOn)
                        {
                            if (Proxy.working_proxies_paid.Count > 0)
                            {
                                proxy = Proxy.working_proxies_paid[rnd.Next(0, Proxy.working_proxies_paid.Count)];
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

                        while (spamming)
                        {
                            GuildMember user = null;
                            try
                            {
                                user = members[random.Next(0, members.Count)];
                            }
                            catch { break; }
                            members = members.Where(o => o != user).ToList();
                            var userId = user.User.Id;
                            EmbedMaker new_msg = null;
                            if (embedOn)
                            {
                                new_msg = new EmbedMaker() { Title = client.User.Username, TitleUrl = "https://discord.gg/bXfjwSeBur", Color = System.Drawing.Color.IndianRed, Description = message };
                            }
                            bool hasSent = false;
                            int c = 0;
                            while (!hasSent && c < 3)
                            {
                                try
                                {
                                    var dm = client.CreateDM(userId);
                                    DiscordMessage msg = null;
                                    if (embedOn)
                                        msg = dm.SendMessage(new_msg);
                                    else
                                        msg = dm.SendMessage(message);
                                    if(msg == null)
                                    {
                                        Dispatcher.Invoke(() =>
                                        {
                                            App.mainView.logPrint($"Token {client.Token} has been rate limited, waiting 5 minutes.");
                                        });
                                        Thread.Sleep(360000);
                                    }
                                    else
                                    {
                                        sent++;
                                        Dispatcher.Invoke(() =>
                                        {
                                            App.mainView.logPrint($"Sent message to user {userId}. Sent {sent} messages");
                                        });
                                    }
                                    hasSent = true;
                                }
                                catch (Exception ex)
                                {
                                    c++;
                                }

                            }
                            Thread.Sleep((int)delay);
                        }
                        Dispatcher.Invoke(() => StatusLight.Fill = Brushes.Red);
                        spamming = false;
                    });
                    spam1.Start();
                }
            });
            spam.Start();
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
    }
}
