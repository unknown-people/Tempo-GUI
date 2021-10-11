using Music_user_bot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using TempoWithGUI.Core;

namespace TempoWithGUI.MVVM.View
{
    /// <summary>
    /// Interaction logic for Proxies.xaml
    /// </summary>
    public partial class Proxies : UserControl
    {
        public static bool freeProxies { get; set; } = true;
        public static bool paidProxies { get; set; } = false;

        public Proxies()
        {
            InitializeComponent();

            if (freeProxies)
            {
                paidRadio.IsChecked = false;
                freeRadio.IsChecked = true;
            }
            else
            {
                paidRadio.IsChecked = true;
                freeRadio.IsChecked = false;
            }

            if (Directory.Exists(App.strWorkPath + "\\proxies"))
                Directory.CreateDirectory(App.strWorkPath + "\\proxies");

            if (!File.Exists(App.strWorkPath + "\\proxies\\http_proxies.txt"))
                File.Create(App.strWorkPath + "\\proxies\\http_proxies.txt");
            if (!File.Exists(App.strWorkPath + "\\proxies\\user_proxies.txt"))
                File.Create(App.strWorkPath + "\\proxies\\user_proxies.txt");
            while (true)
            {
                try
                {
                    SetProxies();
                    break;
                }
                catch (IOException) { Thread.Sleep(100); }
            }
            /*
            Task.Run(() =>
            {
                Dispatcher.Invoke(() => CheckProxies());
                Dispatcher.Invoke(() => SetProxies());
            });
            */
        }
        private void CheckProxies()
        {
            using(StreamWriter writer = new StreamWriter(App.strWorkPath + "\\proxies\\http_proxies1.txt"))
            {
                using(StreamReader reader = new StreamReader(App.strWorkPath + "\\proxies\\http_proxies.txt"))
                {
                    foreach (string proxy in reader.ReadToEnd().Split('\n'))
                    {
                        var proxy_list = proxy.Split(':');
                        bool valid = Proxy.TestProxy("https://discord.com/api/v9/experiments", new Proxy(proxy_list[0], proxy_list[1]));
                        if (valid)
                        {
                            writer.WriteLine(proxy);
                        }
                    }
                }
            }
            File.Delete(App.strWorkPath + "\\proxies\\http_proxies.txt");
            File.Move(App.strWorkPath + "\\proxies\\http_proxies1.txt", App.strWorkPath + "\\proxies\\http_proxies.txt");
        }
        private void SetProxies()
        {
            if (freeProxies)
            {
                while (true)
                {
                    try
                    {
                        using (StreamReader stream = new StreamReader(App.strWorkPath + "\\proxies\\http_proxies.txt", true))
                        {
                            ProxiesIn.Text = stream.ReadToEnd();
                        }
                        break;
                    }
                    catch { }
                }
            }
            else
            {
                while (true)
                {
                    try
                    {
                        using (StreamReader stream = new StreamReader(App.strWorkPath + "\\proxies\\user_proxies.txt", true))
                        {
                            ProxiesIn.Text = stream.ReadToEnd();
                        }
                        Task.Run(() => GetPaidTokens());
                        break;
                    }
                    catch { }
                }
            }
        }
        public static void GetPaidTokens()
        {
            if (Proxy.working_proxies_paid == null)
                Proxy.working_proxies_paid = new List<Proxy>() { };
            using (StreamReader stream = new StreamReader(App.strWorkPath + "\\proxies\\user_proxies.txt", true))
            {
                while (true)
                {
                    var line = stream.ReadLine();
                    if (line == null)
                        break;
                    line = line.Trim('\n').Trim(' ');
                    var proxy_list = line.Split(':');
                    if (proxy_list.Length == 2)
                    {
                        Proxy.working_proxies_paid.Add(new Proxy(proxy_list[0], proxy_list[1]));
                    }
                    else if (proxy_list.Length == 4)
                    {
                        Proxy.working_proxies_paid.Add(new Proxy(proxy_list[0], proxy_list[1], proxy_list[2], proxy_list[3]));
                    }
                }
            }
        }
        private void SaveProxies()
        {
            if (freeProxies)
            {
                using (StreamWriter stream = new StreamWriter(App.strWorkPath + "\\proxies\\http_proxies1.txt"))
                {
                    stream.Write(ProxiesIn.Text);
                }
                File.Delete(App.strWorkPath + "\\proxies\\http_proxies.txt");
                File.Move(App.strWorkPath + "\\proxies\\http_proxies1.txt", App.strWorkPath + "\\proxies\\http_proxies.txt");
            }
            else
            {
                using (StreamWriter stream = new StreamWriter(App.strWorkPath + "\\proxies\\user_proxies1.txt"))
                {
                    stream.Write(ProxiesIn.Text);
                }
                File.Delete(App.strWorkPath + "\\proxies\\user_proxies.txt");
                File.Move(App.strWorkPath + "\\proxies\\user_proxies1.txt", App.strWorkPath + "\\proxies\\user_proxies.txt");
            }
        }

        private void PaidRadio_Click(object sender, RoutedEventArgs e)
        {
            freeProxies = false;
            paidProxies = true;
            SetProxies();
        }

        private void FreeRadio_click(object sender, RoutedEventArgs e)
        {
            freeProxies = true;
            paidProxies = false;
            SetProxies();
        }

        private void BuyBtn_Click(object sender, RoutedEventArgs e)
        {
            string url = "https://dashboard.intenseproxy.com/";
            Process.Start(url);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveProxies();
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            SetProxies();
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            string proxy = "user_proxies.txt";
            if (freeProxies)
                proxy = "http_proxies.txt";
            string args = App.strWorkPath + $"\\proxies\\{proxy}";
            Process.Start(new ProcessStartInfo()
            {
                FileName = "notepad.exe",
                Arguments = args
            });
        }
    }
}
