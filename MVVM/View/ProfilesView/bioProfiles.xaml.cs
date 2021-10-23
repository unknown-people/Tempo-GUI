using Discord;
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
using TempoWithGUI.MVVM.ViewModel;

namespace TempoWithGUI.MVVM.View.ProfilesView
{
    /// <summary>
    /// Interaction logic for bioProfiles.xaml
    /// </summary>
    public partial class bioProfiles : UserControl
    {
        public bool isChanging { get; set; }
        public bioProfiles()
        {
            InitializeComponent();
        }
        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (isChanging)
                return;
            var bio = BioIn.Text;
            bool useProxies = (bool)ProxiesCB.IsChecked;
            CustomMessageBox popup = null;
            if (bio == "" || bio == null)
                return;
            this.Cursor = Cursors.AppStarting;

            float delay = 0;
            int tokens_n = 0;
            if (!float.TryParse(DelayIn.Text, out delay) || (!int.TryParse(TokensIn.Text, out tokens_n)))
            {
                if (delay == 0)
                    delay = 250;
            }
            var max_tokens = TokensIn.Text;
            int max = tokens._tokens.Count;
            if (!(max_tokens == null || max_tokens.ToString().Trim('\n') == ""))
            {
                if (!int.TryParse(max_tokens, out max))
                {
                    MessageBox.Show("Please insert a valid value for max tokens");
                    StartBtn.Cursor = Cursors.Arrow;
                    return;
                }
            }
            isChanging = true;
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
                if (tk.Active)
                    token_list.Add(tk.Token);
            }
            MainViewModel.log.Show();
            this.Cursor = Cursors.Arrow;
            Set_Light(isChanging);
            var thread_pool = new List<Thread>() { };
            Thread addClients = new Thread(() =>
            {
                var tries = 0;
                int i = 0;
                var threads = 0;
                foreach (var token in token_list)
                {
                    if (!isChanging)
                        return;
                    Thread join1 = new Thread(() =>
                    {
                        if (!isChanging)
                            return;
                        DiscordClient client;
                        try
                        {
                            client = new DiscordClient(token);
                            threads++;
                        }
                        catch (Exception ex) { return; }
                        proxies_list = Proxy.working_proxies;
                        if (Proxies.paidProxies)
                            proxies_list = Proxy.working_proxies_paid;
                        if (useProxies)
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
                                                var buff = client.GetBoostSlots();
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
                                            Set_Light(false);
                                        });
                                        isChanging = false;
                                        return;
                                    }
                                }
                            }
                        clients.Add(client);
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
            addClients.Start();
            var changed = 0;
            Thread changer = new Thread(() =>
            {
                while (isChanging)
                {
                    if (changed == token_list.Count)
                        isChanging = false;
                    int i = 0;
                    try
                    {
                        var clients1 = new DiscordClient[clients.Count];
                        clients.CopyTo(clients1);
                        foreach (var client in clients1)
                        {
                            if (!isChanging)
                                break;
                            if (client == null)
                                continue;
                            try
                            {
                                bool hasJoined = false;
                                int c = 0;
                                while (!hasJoined && c < 3)
                                {
                                    client.User.ChangeProfile(new UserProfileUpdate()
                                    {
                                        Biography = bio
                                    });
                                    changed++;
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
                                        App.mainView.logPrint($"{username} has changed bio: (\"{bio}\")");
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
                Dispatcher.Invoke(() => Set_Light(isChanging));
            });
            changer.Priority = ThreadPriority.BelowNormal;
            changer.Start();
        }
        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            StatusLight.Fill = System.Windows.Media.Brushes.Red;
            isChanging = false;
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
