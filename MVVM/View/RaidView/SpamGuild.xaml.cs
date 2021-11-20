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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TempoWithGUI.MVVM.View.RaidView
{
    /// <summary>
    /// Interaction logic for SpamGuild.xaml
    /// </summary>
    public partial class SpamGuild : UserControl
    {
        public static List<string> emojis = new List<string>() { ":laughing:", ":eyes:", ":weary:", ":sob:", ":hot_face:", ":cold_face:", ":man_detective:",
        ":recycle:", ":transgender_flag:"};
        public static bool spamming { get; set; } = false;
        public static IReadOnlyList<GuildMember> members { get; set; }
        public SpamGuild()
        {
            InitializeComponent();
            Debug.Log("Spam guild interface initialized");
            Set_Light(spamming);
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (spamming)
                return;
            members = null;
            StartBtn.Cursor = Cursors.AppStarting;
            var guild_id = GuildIn.Text;
            var channel_id = ChannelIn.Text;
            ulong channelId = 0;
            ulong guildId;
            if ((guild_id == null || guild_id == ""))
            {
                StartBtn.Cursor = Cursors.Arrow;
                return;
            }
            if (channel_id == null || channel_id == "")
            {
                channel_id = "0";
            }
            var message = MessageIn.Text;
            if(message == null || message == "")
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
                delay = 1000;
            }
            bool filterOn = (bool)FilterCB.IsChecked;
            bool mentionOn = (bool)MentionCB.IsChecked;
            bool rolesOn = (bool)RolesCB.IsChecked;
            bool proxyOn = (bool)ProxyCB.IsChecked;

            delay = (int)delay;
            spamming = true;
            var max_tokens = TokensIn.Text;
            int max = 0;
            if (!(max_tokens == null || max_tokens.ToString().Trim('\n') == ""))
                if (!int.TryParse(max_tokens, out max))
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
                IReadOnlyList<GuildChannel> channels = null;
                if (channelId == 0)
                {
                    channels = clients[0].GetGuildChannels(guildId);
                }
                if (mentionOn)
                {
                    if(members == null || members.Count == 0)
                    {
                        var client_s = new DiscordSocketClient(new DiscordSocketConfig() { ApiVersion = 9, HandleIncomingMediaData = false });
                        client_s.Login(token_list[0]);
                        while(client_s.State < GatewayConnectionState.Connected)
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
                            catch { Thread.Sleep(500); }
                        }
                        client_s.Dispose();
                    }
                }

                int i = 0;
                Parallel.ForEach(clients, client =>
                {
                    if (proxyOn)
                    {
                        Random rnd = new Random();
                        Proxy proxy = null;
                        var proxies_list = Proxy.working_proxies;
                        if (Proxies.paidProxies)
                            proxies_list = Proxy.working_proxies_paid;
                        var original_proxies = proxies_list;
                        CustomMessageBox popup = null;

                        var tries = 0;
                        while (tries <= 5)
                        {
                            if (proxies_list.Count > 0)
                            {
                                proxy = proxies_list[rnd.Next(0, proxies_list.Count)];
                                if (proxy._ip != "" && proxy != null)
                                {
                                    HttpProxyClient proxies = new HttpProxyClient(proxy._ip, int.Parse(proxy._port));
                                    if (Proxies.paidProxies)
                                        proxies = new HttpProxyClient(proxy._ip, int.Parse(proxy._port), proxy._username, proxy._password);
                                    client.Proxy = proxies;
                                    while (true)
                                    {
                                        try
                                        {
                                            if (Proxies.paidProxies)
                                                break;
                                            var buff = client.GetVoiceRegions();
                                            break;
                                        }
                                        catch (Exception ex)
                                        {
                                            try
                                            {
                                                proxies_list.Remove(proxy);
                                                if (proxies_list.Count > 0)
                                                {
                                                    proxy = proxies_list[rnd.Next(0, proxies_list.Count - 1)];
                                                    if (proxy._ip != "" && proxy != null)
                                                    {
                                                        proxies = new HttpProxyClient(proxy._ip, int.Parse(proxy._port));
                                                        client.Proxy = proxies;
                                                    }
                                                }
                                                else
                                                {
                                                    //Proxy.GetProxies("https://discord.com");
                                                    client.Proxy = null;
                                                }
                                                break;
                                            }
                                            catch { }
                                        }
                                    }
                                }
                                break;
                            }
                            else
                            {
                                if (Proxy.gettingProxies)
                                {
                                    proxies_list = Proxy.working_proxies;
                                    if (Proxies.paidProxies)
                                        proxies_list = Proxy.working_proxies_paid;
                                    proxies_list = proxies_list.Except(original_proxies).ToList();
                                    original_proxies = proxies_list;

                                    tries++;
                                    Thread.Sleep(2000);
                                }
                                else
                                {
                                    Dispatcher.Invoke(() =>
                                    {
                                        if (popup != null)
                                            return;
                                        popup = new CustomMessageBox("There are no more free proxies available. Try again in a few minutes");
                                        popup.ShowDialog();
                                        Set_Light(false);
                                    });
                                    spamming = false;
                                    return;
                                }
                            }
                        }
                    }
                    while (spamming)
                    {
                        if (!IsInGuild(client, guildId))
                            break;
                        ulong nc = 0;
                        if(channelId == 0)
                        {
                            var channel = channels[random.Next(0, channels.Count)];
                            while(!channel.IsText)
                                channel = channels[random.Next(0, channels.Count)];
                            nc = channel.Id;
                        }
                        var new_msg = message;
                        if (filterOn)
                        {
                            new_msg += "\n" + emojis[random.Next(0, emojis.Count)];
                            for(int t = 0; t < random.Next(5,10); t++)
                            {
                                new_msg += emojis[random.Next(0, emojis.Count)];
                            }
                        }
                        if (mentionOn)
                        {
                            new_msg += "\n";
                            for(int t = 0; t < random.Next(3, 5); t++)
                            {
                                while (true)
                                {
                                    try
                                    {
                                        new_msg += "<@" + members[random.Next(0, members.Count)].User.Id.ToString() + ">\n";
                                        break;
                                    }
                                    catch { }
                                }
                            }
                        }
                        bool hasSent = false;
                        int c = 0;
                        while (!hasSent && c < 1)
                        {
                            DiscordMessage mes;
                            try
                            {
                                if (channelId == 0)
                                    mes = client.SendMessage(nc, new_msg);
                                else
                                    mes = client.SendMessage(channelId, new_msg);
                                hasSent = true;
                            }
                            catch (Exception ex)
                            {
                                c++;
                                Thread.Sleep(5000);
                            }
                        }
                        if(c >= 1)
                        {
                            break;
                        }
                        Thread.Sleep((int)delay);
                    }
                });
                
                Dispatcher.Invoke(() =>
                {
                    if (StatusLight.Fill == Brushes.Green)
                        StatusLight.Fill = Brushes.Red;
                });
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
        public bool IsInGuild(DiscordClient Client, ulong guildId)
        {
            try
            {
                foreach (var guild in Client.GetGuilds())
                {
                    if (guildId == guild.Id)
                        return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                return true;
            }
        }
    }
}
