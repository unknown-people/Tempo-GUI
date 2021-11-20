using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
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
using TempoWithGUI.Core;
using TempoWithGUI.MVVM.View.RaidView;

namespace TempoWithGUI.MVVM.View
{
    /// <summary>
    /// Interaction logic for tokens.xaml
    /// </summary>
    public partial class tokens : UserControl
    {
        public static bool loadingTokens { get; set; } = false;
        public static bool boughtTokens { get; set; } = false;
        public static List<DiscordToken> _tokens { get; set; } = null;
        public static bool checking { get; set; }
        public tokens()
        {
            InitializeComponent();
            Debug.Log("Opened Tokens control");
            if (!File.Exists(App.strWorkPath + "\\tokens\\tokens.txt"))
                File.Create(App.strWorkPath + "\\tokens\\tokens.txt");
            if(_tokens == null)
                _tokens = new List<DiscordToken>();
            this.Cursor = Cursors.AppStarting;
            SetTokens();
            this.Cursor = Cursors.Arrow;
        }
        public void SetTokens()
        {
            loadingTokens = true;
            var buff = new List<DiscordToken>() { };
            using (StreamReader stream = new StreamReader(App.strWorkPath + "\\tokens\\tokens.txt", true))
            {
                while (true)
                {
                    var line = stream.ReadLine();
                    if (line == null || line.Trim('\n') == "")
                        break;
                    var token_array = line.Split(':');
                    if(token_array.Length == 3)
                        buff.Add(new DiscordToken(true, token_array[0], "U"));
                    else
                    {
                        if(token_array[0].Length != 1)
                        {
                            buff.Add(new DiscordToken(true, token_array[0], "U"));
                        }
                        else
                        {
                            buff.Add(new DiscordToken(true, token_array[1], token_array[0]));
                        }
                    }
                }
            }
            var i = 0;
            foreach(var tk in _tokens)
            {
                try
                {
                    if (tk.Active)
                    {
                        buff[i].Active = true;
                    }
                    else
                    {
                        buff[i].Active = false;
                    }
                }
                catch (Exception ex)
                {
                    App.mainView.logPrint($"Couldn't set tokens: {ex.Message}");
                }
                i++;
            }
            _tokens = buff;
            ListTokens.ItemsSource = _tokens;
            TkCounter.Text = _tokens.Count.ToString() + " Tokens";
            loadingTokens = false;
        }
        public static string[] GetTokenInfo(string token)
        {
            string email = "";
            string password = "";
            string creation = "";
            string country = "";
            using (StreamReader stream = new StreamReader(App.strWorkPath + "\\tokens\\tokens.txt", true))
            {
                while (true)
                {
                    var line = stream.ReadLine();
                    if (line == null)
                        break;
                    var token_array = line.Split(':');
                    if (token_array.Length == 3)
                    {
                        if(token_array[0] == token)
                        {
                            email = token_array[1];
                            password = token_array[2];
                        }
                    }
                    else
                    {
                        if(token_array.Length == 6)
                        {
                            if (token_array[1] == token)
                            {
                                email = token_array[2];
                                password = token_array[3];
                                creation = token_array[4];
                                country = token_array[5];
                                if (country == "NULL")
                                    country = "";
                            }
                        }
                        else if(token_array.Length == 5)
                        {
                            if (token_array[0] == token)
                            {
                                email = token_array[1];
                                password = token_array[2];
                                creation = token_array[3];
                                country = token_array[4];
                                if (country == "NULL")
                                    country = "";
                            }
                        }
                        else if (token_array.Length == 4)
                        {
                            if (token_array[0] == token)
                            {
                                email = token_array[1];
                                password = token_array[2];
                                creation = token_array[3];
                                if (country == "NULL")
                                    country = "";
                            }
                        }
                        else if (token_array.Length == 1)
                        {
                            email = "";
                            password = "";
                            creation = "";
                            country = "";
                        }
                    }
                }
            }
            var acc = new string[5];
            acc[0] = (token);
            acc[1] = (email);
            acc[2] = (password);
            acc[3] = (creation);
            acc[4] = (country);
            return acc;
        }

        private void Select_Click(object sender, RoutedEventArgs e)
        {
            int i = 0;
            foreach(var token in _tokens)
            {
                _tokens[i].Active = true;
                i++;
            }
            ListTokens.ItemsSource = null;
            ListTokens.ItemsSource = _tokens;
        }
        private void Deselect_Click(object sender, RoutedEventArgs e)
        {
            int i = 0;
            foreach (var token in _tokens)
            {
                _tokens[i].Active = false;
                i++;
            }
            ListTokens.ItemsSource = null;
            ListTokens.ItemsSource = _tokens;
        }

        private void Checker_Click(object sender, RoutedEventArgs e)
        {
            if (checking)
                return;
            checking = true;
            var checker = new Checker();
            checker.Show();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var popup = new AddTokenPopup();
            popup.ShowDialog();
            SetTokens();
            ListTokens.ItemsSource = null;
            ListTokens.ItemsSource = _tokens;
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            while (tokens.loadingTokens)
                Thread.Sleep(100);
            loadingTokens = true;
            var selected = ListTokens.SelectedItems;
            var buff = new List<DiscordToken>() { };
            foreach(var el in selected)
            {
                buff.Add((DiscordToken)el);
            }
            _tokens = _tokens.Except(buff).ToList();

            ListTokens.ItemsSource = null;
            ListTokens.ItemsSource = _tokens;
            int i = 0;
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
                        if (line_arr.Length == 3)
                        {
                            if (!isInListTokens(line_arr[0]))
                                continue;
                        }
                        else
                        {
                            if (!isInListTokens(line_arr[1]))
                                continue;
                        }
                        writer.WriteLine(line);

                        i++;
                    }
                }
            }
            File.Delete(App.strWorkPath + "\\tokens\\tokens.txt");
            File.Move(App.strWorkPath + "\\tokens\\tokens1.txt", App.strWorkPath + "\\tokens\\tokens.txt");
            TkCounter.Text = _tokens.Count.ToString() + " Tokens";
            loadingTokens = false;
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            string args = App.strWorkPath + "\\tokens\\tokens.txt";
            Process.Start(new ProcessStartInfo()
            {
                FileName = "notepad.exe",
                Arguments = args
            });
        }

        private void Shop_Click(object sender, RoutedEventArgs e)
        {
            var shop = new Shop();
            shop.Show();
        }

        private void Label_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var acc = GetTokenInfo(((Label)e.Source).Content.ToString());
            var accInfo = new EditTokenPopup(acc[0], acc[1], acc[2], acc[3]);
            accInfo.ShowDialog();
        }
        private bool isInListTokens(string token)
        {
            foreach (var tk in ListTokens.Items)
            {
                if (tk.ToString() == token && ((DiscordToken)tk).Active)
                    return true;
            }
            return false;
        }

        private void Reload_Click(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.AppStarting;
            SetTokens();
            this.Cursor = Cursors.Arrow;
        }

        /*
private void CheckBox_Checked(object sender, RoutedEventArgs e)
{
int i = 0;
foreach(var el in ListTokens.SelectedItems)
{
foreach(var token in _tokens)
{
if(token == (DiscordToken)el)
{
_tokens[i].Active = true;
break;
}
}
i++;
}
ListTokens.ItemsSource = null;
ListTokens.ItemsSource = _tokens;
}
*/
    }
}
