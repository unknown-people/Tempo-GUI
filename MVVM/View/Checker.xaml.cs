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
    /// Interaction logic for Checker.xaml
    /// </summary>
    public partial class Checker : Window
    {
        public static bool checking { get; set; } = false;
        public static List<DiscordToken> _tokens_invalid;
        public Checker()
        {
            InitializeComponent();
            Set_Light(checking);
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            tokens.checking = false;
            checking = false;
            this.Hide();;
        }
        private void Label_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var acc = tokens.GetTokenInfo(((Label)e.Source).Content.ToString());
            var accInfo = new EditTokenPopup(acc[0], acc[1], acc[2], acc[3]);
            accInfo.ShowDialog();
        }
        private void Select_Click(object sender, RoutedEventArgs e)
        {
            int i = 0;
            foreach (var token in _tokens_invalid)
            {
                _tokens_invalid[i].Active = true;
                i++;
            }
            ListTokens.ItemsSource = null;
            ListTokens.ItemsSource = _tokens_invalid;
        }
        private void Deselect_Click(object sender, RoutedEventArgs e)
        {
            int i = 0;
            foreach (var token in _tokens_invalid)
            {
                _tokens_invalid[i].Active = false;
                i++;
            }
            ListTokens.ItemsSource = null;
            ListTokens.ItemsSource = _tokens_invalid;
        }

        private void Check_Click(object sender, RoutedEventArgs e)
        {
            checking = true;
            Set_Light(checking);
            Task.Run(() =>
            {
                _tokens_invalid = new List<DiscordToken>() { };
                foreach (var token in tokens._tokens)
                {
                    try
                    {
                        DiscordClient client = new DiscordClient(token.Token);
                        client.GetVoiceRegions();
                    }
                    catch (Exception ex)
                    {
                        _tokens_invalid.Add(token);
                        Dispatcher.Invoke(() => { ListTokens.ItemsSource = null; ListTokens.ItemsSource = _tokens_invalid; });
                    }
                }

                checking = false;
                Dispatcher.Invoke(() => Set_Light(checking));
            });
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

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            while (tokens.loadingTokens)
                Thread.Sleep(100);
            tokens.loadingTokens = true;
            Task.Run(() =>
            {
                using (StreamReader stream = new StreamReader(App.strWorkPath + "\\tokens\\tokens.txt", true))
                {
                    using (StreamWriter writer = new StreamWriter(App.strWorkPath + "\\tokens\\tokens1.txt"))
                    {
                        while (true)
                        {
                            var line = stream.ReadLine();
                            if (line == null)
                                break;
                            line = line.Trim('\n').Trim(' ').Trim('\t');
                            var line_arr = line.Split(':');
                            if(line_arr.Length == 3)
                            {
                                if (isInListTokens(line_arr[0]))
                                    continue;
                            }
                            else
                            {
                                if (isInListTokens(line_arr[1]))
                                    continue;
                            }
                            writer.WriteLine(line);
                        }
                    }
                }
                File.Delete(App.strWorkPath + "\\tokens\\tokens.txt");
                File.Move(App.strWorkPath + "\\tokens\\tokens1.txt", App.strWorkPath + "\\tokens\\tokens.txt");

                Dispatcher.Invoke(() =>
                {
                    foreach (var el in _tokens_invalid.Where(o => o.Active))
                    {
                        _tokens_invalid = _tokens_invalid.Where(o => o.Token != el.ToString()).ToList();
                        tokens._tokens = tokens._tokens.Where(o => o.Token != el.ToString()).ToList();
                    }
                    ListTokens.ItemsSource = null;
                    ListTokens.ItemsSource = _tokens_invalid;
                });
                tokens.loadingTokens = false;
            });
        }
        private bool isInListTokens(string token)
        {
            foreach(var tk in ListTokens.Items)
            {
                if (tk.ToString() == token && ((DiscordToken)tk).Active)
                    return true;
            }
            return false;
        }
    }
}
