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

namespace TempoWithGUI.MVVM.View.RaidView
{
    /// <summary>
    /// Interaction logic for JoinGuild.xaml
    /// </summary>
    public partial class JoinGuild : UserControl
    {
        public static bool joining { get; set; } = false;
        public JoinGuild()
        {
            InitializeComponent();
            Set_Light(joining);
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (joining)
                return;
            StartBtn.Cursor = Cursors.AppStarting;
            var invite = InviteIn.Text;
            if (invite.StartsWith("https://discord.gg/"))
                invite = invite.Remove(0, ("https://discord.gg/").Length);
            if(invite == null || invite == "")
            {
                StartBtn.Cursor = Cursors.Arrow;
                return;
            }
            if(!float.TryParse(DelayIn.Text, out var delay) || (!int.TryParse(TokensIn.Text, out var tokens))){
                delay = 250;
                tokens = 0;
            }
            joining = true;
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
            Task.Run(() =>
            {
                var token_list = new List<string>() { };
                using(StreamReader reader = new StreamReader(App.strWorkPath + "\\tokens\\tokens.txt"))
                {
                    var line = reader.ReadLine();
                    while(line != null)
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
                List<DiscordClient> clients = new List<DiscordClient>();
                foreach (var token in token_list)
                {
                    clients.Add(new DiscordClient(token));
                    if (max > 0)
                        if (clients.Count >= max)
                            break;
                }
                int i = 0;
                foreach (var client in clients)
                {
                    if (!joining)
                        break;
                    Random rnd = new Random();
                    Proxy proxy = null;
                    if(Proxy.working_proxies.Count > 0)
                    {
                        proxy = Proxy.working_proxies[rnd.Next(0, Proxy.working_proxies.Count)];
                        if (proxy._ip != "" && proxy != null)
                        {
                            HttpProxyClient proxies = new HttpProxyClient(proxy._ip, int.Parse(proxy._port));
                            client.Proxy = proxies;
                        }
                    }

                    bool hasJoined = false;
                    int c = 0;
                    while (!hasJoined && c < 10)
                    {
                        try
                        {
                            client.JoinGuild(invite);
                            hasJoined = true;
                        }
                        catch (Exception ex)
                        {
                            if (Proxy.working_proxies.Count > 0)
                            {
                                proxy = Proxy.working_proxies[rnd.Next(0, Proxy.working_proxies.Count - 1)];
                                if (proxy._ip != "" && proxy != null)
                                {
                                    HttpProxyClient proxies = new HttpProxyClient(proxy._ip, int.Parse(proxy._port));
                                    client.Proxy = proxies;
                                }
                            }
                            c++;
                            continue;
                        }
                    }
                    Thread.Sleep((int)delay);
                }
                Dispatcher.Invoke(() => Set_Light(false));
            });
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
    }
}
