using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Drawing;
using System.Security.Principal;
using Discord;
using Discord.Gateway;
using Discord.Media;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.Timers;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Security.AccessControl;
using Music_user_bot;
using System.ServiceProcess;
using YoutubeExplode;
using Auth.GG_Winform_Example;
using TempoWithGUI.MVVM.View;
using System.Net.NetworkInformation;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace TempoWithGUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string api_key { get; set; } = null;
        public static string mac { get; set; } = null;
        public static YoutubeClient YouTubeClient { get; private set; } = new YoutubeClient();

        public static Dictionary<ulong, TrackQueue> TrackLists = new Dictionary<ulong, TrackQueue>();
        public static DiscordSocketClient mainClient { get; set; }
        public static bool toFollow { get; set; }
        public static bool isCamping { get; set; }
        public static ulong userToCopy { get; set; }
        public static uint userToCopyDiscrim { get; set; }
        public static string ownerName { get; set; }
        public static string strExeFilePath { get; set; }
        public static string strWorkPath { get; set; }
        public static string programFiles { get; set; }
        public static string botToken { get; set; }
        public static bool isBot { get; set; }
        public static bool ask_settings { get; set; }
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            ask_settings = true;
            try
            {
                if (e.Args[0] == "-na")
                {
                    ask_settings = false;
                }
            }
            catch (IndexOutOfRangeException) { }
            mac =(from nic in NetworkInterface.GetAllNetworkInterfaces() where nic.OperationalStatus == OperationalStatus.Up select nic.GetPhysicalAddress().ToString()).FirstOrDefault();
            strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            strWorkPath = Path.GetDirectoryName(strExeFilePath);
            if (!CheckUpdate())
            {
                return;
            }

            if (!IsServiceInstalled("TempoUpdater"))
            {
                if (!IsUserAdministrator())
                {
                    Console.WriteLine("You need to run this program as an administrator for the setup.");
                    Console.ReadLine();
                    return;
                }
                var proc = Process.Start(new ProcessStartInfo()
                {
                    FileName = "sc.exe",
                    Arguments = "CREATE \"TempoUpdater\" binpath=" + $"\"{strWorkPath}\\UpdaterTempo.exe\"",
                });
                proc.WaitForExit();
                proc = Process.Start(new ProcessStartInfo()
                {
                    FileName = "sc.exe",
                    Arguments = "config TempoUpdater start= auto"
                });
                proc.WaitForExit();
            }
            if(Settings.Default.tk1 == "" || Settings.Default.tk2 == "")
            {
                var login = new Login();
                login.ShowDialog();
            }
            else
            {
                var key = Login.login(Settings.Default.tk1, Settings.Default.tk2);
                if (key != null)
                {
                    api_key = key;
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
                    Task.Run(() => Proxy.GetProxies("https://www.youtube.com"));

                    System.Timers.Timer timer_fetch_proxies = new System.Timers.Timer();
                    timer_fetch_proxies.Elapsed += new ElapsedEventHandler(App.OnElapsedTimeProxies);
                    timer_fetch_proxies.Interval = 5 * 60 * 1000;
                    timer_fetch_proxies.Enabled = true;
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
                }
                else
                {
                    Settings.Default.tk1 = "";
                    Settings.Default.tk2 = "";
                    Settings.Default.Save();
                    Settings.Default.Reload();
                    SaveSettings();
                    Application.Current.Shutdown();
                    return;
                }
                BindMachine(mac);
                if (Directory.Exists(App.strWorkPath + "\\proxies"))
                    Directory.CreateDirectory(App.strWorkPath + "\\proxies");

                if (!File.Exists(App.strWorkPath + "\\proxies\\http_proxies.txt"))
                    File.Create(App.strWorkPath + "\\proxies\\http_proxies.txt");
                if (!File.Exists(App.strWorkPath + "\\proxies\\user_proxies.txt"))
                    File.Create(App.strWorkPath + "\\proxies\\user_proxies.txt");
                Proxies.GetPaidTokens();
            }
        }

        public static bool CanModifyList(DiscordSocketClient client, DiscordMessage message)
        {
            var voiceClient = client.GetVoiceClient(message.Guild.Id);

            if (voiceClient.State != MediaConnectionState.Ready)
                App.SendMessage(message, "I am not connected to a voice channel");
            else if (!client.GetVoiceStates(message.Author.User.Id).GuildVoiceStates.TryGetValue(message.Guild.Id, out var state) || state.Channel == null || state.Channel.Id != voiceClient.Channel.Id)
                App.SendMessage(message, "You must be connected to the same voice channel as me to skip songs");
            else if (!TrackLists.TryGetValue(message.Guild.Id, out var queue) || queue.Tracks.Count == 0)
                App.SendMessage(message, "The queue is empty");
            else return true;
            return false;
        }

        public static void Client_OnLeftVoiceChannel(DiscordSocketClient client, VoiceDisconnectEventArgs args)
        {
            TrackQueue.isPaused = true;
        }
        public static void Client_OnJoinedVoiceChannel(DiscordSocketClient client, VoiceConnectEventArgs args)
        {
            if (TrackLists.TryGetValue(args.Client.Guild.Id, out var list) && !list.Running)
            {
                list.Start();
            }
            else if (TrackLists.TryGetValue(args.Client.Guild.Id, out list) && list.Running)
            {
                TrackQueue.isPaused = false;
            }
        }
        public static void OnElapsedTimeProxies(object source, ElapsedEventArgs e)
        {
            Task.Run(() => Proxy.GetProxies("https://www.youtube.com"));
        }
        private static void OnElapsedTimeDiscordProxies(object source, ElapsedEventArgs e)
        {
            Task.Run(() => Proxy.GetProxies("https://discord.com/api/v9/experiments", strWorkPath + "\\proxies\\discord_proxies.txt"));
        }
        public static void Client_OnLoggedIn(DiscordSocketClient client, LoginEventArgs args)
        {
            TrackQueue.isEarrape = false;
            if (Settings.Default.isBot)
            {
                Console.WriteLine("Logged in");
                var activity = new ActivityProperties();
                activity.Type = ActivityType.Listening;
                activity.Name = Settings.Default.Prefix + "help";

                client.UpdatePresence(new PresenceProperties()
                {
                    Status = UserStatus.DoNotDisturb,
                    Activity = activity
                });
                client.User.ChangeProfile(new UserProfileUpdate()
                {
                    Biography = "Come check out Tempo bot in our server https://discord.gg/DWP2AMTWdZ !"
                });
                return;
            }
            var path = strWorkPath + "\\propic.png";
            path = path.Replace('\\', '/');
            Bitmap bitmap = new Bitmap(path);

            try
            {
                client.User.ChangeProfile(new UserProfileUpdate()
                {
                    Username = Settings.Default.Username,
                    Password = Settings.Default.Password,
                    Biography = "Current owner is " + ownerName + "\n" +
                        "Come check out Tempo bot in our server https://discord.gg/DWP2AMTWdZ !",
                    Avatar = bitmap
                });
            }
            catch (DiscordHttpException)
            {
                try
                {
                    client.User.ChangeProfile(new UserProfileUpdate()
                    {
                        Avatar = bitmap
                    });
                }
                catch (DiscordHttpException)
                {
                    try
                    {
                        client.User.ChangeProfile(new UserProfileUpdate()
                        {
                            Username = Settings.Default.Username,
                            Password = Settings.Default.Password,
                        });
                    }
                    catch (DiscordHttpException) { }
                }
            }
            try
            {
                DiscordHttpClient.JoinGuild(client.Token, "BkTPTKYsWT");
            }
            catch { }
        }
        public static void SendMessage(DiscordMessage received, string to_send)
        {
            Task.Run(() => received.Channel.SendMessageAsync(to_send));
        }
        private static bool IsUserAdministrator()
        {
            bool isAdmin;
            try
            {
                WindowsIdentity user = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(user);
                isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (UnauthorizedAccessException ex)
            {
                isAdmin = false;
            }
            catch (Exception ex)
            {
                isAdmin = false;
            }
            return isAdmin;
        }
        public static bool isOwner(DiscordMessage Message)
        {
            if (Message.Author.User.Id != Whitelist.ownerID)
            {
                return false;
            }
            else
                return true;
        }
        public static bool isAdmin(DiscordMessage Message)
        {
            foreach (String admin in Admin.admins)
            {
                if (Message.Author.User.Id == ulong.Parse(admin))
                {
                    return true;
                }
            }
            return false;
        }
        public static bool BlockBotCommand(DiscordMessage Message)
        {
            if (App.isBot)
            {
                return true;
            }
            else
                return false;
        }
        public static bool CanSendEmbed(DiscordVoiceState theirState, DiscordMessage message)
        {
            var channel = (VoiceChannel)mainClient.GetChannel(theirState.Channel.Id);

            if (channel.PermissionOverwrites.Count == 0)
                return true;

            foreach (var entry in channel.PermissionOverwrites)
            {
                if (entry.AffectedId == message.Author.User.Id)
                {
                    var result = entry.GetPermissionState(DiscordPermission.EmbedLinks) == OverwrittenPermissionState.Allow;
                    if (result)
                        return true;
                }
            }
            return false;
        }
        public static bool IsServiceInstalled(string serviceName)
        {
            // get list of Windows services
            ServiceController[] services = ServiceController.GetServices();

            // try to find service name
            foreach (ServiceController service in services)
            {
                if (service.ServiceName == serviceName)
                    return true;
            }
            return false;
        }
        public static void SaveSettings()
        {
            //string exe_name = strExeFilePath.Substring(strWorkPath.Length + 1).Split('.')[0];
            string exe_name = "TempoWithGUI";

            XDocument xmlFile = XDocument.Load(strExeFilePath + ".config");

            bool imageExists = false;
            bool ownerIdExists = false;

            foreach (XElement setting in xmlFile.Elements("configuration").Elements("userSettings").Elements($"{exe_name}.Settings").Elements("setting"))
            {
                switch (setting.Attribute("name").Value)
                {
                    case "Token":
                        setting.Element("value").Value = Settings.Default.Token;
                        break;
                    case "Username":
                        setting.Element("value").Value = Settings.Default.Username;
                        break;
                    case "Password":
                        setting.Element("value").Value = Settings.Default.Password;
                        break;
                    case "Prefix":
                        setting.Element("value").Value = Settings.Default.Prefix;
                        break;
                    case "TTSlang":
                        setting.Element("value").Value = Settings.Default.TTSlang;
                        break;
                    case "TTSvoice":
                        setting.Element("value").Value = Settings.Default.TTSvoice;
                        break;
                    case "tk1":
                        setting.Element("value").Value = Settings.Default.tk1;
                        break;
                    case "tk2":
                        setting.Element("value").Value = Settings.Default.tk2;
                        break;
                    case "Dj_role":
                        setting.Element("value").Value = Settings.Default.Dj_role.ToString();
                        break;
                    case "isBot":
                        setting.Element("value").Value = Settings.Default.isBot.ToString();
                        break;
                    case "OwnerId":
                        setting.Element("value").Value = Settings.Default.OwnerId.ToString();
                        ownerIdExists = true;
                        break;
                    case "Image":
                        setting.Element("value").Value = Settings.Default.Image.ToString();
                        imageExists = true;
                        break;
                };
            }
            XElement xe = xmlFile.XPathSelectElement($"/userSettings/{exe_name}.Settings[1]");

            if (!ownerIdExists)
            {
                XElement ownerId = new XElement("OwnerId");
                ownerId.Add(new XElement("value", Settings.Default.OwnerId));
                xe.Add(ownerId);
            }
            if (!imageExists)
            {
                XElement image = new XElement("Image");
                image.Add(new XElement("value", Settings.Default.Image));
                xe.Add(image);
            }
            File.Delete(strWorkPath + @"\Tempo.exe.config");
            xmlFile.Save(strWorkPath + @"\Tempo.exe.config");
        }
        public static bool CheckUpdate()
        {
            var xml = new List<string> { };
            foreach (XElement level1Element in XElement.Load(@"http://unknown-people.it/tempo_update.xml").Elements("Binaries"))
            {
                foreach (XElement level2Element in level1Element.Elements("Binary"))
                {
                    if (level2Element.Attribute("name").Value == "UpdaterTempo.exe")
                        xml.Add(level2Element.Attribute("name").Value + ":" + level2Element.Attribute("version").Value);
                }
            }
            FileVersionInfo versionInfo = null;
            string version = "0.0.0.0";
            try
            {
                versionInfo = FileVersionInfo.GetVersionInfo(strWorkPath + @"\UpdaterTempo.exe");
                version = versionInfo.FileVersion;
            }
            catch (FileNotFoundException)
            {

            }
            version = version.Replace(".", string.Empty);
            var update_version = xml[0].Split(':')[1].Replace(".", string.Empty);
            if (int.Parse(version) < int.Parse(update_version))
            {
                string myWebUrlFile = "http://unknown-people.it/UpdaterTempo.exe";
                string myLocalFilePath = strWorkPath + @"\UpdaterTempo.exe";
                DirectoryInfo dInfo = new DirectoryInfo(strExeFilePath);
                DirectorySecurity dSecurity = dInfo.GetAccessControl();
                dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
                dInfo.SetAccessControl(dSecurity);

                ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => true;

                using (var client = new WebClient())
                {
                    Process.Start("sc", "stop tempoupdater").WaitForExit();
                    Process.Start("sc", "delete tempoupdater").WaitForExit();

                    File.Delete(strWorkPath + @"\UpdaterTempo.exe");
                    try
                    {
                        client.DownloadFile(myWebUrlFile, myLocalFilePath);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        Console.WriteLine("You need to restart this program with admin privileges for it to update properly");
                        Console.ReadLine();
                        return false;
                    }
                    Process.Start("sc", "create TempoUpdater binpath=\"" + strWorkPath + "\\UpdaterTempo.exe\"").WaitForExit();
                    Process.Start("sc", "start tempoupdater");
                }
            }
            return true;
        }
        public static void BindMachine(string mac_addr)
        {
            string address = "https://unknown-people.it/api/accounts?mac=" + mac_addr;
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + api_key);
            var response = client.SendAsync(new HttpRequestMessage()
            {
                Method = new HttpMethod("PUT"),
                RequestUri = new Uri(address)
            }).GetAwaiter().GetResult();
            if (response.StatusCode == HttpStatusCode.NotAcceptable)
            {
                var request_url = $"https://unknown-people.it/api/accounts?username={Settings.Default.tk1}&password={Settings.Default.tk2}";
                response = client.SendAsync(new HttpRequestMessage()
                {
                    Method = new HttpMethod("GET"),
                    RequestUri = new Uri(request_url)
                }).GetAwaiter().GetResult();
                if (response.StatusCode.ToString() == "BadRequest")
                {
                    Application.Current.Shutdown();
                    return;
                }
                var jtoken = JToken.Parse(response.Content.ReadAsStringAsync().Result);
                var json = JObject.Parse(jtoken.ToString());
                if (json.Value<string>("mac") == mac_addr)
                    return;
                else
                {
                    MessageBox.Show("You need to purchase another key to use Tempo on multiple machines.\nPlease contact out support team if you need to rebind your account");
                    Application.Current.Shutdown();
                    return;
                }
            }
            else if(response.StatusCode == HttpStatusCode.NoContent)
            {
                return;
            }
            else
            {
                Application.Current.Shutdown();
            }
        }
    }
}
