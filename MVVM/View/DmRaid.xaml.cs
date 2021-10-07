using Discord;
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
            this.Close();
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
            if (!float.TryParse(DelayIn.Text, out var delay) && (!int.TryParse(TokensIn.Text, out var tokens)))
            {
                delay = 1000;
                tokens = 0;
            }
            bool embedOn = (bool)EmbedCB.IsChecked;
            bool deleteOn = (bool)DeleteCB.IsChecked;

            delay = (int)delay;
            spamming = true;
            StatusLight.Fill = Brushes.Green;
            StartBtn.Cursor = Cursors.Arrow;

            Task.Run(() =>
            {
                var token_list = new List<string>() { };
                using (StreamReader reader = new StreamReader(App.strWorkPath + "\\tokens\\tokens.txt"))
                {
                    var line = reader.ReadLine();
                    while (line != null && line != "")
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
                }
                int i = 0;
                Parallel.ForEach(clients, client =>
                {
                    while (spamming)
                    {
                        Random random = new Random();
                        EmbedMaker new_msg = null;
                        if (embedOn)
                        {
                            new_msg = new EmbedMaker() { Title = client.User.Username, TitleUrl = "https://discord.gg/DWP2AMTWdZ", Color = System.Drawing.Color.IndianRed, Description = message };
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
