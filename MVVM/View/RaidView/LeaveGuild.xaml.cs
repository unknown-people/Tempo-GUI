using Discord;
using Leaf.xNet;
using Music_user_bot;
using System;
using System.Collections.Generic;
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
using TempoWithGUI.MVVM.ViewModel;

namespace TempoWithGUI.MVVM.View.RaidView
{
    /// <summary>
    /// Interaction logic for LeaveGuild.xaml
    /// </summary>
    public partial class LeaveGuild : UserControl
    {
        public static bool leaving { get; set; } = false;
        public LeaveGuild()
        {
            InitializeComponent();
            Debug.Log("Leave guild interface initialized");
        }
        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (leaving)
                return;
            StartBtn.Cursor = Cursors.AppStarting;
            var guildId = GuildIn.Text;
            if (!float.TryParse(DelayIn.Text, out var delay) || (!int.TryParse(TokensIn.Text, out var tokens_n)))
            {
                delay = 250;
                tokens_n = 0;
            }
            ulong guild_id = 0;
            ulong channel_id = 0;
            if (!ulong.TryParse(guildId, out guild_id))
            {
                StartBtn.Cursor = Cursors.Arrow;
                return;
            }
            leaving = true;
            bool useProxies = (bool)ProxiesCB.IsChecked;
            var max_tokens = TokensIn.Text;
            int max = 0;
            if (!(max_tokens == null || max_tokens.ToString().Trim('\n') == ""))
            {
                if (!int.TryParse(max_tokens, out max))
                {
                    MessageBox.Show("Please insert a valid value for max tokens");
                    StartBtn.Cursor = Cursors.Arrow;
                    return;
                }
            }
            else
            {
                max = tokens._tokens.Count;
            }
            StatusLight.Fill = Brushes.Green;
            StartBtn.Cursor = Cursors.Arrow;
            var token_list = new List<string>() { };
            foreach (var tk in tokens._tokens)
            {
                if (tk.Active)
                    token_list.Add(tk.Token);
            }
            List<DiscordClient> clients = new List<DiscordClient>();
            int left = 0;
            MainViewModel.log.Show();
            var leaver = new Thread(() =>
            {
                while (leaving)
                {
                    if (left == token_list.Count)
                        leaving = false;
                    int i = 0;
                    try
                    {
                        var clients1 = new DiscordClient[clients.Count];
                        clients.CopyTo(clients1);
                        foreach (var client in clients1)
                        {
                            if (!leaving)
                                return;
                            if (client == null)
                                continue;
                            try
                            {
                                bool hasJoined = false;
                                int c = 0;
                                while (!hasJoined && c < 3)
                                {
                                    client.LeaveGuild(guild_id);
                                    left++;
                                    hasJoined = true;
                                    clients1[i] = null;
                                    clients[i] = null;
                                    var username = client.User.Username;
                                    var discr = client.User.Discriminator.ToString();
                                    for (int d = 0; d < 4 - discr.Length; d++)
                                    {
                                        discr = "0" + discr;
                                    }
                                    username += "#" + discr;
                                    Dispatcher.Invoke(() =>
                                    {
                                        App.mainView.logPrint($"{username} has left server {guild_id}");
                                    });
                                }
                                if (c >= 3)
                                {
                                    clients1[i] = null;
                                    clients[clients.IndexOf(client)] = null;
                                    token_list.Remove(client.Token);
                                    Dispatcher.Invoke(() =>
                                    {
                                        App.mainView.logPrint($"Token {client.Token} is probably phone locked, descarding it.");
                                    });
                                }
                            }
                            catch (Exception ex) { }
                            i++;
                            Thread.Sleep((int)delay);
                        }
                    }
                    catch (InvalidOperationException ex) { Thread.Sleep(500); }
                    Thread.Sleep(1000);
                }
                Dispatcher.Invoke(() => Set_Light(leaving));
            });
            leaver.Priority = ThreadPriority.BelowNormal;
            leaver.Start();
            var thread_pool = new List<Thread>() { };
            Thread leave = new Thread(() =>
            {
                Random rnd = new Random();
                Proxy proxy = null;
                var proxies_list = Proxy.working_proxies;
                if (Proxies.paidProxies)
                    proxies_list = Proxy.working_proxies_paid;
                var original_proxies = proxies_list;
                CustomMessageBox popup = null;
                foreach (var token in token_list)
                {
                    Thread join1 = new Thread(() =>
                    {
                        try
                        {
                            if (!leaving)
                                return;
                            var client = new DiscordClient(token);
                            var tries = 0;
                            if(useProxies)
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
                                            leaving = false;
                                            return;
                                        }
                                    }
                                }
                            if(tries >= 5)
                            {
                                Dispatcher.Invoke(() =>
                                {
                                    if (popup != null)
                                        return;
                                    popup = new CustomMessageBox("There are no more free proxies available. Try again in a few minutes");
                                    popup.ShowDialog();
                                    Set_Light(false);
                                });
                                leaving = false;
                                return;
                            }
                            if (IsInGuild(client, guild_id) == false)
                            {
                                left++;
                                return;
                            }
                            clients.Add(client);
                        }
                        catch (Exception ex) { }
                    });
                    thread_pool.Add(join1);
                    join1.Start();
                    while (thread_pool.Count >= 5)
                    {
                        foreach(var thread in thread_pool)
                        {
                            if (!thread.IsAlive)
                            {
                                thread_pool.Remove(thread);
                                break;
                            }
                        }
                        Thread.Sleep(500);
                    }
                    Thread.Sleep((int)delay);
                }
            });
            leave.Start();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            StatusLight.Fill = Brushes.Red;
            leaving = false;
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
            foreach (var guild in Client.GetGuilds())
            {
                if (guildId == guild.Id)
                    return true;
            }
            return false;
        }
    }
}
