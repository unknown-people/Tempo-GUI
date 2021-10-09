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
            var guildId = GuildIn.Text;
            if (invite.StartsWith("https://discord.gg/"))
                invite = invite.Remove(0, ("https://discord.gg/").Length);
            if((invite == null || invite == "") || (guildId == null || guildId == ""))
            {
                StartBtn.Cursor = Cursors.Arrow;
                return;
            }
            if(!float.TryParse(DelayIn.Text, out var delay) || (!int.TryParse(TokensIn.Text, out var tokens_n))){
                delay = 250;
                tokens_n = 0;
            }
            ulong guild_id = 0;
            if(!ulong.TryParse(guildId, out guild_id))
            {
                StartBtn.Cursor = Cursors.Arrow;
                return;
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
            bool acceptRules = (bool)RulesCB.IsChecked;

            StatusLight.Fill = Brushes.Green;
            StartBtn.Cursor = Cursors.Arrow;
            Thread join = new Thread(() =>
            {
                var token_list = new List<string>() { };
                foreach(var tk in tokens._tokens)
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
                            if (acceptRules)
                            {
                                try
                                {
                                    client.AcceptRulesAsync(guildId);
                                }
                                catch (Exception ex) { Thread.Sleep((int)delay); }
                            }
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
            join.Start();
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
