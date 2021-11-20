using Discord;
using Discord.Gateway;
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
using System.Windows.Shapes;
using TempoWithGUI.MVVM.ViewModel;

namespace TempoWithGUI.MVVM.View
{
    /// <summary>
    /// Interaction logic for FriendRaid.xaml
    /// </summary>
    public partial class FriendRaid : Window
    {
        public static bool spamming { get; set; }
        public FriendRaid()
        {
            InitializeComponent();
            Debug.Log("Friend request spam interface initialized");
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            RaidModel.friendOn = false;
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
            if (!ulong.TryParse(user_id, out userId))
            {
                MessageBox.Show("Please use a valid user ID");
                return;
            }
            if (!float.TryParse(DelayIn.Text, out var delay))
            {
                delay = 1000;
            }

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

            bool single_spam = (bool)SingleCB.IsChecked;
            bool delete_spam = (bool)DeleteCB.IsChecked;
            spamming = true;
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
                int i = 0;
                Parallel.ForEach(clients, client =>
                {
                    while (spamming)
                    {
                        bool hasSent = false;
                        int c = 0;
                        while (!hasSent && c < 3)
                        {
                            try
                            {
                                client.SendFriendRequest(userId);
                                hasSent = true;

                                Thread.Sleep((int)delay);
                                if (delete_spam)
                                    client.DeleteFriendRequest(userId);
                            }
                            catch (Exception ex)
                            {
                                c++;
                            }
                        }

                        if (single_spam)
                            break;
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
