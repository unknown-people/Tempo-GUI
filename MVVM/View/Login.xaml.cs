using Music_user_bot;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;

namespace TempoWithGUI.MVVM.View
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        public static bool hasTempo { get; set; } = false;
        public Login()
        {
            InitializeComponent();
            Debug.Log("Opened Login prompt");
            UsernameIn.Text = Settings.Default.tk1;
            PasswordIn.Text = Settings.Default.tk2;
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var username = UsernameIn.Text;
            var pass = PasswordIn.Text;

            var key = login(username, pass);
            if(key != null)
            {
                App.api_key = key;
                Settings.Default.tk1 = username;
                Settings.Default.tk2 = pass;
                Settings.Default.Save();
                Settings.Default.Reload();
                App.SaveSettings();
                while (true)
                {
                    ServiceController sc = null;
                    try
                    {
                        sc = new ServiceController("TempoUpdater");
                    }
                    catch (InvalidOperationException)
                    {
                        continue;
                    }
                    if (sc == null)
                        break;
                    switch (sc.Status)
                    {
                        case ServiceControllerStatus.Stopped:
                            sc.Start();
                            break;
                        case ServiceControllerStatus.Paused:
                            sc.Continue();
                            break;
                        default:
                            break;
                    }
                    break;
                }

                App.programFiles = Environment.ExpandEnvironmentVariables("%ProgramW6432%");

                if (!Directory.Exists(App.strWorkPath + "\\tokens"))
                    Directory.CreateDirectory(App.strWorkPath + "\\tokens");

                Task.Run(Spotify.Login);
                //Task.Run(() => Proxy.GetProxies("https://www.youtube.com"));
                Task.Run(() => Proxy.GetProxies("https://discord.com"));

                System.Timers.Timer timer_fetch_discord_proxies = new System.Timers.Timer();
                timer_fetch_discord_proxies.Elapsed += new ElapsedEventHandler(App.OnElapsedTimeDiscordProxies);
                timer_fetch_discord_proxies.Interval = 5 * 60 * 1000;
                timer_fetch_discord_proxies.Enabled = true;
                /*
                System.Timers.Timer timer_fetch_proxies = new System.Timers.Timer();
                timer_fetch_proxies.Elapsed += new ElapsedEventHandler(App.OnElapsedTimeProxies);
                timer_fetch_proxies.Interval = 5 * 60 * 1000;
                timer_fetch_proxies.Enabled = true;
                */
                using (StreamReader stream = new StreamReader(App.strWorkPath + "\\tokens\\tokens.txt", true))
                {
                    tokens._tokens = new List<DiscordToken>() { };
                    while (true)
                    {
                        var line = stream.ReadLine();
                        if (line == null || line.Trim('\n') == "")
                            break;
                        var token_array = line.Split(':');
                        if (token_array.Length == 3)
                            tokens._tokens.Add(new DiscordToken(true, token_array[0], "U"));
                        else
                            tokens._tokens.Add(new DiscordToken(true, token_array[1], token_array[0]));
                    }
                }
                MainWindow window = new MainWindow();

                window.Show();
                this.Hide();;
            }
            else
            {
                ErrorMsg.Text = "Invalid credentials.\nRegister your account at unknown-people.it/register";
                ErrorMsg.Visibility = Visibility.Visible;
                return;
            }
        }
        public static string login(string username, string password)
        {
            var request_url = $"https://unknown-people.it/api/accounts?username={username}&password={password}";
            HttpClient client = new HttpClient();
            var response = client.SendAsync(new HttpRequestMessage()
            {
                Method = new HttpMethod("GET"),
                RequestUri = new Uri(request_url)
            }).GetAwaiter().GetResult();
            if (response.StatusCode.ToString() == "BadRequest")
                return null;
            var jtoken = JToken.Parse(response.Content.ReadAsStringAsync().Result);
            var json = JObject.Parse(jtoken.ToString());

            if (json.Value<string>("tempo") == "1")
                hasTempo = true;
            else
                return null;

            return json.Value<string>("api_key");
        }
    }
}
