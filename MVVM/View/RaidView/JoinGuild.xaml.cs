using Discord;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using TempoWithGUI.MVVM.ViewModel;

namespace TempoWithGUI.MVVM.View.RaidView
{
    /// <summary>
    /// Interaction logic for JoinGuild.xaml
    /// </summary>
    public partial class JoinGuild : UserControl
    {
        public static uint verifyingCount { get; set; } = 0;
        public static bool joining { get; set; } = false;
        public JoinGuild()
        {
            InitializeComponent();
            Debug.Log("Join guild interface initialized");
            var react = ReactionCB.IsChecked;
            if (react != false)
            {
                ChannelLabel.Visibility = Visibility.Visible;
                ChannelIn.Visibility = Visibility.Visible;
            }
            else
            {
                ChannelLabel.Visibility = Visibility.Collapsed;
                ChannelIn.Visibility = Visibility.Collapsed;
            }
            Set_Light(joining);
        }
        private void Start_Click(object sender, RoutedEventArgs e)
        {
            bool acceptRules = (bool)RulesCB.IsChecked;
            bool reaction_accept = (bool)ReactionCB.IsChecked;
            CustomMessageBox popup = null;
            if (joining)
                return;
            StartBtn.Cursor = Cursors.AppStarting;
            var invite = InviteIn.Text;
            if (invite.StartsWith("https://discord.gg/"))
                invite = invite.Remove(0, "https://discord.gg/".Length);
            if(invite.StartsWith("https://discord.com/invite/"))
                invite = invite.Remove(0, "https://discord.com/invite/".Length);
            Dispatcher.Invoke(() =>
            {
                App.mainView.logPrint($"Getting guild ID");
            });
            var (guildId, channelWelcomeId) = Get_GuildID(invite);
            if (guildId == "1")
            {
                if(popup == null)
                {
                    popup = new CustomMessageBox("Could not get invite informations.\nYou are probably IP banned or rate-limited from Discord. Please activate a vpn and try again");
                    popup.ShowDialog();
                    return;
                }
            }
            Dispatcher.Invoke(() =>
            {
                App.mainView.logPrint($"Guild ID is: {guildId}");
            });
            var channelId = ChannelIn.Text;
            if (invite == null || invite == "")
            {
                StartBtn.Cursor = Cursors.Arrow;
                return;
            }
            if ((channelId == null || channelId == "") && reaction_accept)
            {
                StartBtn.Cursor = Cursors.Arrow;
                return;
            }
            float delay = 0;
            int tokens_n = 0;
            if (!float.TryParse(DelayIn.Text, out delay) || (!int.TryParse(TokensIn.Text, out tokens_n)))
            {
                if (delay == 0)
                    delay = 250;
            }
            ulong guild_id = 0;
            ulong channel_id = 0;
            if (!ulong.TryParse(guildId, out guild_id))
            {
                StartBtn.Cursor = Cursors.Arrow;
                return;
            }
            if (!ulong.TryParse(channelId, out channel_id) && reaction_accept)
            {
                StartBtn.Cursor = Cursors.Arrow;
                return;
            }
            joining = true;
            var max_tokens = TokensIn.Text;
            int max = tokens._tokens.Count;
            if (!(max_tokens == null || max_tokens.ToString().Trim('\n') == "" || int.TryParse(max_tokens, out max)))
            {
                if (!int.TryParse(max_tokens, out max))
                {
                    if (popup == null)
                    {
                        popup = new CustomMessageBox("Please insert a valid value for max tokens. Leave empty to use all tokens");
                        popup.ShowDialog();
                        return;
                    }
                    StartBtn.Cursor = Cursors.Arrow;
                    return;
                }
            }

            StatusLight.Fill = Brushes.Green;
            StartBtn.Cursor = Cursors.Arrow;
            bool useProxies = (bool)ProxiesCB.IsChecked;
            Random rnd = new Random();
            Proxy proxy = null;
            var proxies_list = Proxy.working_proxies;
            if (Proxies.paidProxies)
                proxies_list = Proxy.working_proxies_paid;
            var original_proxies = proxies_list;
            var clients = new List<DiscordClient>() { };
            var joined = 0;
            var token_list = new List<string>() { };
            foreach (var tk in tokens._tokens)
            {
                if (token_list.Count >= max)
                    break;
                if (tk.Active)
                    token_list.Add(tk.Token);
            }
            MainViewModel.log.Show();
            Thread joiner = new Thread(() =>
            {
                var joined_time = DateTime.Now;
                while (joining)
                {
                    while (tokens.loadingTokens)
                        Thread.Sleep(500);
                    try
                    {
                        if (joined == token_list.Count)
                            joining = false;
                    }
                    catch (Exception ex) { }
                    int i = 0;
                    try
                    {
                        var clients1 = new DiscordClient[clients.Count + 10];
                        clients.CopyTo(clients1);
                        foreach (var client in clients1)
                        {
                            if (!joining)
                                break;
                            if (client == null)
                                continue;
                            try
                            {
                                bool hasJoined = false;
                                int c = 0;
                                while (!hasJoined && c < 3)
                                {
                                    try
                                    {
                                        joined_time = DateTime.Now;

                                        client.JoinGuild(invite, guild_id, ulong.Parse(channelWelcomeId));
                                    }
                                    catch (Exception ex)
                                    {
                                        if (ex.Message == "Failed to connect to Discord" && c >= 3)
                                        {
                                            Dispatcher.Invoke(() =>
                                            {
                                                App.mainView.logPrint($"{client.User.Username + "#" + client.User.Discriminator} could not join, may be a proxy problem or the token being banned.");
                                            });
                                        }
                                        c++;
                                        continue;
                                    }
                                    var username = client.User.Username;
                                    var discr = client.User.Discriminator.ToString();
                                    for (int d = 0; d < 4 - discr.Length; d++)
                                    {
                                        discr = "0" + discr;
                                    }
                                    username += "#" + discr;
                                    Task.Run(() =>
                                    {
                                        verifyingCount++;
                                        if (acceptRules)
                                        {
                                            var z = 0;
                                            while (z < 3)
                                            {
                                                try
                                                {
                                                    var accepted = client.GetGuildVerificationForm(guild_id, invite);
                                                }
                                                catch (Exception ex) { Thread.Sleep((int)delay); z++; }
                                            }
                                            if(z >= 3)
                                            {
                                                Dispatcher.Invoke(() =>
                                                {
                                                    App.mainView.logPrint($"{username} could not accepted rules on server {guild_id}.");
                                                });
                                            }
                                            else
                                            {
                                                Dispatcher.Invoke(() =>
                                                {
                                                    App.mainView.logPrint($"{username} has accepted rules on server {guild_id}.");
                                                });
                                            }
                                        }
                                        if (reaction_accept)
                                        {
                                            try
                                            {
                                                var messages = client.GetChannelMessages(channel_id, new MessageFilters()
                                                {
                                                    Limit = 20
                                                });
                                                foreach (var message in messages)
                                                {
                                                    var reactions = message.Reactions;
                                                    if(reactions.Count < 5)
                                                    {
                                                        foreach (var reaction in reactions)
                                                        {
                                                            message.AddReaction(reaction.Emoji.Name, reaction.Emoji.Id);
                                                            Thread.Sleep(500);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        MessageReaction to_react = reactions.First();
                                                        foreach (var reaction in reactions)
                                                        {
                                                            if (reaction.Count > to_react.Count)
                                                                to_react = reaction;
                                                        }
                                                        message.AddReaction(to_react.Emoji.Name, to_react.Emoji.Id);
                                                    }
                                                }
                                                Dispatcher.Invoke(() =>
                                                {
                                                    App.mainView.logPrint($"{username} has verified reaction on server {guild_id}.");
                                                });
                                            }
                                            catch (Exception ex) { }
                                        }
                                        verifyingCount--;
                                    });
                                    joined++;
                                    hasJoined = true;
                                    clients1[i] = null;
                                    clients[clients.IndexOf(client)] = null;
                                    Dispatcher.Invoke(() =>
                                    {
                                        App.mainView.logPrint($"{username} has joined server {guild_id}. {joined} accounts joined this server");
                                    });
                                }
                                if(c >= 4)
                                {
                                    clients1[i] = null;
                                    clients[clients.IndexOf(client)] = null;
                                    token_list.Remove(client.Token);
                                    Dispatcher.Invoke(() =>
                                    {
                                        App.mainView.logPrint($"Token {client.Token} is probably phone locked or banned from the guild, descarding it.");
                                    });
                                    continue;
                                }
                            }
                            catch (Exception ex) {
                            }
                            i++;
                            var to_sleep = delay - (DateTime.Now - joined_time).TotalMilliseconds;
                            if (to_sleep < 0)
                                to_sleep = 0;
                            Thread.Sleep((int)to_sleep);
                        }
                    }
                    catch (InvalidOperationException ex) { Thread.Sleep(500); }
                }
                while (verifyingCount != 0)
                    Thread.Sleep(500);
                Dispatcher.Invoke(() => Set_Light(joining));
            });
            joiner.Priority = ThreadPriority.AboveNormal;
            joiner.Start();

            var thread_pool = new List<Thread>() { };
            Thread join = new Thread(() =>
            {
                int i = 0;
                var threads = 0;
                while (tokens.loadingTokens)
                {
                    Dispatcher.Invoke(() =>
                    {
                        App.mainView.logPrint($"Token list loading, please wait a minute");
                    });
                    Thread.Sleep(1000);
                }
                var tk_list = token_list.ToArray();
                foreach (var token in tk_list)
                {
                    if (!joining || thread_pool.Count >= max)
                        return;
                    Thread join1 = new Thread(() =>
                    {
                        if (!joining)
                            return;
                        DiscordClient client;
                        try
                        {
                            client = new DiscordClient(token);
                            threads++;
                        }
                        catch (Exception ex) { return; }
                        int tries = 0;
                        proxies_list = Proxy.working_proxies;
                        if (Proxies.paidProxies)
                            proxies_list = Proxy.working_proxies_paid;
                        if (useProxies)
                        {
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
                                                var buff = client.GetActiveSubscription();
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
                                                        Dispatcher.Invoke(() =>
                                                        {
                                                            if (popup != null)
                                                                return;
                                                            popup = new CustomMessageBox("There are no more free proxies available. Try again in a few minutes");
                                                            popup.ShowDialog();
                                                        });
                                                        return;
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
                                        Thread.Sleep(1000);
                                    }
                                    else
                                    {
                                        Dispatcher.Invoke(() =>
                                        {
                                            if (popup != null)
                                                return;
                                            popup = new CustomMessageBox("There are no more free proxies available. Try again in a few minutes");
                                            popup.ShowDialog();
                                        });
                                        return;
                                    }
                                }
                            }
                        }
                        if (IsInGuild(client, guild_id) == true && !reaction_accept && !acceptRules)
                        {
                            joined++;
                            return;
                        }
                        else
                        {
                            clients.Add(client);
                        }
                    });
                    thread_pool.Add(join1);
                    join1.Priority = ThreadPriority.AboveNormal;
                    join1.Start();
                    while (thread_pool.Count >= 5)
                    {
                        foreach (var thread in thread_pool)
                        {
                            if (!thread.IsAlive)
                            {
                                thread_pool.Remove(thread);
                                break;
                            }
                        }
                        Thread.Sleep(500);
                    }
                }
            });
            join.Start();
        }
            
