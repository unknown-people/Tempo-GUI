using Discord;
using Leaf.xNet;
using Microsoft.WindowsAPICodePack.Dialogs;
using Music_user_bot;
using System;
using System.Collections.Generic;
using System.Drawing;
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
using TempoWithGUI.MVVM.View.RaidView;
using TempoWithGUI.MVVM.ViewModel;
using Brushes = System.Windows.Media.Brushes;

namespace TempoWithGUI.MVVM.View.ProfilesView
{
    /// <summary>
    /// Interaction logic for nameProfiles.xaml
    /// </summary>
    public partial class nameProfiles : UserControl
    {
        public List<string> tokens_list { get; set; } = new List<string>() { };
        public bool isChanging { get; set; } = false;
        public bool isFile { get; set; } = false;
        public List<string>  names { get; set; }
        public nameProfiles()
        {
            InitializeComponent();
            Debug.Log("Name changer interface initialized");
            if ((bool)ButtonFile.IsChecked)
            {
                ExploreBtn.Visibility = Visibility.Visible;
            }
            else
            {
                ExploreBtn.Visibility = Visibility.Hidden;
            }
        }
        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (isChanging)
                return;
            this.Cursor = Cursors.AppStarting;
            var file = FileIn.Text;
            bool useProxies = (bool)ProxiesCB.IsChecked;
            CustomMessageBox popup = null;
            names = new List<string>() { };
            if (!isFile)
            {
                names.Add(file);
            }
            else
            {
                if (File.Exists(file))
                {
                    using(StreamReader reader = new StreamReader(file))
                    {
                        while (true)
                        {
                            var line = reader.ReadLine();
                            if (line == null)
                                break;
                            line = line.Trim().Trim('\n');
                            if (line == "")
                                break;
                            int maxl = 32;
                            if (line.Length < 32)
                                maxl = line.Length;
                            names.Add(line.Substring(0, maxl));
                        }
                    }
                }
                else
                {
                    popup = new CustomMessageBox("Please use a valid image (jpg or png)");
                    popup.ShowDialog();
                    isChanging = false;
                    Set_Light(isChanging);
                    return;
                }
            }
            float delay = 0;
            int tokens_n = 0;
            if (!float.TryParse(DelayIn.Text, out delay) || (!int.TryParse(TokensIn.Text, out tokens_n)))
            {
                if (delay == 0)
                    delay = 250;
            }
            var max_tokens = TokensIn.Text;
            int max = tokens._tokens.Count;
            if (!(max_tokens == null || max_tokens.ToString().Trim('\n') == ""))
            {
                if (!int.TryParse(max_tokens, out max))
                {
                    MessageBox.Show("Please insert a valid value for max tokens");
                    StartBtn.Cursor = Cursors.Arrow;
                    return;
                }
            }
            isChanging = true;
            Random rnd = new Random();
            Proxy proxy = null;
            var proxies_list = Proxy.working_proxies;
            if (Proxies.paidProxies)
                proxies_list = Proxy.working_proxies_paid;
            var original_proxies = proxies_list;
            var clients = new List<DiscordClient>() { };
            var joined = 0;
            var token_list = new List<string>() { };
            foreach (var tk in tokens._tokens)
            {
                if (tk.Active)
                    token_list.Add(tk.Token);
            }
            MainViewModel.log.Show();
            this.Cursor = Cursors.Arrow;
            Set_Light(isChanging);
            var thread_pool = new List<Thread>() { };
            Thread addClients = new Thread(() =>
            {
                int i = 0;
                var threads = 0;
                while (tokens.loadingTokens)
                {
                    Dispatcher.Invoke(() =>
                    {
                        App.mainView.logPrint($"Token list loading, please wait a minute");
                    });
                    Thread.Sleep(1000);
                }
                var tk_list = token_list.ToArray();
                foreach (var token in tk_list)
                {
                    if (!isChanging || thread_pool.Count >= max)
                        return;
                    Thread join1 = new Thread(() =>
                    {
                        if (!isChanging)
                            return;
                        DiscordClient client;
                        try
                        {
                            client = new DiscordClient(token);
                            threads++;
                        }
                        catch (Exception ex) { return; }
                        int tries = 0;
                        proxies_list = Proxy.working_proxies;
                        if (Proxies.paidProxies)
                            proxies_list = Proxy.working_proxies_paid;
                        if (useProxies)
                        {
                            while (tries <= 5)
                            {
                                if (proxies_list.Count > 0)
                                {
                                    proxy = proxies_list[rnd.Next(0, proxies_list.Count)];
                                    if (proxy._ip != "" && proxy != null)
                                    {
                                        HttpProxyClient proxies = new HttpProxyClient(proxy._ip, int.Parse(proxy._port));
                                        if (Proxies.paidProxies)
                                            proxies = new HttpProxyClient(proxy._ip, int.Parse(proxy._port), proxy._username, proxy._password);
                                        client.Proxy = proxies;
                                        while (true)
                                        {
                                            try
                                            {
                                                if (Proxies.paidProxies)
                                                    break;
                                                var buff = client.GetActiveSubscription();
                                                break;
                                            }
                                            catch (Exception ex)
                                            {
                                                try
                                                {
                                                    proxies_list.Remove(proxy);
                                                    if (proxies_list.Count > 0)
                                                    {
                                                        proxy = proxies_list[rnd.Next(0, proxies_list.Count - 1)];
                                                        if (proxy._ip != "" && proxy != null)
                                                        {
                                                            proxies = new HttpProxyClient(proxy._ip, int.Parse(proxy._port));
                                                            client.Proxy = proxies;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Dispatcher.Invoke(() =>
                                                        {
                                                            if (popup != null)
                                                                return;
                                                            popup = new CustomMessageBox("There are no more free proxies available. Try again in a few minutes");
                                                            popup.ShowDialog();
                                                        });
                                                        return;
                                                    }
                                                    break;
                                                }
                                                catch { }
                                            }
                                        }
                                    }
                                    break;
                                }
                                else
                                {
                                    if (Proxy.gettingProxies)
                                    {
                                        proxies_list = Proxy.working_proxies;
                                        if (Proxies.paidProxies)
                                            proxies_list = Proxy.working_proxies_paid;
                                        proxies_list = proxies_list.Except(original_proxies).ToList();
                                        original_proxies = proxies_list;
                                        tries++;
                                        Thread.Sleep(1000);
                                    }
                                    else
                                    {
                                        Dispatcher.Invoke(() =>
                                        {
                                            if (popup != null)
                                                return;
                                            popup = new CustomMessageBox("There are no more free proxies available. Try again in a few minutes");
                                            popup.ShowDialog();
                                        });
                                        return;
                                    }
                                }
                            }
                        }
                        clients.Add(client);
                    });
                    thread_pool.Add(join1);
                    join1.Priority = ThreadPriority.AboveNormal;
                    join1.Start();
                    while (thread_pool.Count >= 5)
                    {
                        foreach (var thread in thread_pool)
                        {
                            if (!thread.IsAlive)
                            {
                                thread_pool.Remove(thread);
                                break;
                            }
                        }
                        Thread.Sleep(500);
                    }
                }
            });
            addClients.Start();
            var changed = 0;
            Thread changer = new Thread(() =>
            {
                int tries = 0;
                while (true)
                {
                    try
                    {
                        using(StreamReader reader = new StreamReader(App.strWorkPath + "/tokens/tokens.txt", Encoding.UTF8))
                        {
                            var line = reader.ReadLine();

                            while (true)
                            {
                                if (line == null)
                                    break;
                                line = line.Trim().Trim('\n').Trim('\t').Trim('\r');
                                if (line == "")
                                    break;
                                tokens_list.Add(line);
                                line = reader.ReadLine();
                            }
                        }
                        Dispatcher.Invoke(() =>
                        {
                            App.mainView.logPrint($"Loaded {tokens_list.Count} tokens. -----");
                        });
                        break;
                    }
                    catch(Exception ex)
                    {
                        tries++;

                        if (tries < 5)
                            continue;
                        isChanging = false;
                        Dispatcher.Invoke(() =>
                        {
                            App.mainView.logPrint($"Couldn't open tokens.txt. The file may be in use by another process, or Tempo itself. Try restarting the application!\n{ex.Message}");
                            Set_Light(isChanging);
                        });
                        return;
                    }
                }
                while (isChanging)
                {
                    if (changed == token_list.Count)
                        isChanging = false;
                    int i = 0;
                    var name = names[rnd.Next(0, names.Count)];
                    try
                    {
                        var clients1 = new DiscordClient[clients.Count];
                        clients.CopyTo(clients1);
                        int cNum = -1;
                        foreach (var client in clients1)
                        {
                            if (changed == token_list.Count)
                                isChanging = false;
                            cNum++;

                            if (!isChanging)
                                break;
                            if (client == null)
                                continue;
                            var username = client.User.Username;
                            name = names[rnd.Next(0, names.Count)];
                            try
                            {
                                var pass = "";
                                bool hasJoined = false;
                                int c = 0;
                                if (tokens_list.Count == 0)
                                    break;
                                for(i = 0; i < tokens_list.Count; i++)
                                {
                                    var arr = tokens_list[i].Split(':');
                                    if(arr[0].Length == 1)
                                    {
                                        if (arr[1] == client.Token)
                                        {
                                            pass = arr[3];
                                            tokens_list.RemoveAt(i);

                                            break;
                                        }
                                        else
                                            continue;
                                    }
                                    else
                                    {
                                        if (arr[0] == client.Token)
                                        {
                                            pass = arr[2];
                                            tokens_list.RemoveAt(i);

                                            break;
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                    }
                                }
                                while (!hasJoined && c < 3)
                                {
                                    client.User.ChangeProfile(new UserProfileUpdate()
                                    {
                                        Username = name,
                                        Password = pass
                                    });
                                    changed++;
                                    hasJoined = true;
                                    clients1[cNum] = null;
                                    clients[cNum] = null;
                                    var discr = client.User.Discriminator.ToString();
                                    for (int d = 0; d < 4 - discr.Length; d++)
                                    {
                                        discr = "0" + discr;
                                    }
                                    username += "#" + discr;
                                    Dispatcher.Invoke(() =>
                                    {
                                        App.mainView.logPrint($"{username} has changed name: ({name}) || {changed} accounts changed their name");
                                    });
                                }
                                if(c >= 3)
                                {
                                    Dispatcher.Invoke(() =>
                                    {
                                        App.mainView.logPrint($"Could not fetch password for {username}, make sure to buy tokens from our shop to make this work!");
                                    });
                                }
                            }
                            catch (Exception ex) { }
                            i++;
                            Thread.Sleep((int)delay);
                        }
                    }
                    catch (InvalidOperationException ex) { Thread.Sleep(500); }
                    Thread.Sleep(1000);
                }
                Dispatcher.Invoke(() => Set_Light(isChanging));
            });
            changer.Priority = ThreadPriority.BelowNormal;
            changer.Start();
        }
        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            StatusLight.Fill = System.Windows.Media.Brushes.Red;
            isChanging = false;
        }
        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)ButtonFile.IsChecked)
            {
                this.ExploreBtn.Visibility = Visibility.Visible;
                isFile = true;
            }
            else
            {
                this.ExploreBtn.Visibility = Visibility.Hidden;
                isFile = false;
            }
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
        private void ExploreBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.Title = "Select your names file";
            dialog.AddToMostRecentlyUsedList = true;
            dialog.EnsureFileExists = true;
            dialog.EnsurePathExists = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                var image = dialog.FileName;

                FileIn.Text = image;
            }
        }
    }
}
