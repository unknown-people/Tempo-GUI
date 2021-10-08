using Discord;
using Discord.Gateway;
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
            Set_Light(spamming);
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (spamming)
                return;
            StartBtn.Cursor = Cursors.AppStarting;
            var guild_id = GuildIn.Text;
            var channel_id = ChannelIn.Text;
            ulong channelId = 0;
            ulong guildId;
            if ((guild_id == null || guild_id == "") || (channel_id == null || channel_id == ""))
            {
                StartBtn.Cursor = Cursors.Arrow;
                return;
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
            if (!float.TryParse(DelayIn.Text, out var delay) && (!int.TryParse(TokensIn.Text, out var tokens)))
            {
                delay = 1000;
                tokens = 0;
            }
            bool filterOn = (bool)FilterCB.IsChecked;
            bool mentionOn = (bool)MentionCB.IsChecked;
            bool rolesOn = (bool)RolesCB.IsChecked;

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

            Task.Run(() =>
            {
                var token_list = new List<string>() { };
                using (StreamReader reader = new StreamReader(App.strWorkPath + "\\tokens\\tokens.txt"))
                {
                    var line = reader.ReadLine();
                    while(true)
                    {
                        if (line == null || line.Trim('\n') == "")
                            break;
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

                if (mentionOn)
                {
                    if(members == null || members.Count == 0)
                    {
                        var client = new DiscordSocketClient(new DiscordSocketConfig() { ApiVersion = 9, HandleIncomingMediaData = false });
                        client.Login(token_list[0]);
                        Thread.Sleep(1000);
                        while (true)
                        {
                            try
                            {
                                members = client.GetGuildChannelMembers(guildId, channelId);
                                break;
                            }
                            catch { Thread.Sleep(500); }
                        }
                        client.Logout();
                    }
                }
                int i = 0;
                Parallel.ForEach(clients, client =>
                {
                    while (spamming)
                    {
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
                            for(int t = 0; t < random.Next(3, 5); t++)
                            {
                                new_msg += "<@" + members[random.Next(0, members.Count)].User.Id.ToString() + ">\n";
                            }
                        }
                        bool hasSent = false;
                        int c = 0;
                        while (!hasSent && c < 3)
                        {
                            try
                            {
                                client.SendMessage(channelId, new_msg);
                                hasSent = true;
                            }
                            catch (Exception ex)
                            {
                                c++;
                            }
                        }
                        Thread.Sleep((int)delay);
                    }

                });
                Dispatcher.Invoke(() => StatusLight.Fill = Brushes.Red);
            });
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
