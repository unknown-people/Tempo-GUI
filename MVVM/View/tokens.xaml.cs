using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
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

namespace TempoWithGUI.MVVM.View
{
    /// <summary>
    /// Interaction logic for tokens.xaml
    /// </summary>
    public partial class tokens : UserControl
    {
        public static bool boughtTokens { get; set; } = false;
        public static List<DiscordToken> _tokens;
        public tokens()
        {
            InitializeComponent();
            if (!File.Exists(App.strWorkPath + "\\tokens\\tokens.txt"))
                File.Create(App.strWorkPath + "\\tokens\\tokens.txt");
            _tokens = new List<DiscordToken>();

            SetTokens();
        }
        private void SetTokens()
        {
            using (StreamReader stream = new StreamReader(App.strWorkPath + "\\tokens\\tokens.txt", true))
            {
                _tokens = new List<DiscordToken>() { };
                while (true)
                {
                    var line = stream.ReadLine();
                    if (line == null || line.Trim('\n') == "")
                        break;
                    var token_array = line.Split(':');
                    if(token_array.Length == 3)
                        _tokens.Add(new DiscordToken(true, token_array[0], "U"));
                    else 
                        _tokens.Add(new DiscordToken(true, token_array[1], token_array[0]));
                }
            }
            ListTokens.ItemsSource = _tokens;
        }
        private string[] GetTokenInfo(string token)
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
                        if(token_array[1] == token)
                        {
                            email = token_array[2];
                            password = token_array[3];
                            creation = token_array[4];
                            country = token_array[5];
                            if (country == "NULL")
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
            int line_skip = _tokens.IndexOf((DiscordToken)ListTokens.SelectedItem);
            try
            {
                _tokens = (List<DiscordToken>)_tokens.Where(n => n.ToString() != ListTokens.SelectedItem.ToString()).ToList();
            }
            catch (NullReferenceException){ return; }
            ListTokens.ItemsSource = null;
            ListTokens.ItemsSource = _tokens;
            int i = 0;
            using (StreamReader stream = new StreamReader(App.strWorkPath + "\\tokens\\tokens.txt", true))
            {
                using (StreamWriter writer = new StreamWriter(App.strWorkPath + "\\tokens\\tokens1.txt"))
                {
                    while (true)
                    {
                        var line = stream.ReadLine().Trim('\n').Trim(' ').Trim('\t');
                        if (line == null)
                            break;
                        if (i == line_skip)
                            continue;
                        writer.WriteLine(line);

                        i++;
                    }
                }
            }
            File.Delete(App.strWorkPath + "\\tokens\\tokens.txt");
            File.Move(App.strWorkPath + "\\tokens\\tokens1.txt", App.strWorkPath + "\\tokens\\tokens.txt");
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
