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
using System.Windows.Shapes;
using TempoWithGUI.MVVM.ViewModel;

namespace TempoWithGUI.MVVM.View
{
    /// <summary>
    /// Interaction logic for DmRaid.xaml
    /// </summary>
    public partial class DmRaid : Window
    {
        public static List<string> emojis = new List<string>() { ":laughing:", ":eyes:", ":weary:", ":sob:", ":hot_face:", ":cold_face:", ":man_detective:",
        ":recycle:", ":transgender_flag:"};
        public static bool spamming { get; set; } = false;
        public DmRaid()
        {
            InitializeComponent();
            Debug.Log("DM spam interface initialized");
            Set_Light(spamming);
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            RaidModel.dmOn = false;
            this.Hide();;
        }
        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (spamming)
                return;
            StartBtn.Cursor = Cursors.AppStarting;
            var user_id = UserIn.Text;
            ulong userId;
            if (user_id == null || user_id == "")
            {
                StartBtn.Cursor = Cursors.Arrow;
                return;
            }
            var message = MessageIn.Text;
            if (message == null || message == "")
            {
                StartBtn.Cursor = Cursors.Arrow;
                return;
            }
            if (!ulong.TryParse(user_id, out userId))
            {
                MessageBox.Show("Please use a valid user ID");
                return;
            }
            if (!float.TryParse(DelayIn.Text, out var delay))
            {
                delay = 1000;
            }
            bool embedOn = (bool)EmbedCB.IsChecked;
            bool deleteOn = (bool)DeleteCB.IsChecked;

            var max_tokens = TokensIn.Text;

            int max = 0;
            if (!(max_tokens == null || max_tokens.ToString().Trim('\n') == ""))
                if (!int.TryParse(max_tokens, out max))
                {
                    MessageBox.Show("Please insert a valid value for max tokens");
                    StartBtn.Cursor = Cursors.Arrow;
                    return;
                }
            delay = (int)delay;
            spamming = true;
            bool proxyOn = (bool)ProxyCB.IsChecked;

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
                    Random rnd = new Random();
                    Proxy proxy = null;
                    DiscordClient client;
                    try
                    {
                        client = new DiscordClient(token);
                    }
                    catch { continue; }
                    var c = 0;
                    while(c < 10)
                    {
                        try
                        {
                            if (Proxy.working_proxies.Count > 0)
                            {
                                proxy = Proxy.working_proxies[rnd.Next(0, Proxy.working_proxies.Count)];
                                if (proxy._ip != "" && proxy != null)
                                {
                                    HttpProxyClient proxies = new HttpProxyClient(proxy._ip, int.Parse(proxy._port));
                                    client.Proxy = proxies;
                                }
                            }
                            clients.Add(client);
                            break;
                        }
                        catch
                        {
                            Proxy.working_proxies.Remove(proxy);
                            if (Proxy.working_proxies.Count > 0)
                            {
                                proxy = Proxy.working_proxies[rnd.Next(0, Proxy.working_proxies.Count - 1)];
                                if (proxy._ip != "" && proxy != null)
                                {
                                    HttpProxyClient proxies = new HttpProxyClient(proxy._ip, int.Parse(proxy._port));
                                    client.Proxy = proxies;
                                }
                            }
                            else
                            {
                                Proxy.GetProxies("https://discord.com");
                                client.Proxy = null;
                            }
                            c++;
                        }
                    }

                    if (max > 0)
                        if (clients.Count >= max)
                            break;
                }
                int i = 0;
                Parallel.ForEach(clients, client =>
                {
                    if (proxyOn)
                    {
                        Random rnd = new Random();
                        Proxy proxy = null;
                        var proxies_list = new List<Proxy>() { };
                        if (Proxies.freeProxies)
                        {
                            proxies_list = Proxy.working_proxies;
                        }
                        else
                        {
                            proxies_list = Proxy.working_proxies_paid;
                        }

                        if (proxies_list.Count > 0)
                        {
                            proxy = proxies_list[rnd.Next(0, proxies_list.Count)];
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
                        Dispatcher.Invoke(() =>
                        {
                            App.mainView.logPrint($"{client.User.Username + "#" + client.User.Discriminator} started spamming to {userId}");
                        });
                        Random random = new Random();
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
                                if(embedOn)
                                    msg = dm.SendMessage(new_msg);
                                else
                                    msg = dm.SendMessage(message);
                                if (deleteOn)
                                {
                                    try
                                    {
                                        msg.Delete();
                                    }
                                    catch { }
                                }
                                hasSent = true;
                            }
                            catch (Exception ex)
                            {
                                c++;
                            }
                        }
                        if (c >= 3)
                            break;
                        Thread.Sleep((int)delay);
                    }

                });
                Dispatcher.Invoke(() => StatusLight.Fill = Brushes.Red);
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