        public static (string, string) Get_GuildID(string invite)
        {
            string request_url = $"https://discord.com/api/v9/invites/{invite}";
            HttpClient client = new HttpClient();
            var response = client.SendAsync(new HttpRequestMessage()
            {
                Method = new System.Net.Http.HttpMethod("GET"),
                RequestUri = new Uri(request_url)
            }).GetAwaiter().GetResult();
            if(((int)response.StatusCode) == 429)
            {
                return ("1", null);
            }
            var jtoken = JToken.Parse(response.Content.ReadAsStringAsync().Result);
            var json = JObject.Parse(jtoken.ToString());
            try
            {
                string guild_id = json["guild"].Value<string>("id");
                string channel_id = json["channel"].Value<string>("id");
                return (guild_id, channel_id);
            }
            catch (Exception ex)
            {
                return (null, null);
            }
        }
        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            StatusLight.Fill = Brushes.Red;
            joining = false;
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

        private void ReactionCB_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)ReactionCB.IsChecked)
            {
                ChannelIn.Visibility = Visibility.Visible;
                ChannelLabel.Visibility = Visibility.Visible;
            }
            else
            {
                ChannelIn.Visibility = Visibility.Collapsed;
                ChannelLabel.Visibility = Visibility.Collapsed;
            }
        }
        public bool? IsInGuild(DiscordClient Client, ulong guildId)
        {
            var tries = 0;
            if (Client.User.PhoneNumber == null)
                return false;
            while(tries < 3)
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
                    if (ex.Message.Contains("You need to verify"))
                        return true;
                    continue;
                }
            }
            return null;
        }
    }
}
