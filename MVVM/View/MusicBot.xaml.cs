﻿using Discord;
using Discord.Gateway;
using Music_user_bot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

namespace TempoWithGUI.MVVM.View
{
    /// <summary>
    /// Interaction logic for HomeView.xaml
    /// </summary>
    public partial class MusicBotView : UserControl
    {
        public bool isBot { get; set; } = Settings.Default.isBot;
        public static bool isLoggedIn { get; set; } = false;
        public MusicBotView()
        {
            InitializeComponent();
            Debug.Log("Opened MusicBot control");
            if (Settings.Default.Username != "")
                this.UsernameIn.Text = Settings.Default.Username;
            if (Settings.Default.Password != "")
                this.PasswordIn.Text = Settings.Default.Password;
            if (Settings.Default.Token != "")
                this.TokenIn.Text = Settings.Default.Token;
            if (Settings.Default.OwnerId != 0)
                this.OwnerIn.Text = Settings.Default.OwnerId.ToString();
            if (Settings.Default.Prefix != "")
                this.PrefixIn.Text = Settings.Default.Prefix;
            if (Settings.Default.Image != "")
                this.ImageIn.Text = Settings.Default.Image;
            if (Settings.Default.Dj_role.ToString() != "0" && Settings.Default.Dj_role.ToString() != "")
                this.DjRoleIn.Text = Settings.Default.Dj_role.ToString();

            if (isBot)
            {
                this.WhitelistIn.Visibility = Visibility.Collapsed;
                this.AdminsIn.Visibility = Visibility.Collapsed;
                this.WhitelistLabel.Visibility = Visibility.Collapsed;
                this.AdminsLabel.Visibility = Visibility.Collapsed;
                this.PasswordIn.Visibility = Visibility.Collapsed;

                this.DjRoleLabel.Visibility = Visibility.Visible;
                this.DjRoleIn.Visibility = Visibility.Visible;
                this.ButtonUser.IsChecked = false;
                this.ButtonBot.IsChecked = true;
            }
            else
            {
                this.WhitelistIn.Visibility = Visibility.Visible;
                this.AdminsIn.Visibility = Visibility.Visible;
                this.WhitelistLabel.Visibility = Visibility.Visible;
                this.AdminsLabel.Visibility = Visibility.Visible;
                this.PasswordIn.Visibility = Visibility.Visible;

                this.DjRoleIn.Visibility = Visibility.Collapsed;
                this.DjRoleLabel.Visibility = Visibility.Collapsed;
                this.ButtonBot.IsChecked = false;
                this.ButtonUser.IsChecked = true;
            }
            if (!App.ask_settings)
            {
                Start();
            }
            if (isLoggedIn)
            {
                StatusLight.Fill = Brushes.Green;
                StartBtn.Content = "LOGOUT";
            }
            else
            {
                StatusLight.Fill = Brushes.Red;
                StartBtn.Content = "START";
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Start();
        }
        public void Start()
        {
            if (isLoggedIn)
            {
                if(TrackQueue.currentSong != null)
                    TrackQueue.currentSong.CancellationTokenSource.Cancel();
                App.mainClient.Logout();
                isLoggedIn = false;
                StatusLight.Fill = Brushes.Red;
                StartBtn.Content = "START";
                return;
            }
            this.StartBtn.Cursor = Cursors.AppStarting;
            if (UsernameIn.Text != "" && PasswordIn.Text != "")
            {
                Settings.Default.Username = UsernameIn.Text;
                Settings.Default.Password = PasswordIn.Text;
            }
            else
            {
                MessageBox.Show("Please insert your bot's password and choose a username");
            }
            if (TokenIn.Text != "" && PrefixIn.Text != "" && OwnerIn.Text != "")
            {
                Settings.Default.Token = TokenIn.Text;
                Settings.Default.Prefix = PrefixIn.Text;
                if (ulong.TryParse(OwnerIn.Text, out var ownerId))
                    Settings.Default.OwnerId = ownerId;
                else
                {
                    MessageBox.Show("Please insert a valid owner ID\nEnable Discord's developer mode to see IDs!");
                    return;
                }
                if (ImageIn.Text != "" && File.Exists(ImageIn.Text) && (ImageIn.Text.EndsWith(".png") || ImageIn.Text.EndsWith(".jpg")))
                {
                    Settings.Default.Image = ImageIn.Text;
                }
                else
                {
                    Settings.Default.Image = App.strWorkPath + "\\propic.png";
                }
            }
            else
            {
                MessageBox.Show("Please fill all the required input before starting Tempo");
                return;
            }
            if (Settings.Default.WhiteList == null)
            {
                Whitelist.white_list = new System.Collections.Specialized.StringCollection();
            }
            else
            {
                Whitelist.white_list = Settings.Default.WhiteList;
            }
            if (Settings.Default.Admins == null)
            {
                Admin.admins = new System.Collections.Specialized.StringCollection();
            }
            else
            {
                Admin.admins = Settings.Default.Admins;
            }
            if (!Settings.Default.isBot)
            {
                if (WhitelistIn.Text != "")
                {
                    foreach (string id in WhitelistIn.Text.Split('\n'))
                    {
                        if (ulong.TryParse(id, out var user_id))
                        {
                            bool inWL = false;
                            foreach (var ID in Whitelist.white_list)
                            {
                                if (id == ID)
                                {
                                    inWL = true;
                                    break;
                                }
                            }
                            if (!inWL)
                                Whitelist.AddToWL(user_id);
                        }
                        else
                        {
                            MessageBox.Show("There was an error with the whitelist");
                            return;
                        }
                    }
                }
                if (AdminsIn.Text != "")
                {
                    foreach (string id in AdminsIn.Text.Split('\n'))
                    {
                        if (ulong.TryParse(id, out var user_id))
                        {
                            bool inAD = false;
                            foreach (var ID in Admin.admins)
                            {
                                if (id == ID)
                                {
                                    inAD = true;
                                    break;
                                }
                            }
                            if (!inAD)
                                Admin.AddToAl(user_id);
                        }
                        else
                        {
                            MessageBox.Show("There was an error with the admin list");
                            return;
                        }
                    }
                }
            }
            else
            {

            }
            var random = new string[] { };
            App.botToken = "";
            if (Settings.Default.isBot)
            {
                App.botToken += "Bot ";
                ulong djrole = 0;
                if(ulong.TryParse(this.DjRoleIn.Text, out djrole))
                {
                    Settings.Default.Dj_role = djrole;
                }
            }

            App.botToken += Settings.Default.Token;
            Whitelist.ownerID = Settings.Default.OwnerId;
            if (!Settings.Default.isBot)
            {
                DiscordClient clientNew = new DiscordClient(App.botToken);

                string discriminator = "";
                for (int i = 0; i < 4 - clientNew.GetUser(Whitelist.ownerID).Discriminator.ToString().Length; i++)
                {
                    discriminator += "0";
                }
                discriminator += clientNew.GetUser(Whitelist.ownerID).Discriminator;
                App.ownerName = clientNew.GetUser(Whitelist.ownerID).Username + "#" + discriminator;
            }

            uint apiVersion = 9;
            if (Settings.Default.isBot)
                apiVersion = 8;
            DiscordSocketClient client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                HandleIncomingMediaData = false,
                Intents = DiscordGatewayIntent.Guilds | DiscordGatewayIntent.GuildMessages | DiscordGatewayIntent.GuildVoiceStates,
                ApiVersion = apiVersion
            });

            client.CreateCommandHandler(Settings.Default.Prefix);
            client.OnLoggedIn += App.Client_OnLoggedIn;
            client.OnJoinedVoiceChannel += App.Client_OnJoinedVoiceChannel;
            client.OnLeftVoiceChannel += App.Client_OnLeftVoiceChannel;
            client.Login(App.botToken);
            App.mainClient = client;

            Settings.Default.Save();
            Settings.Default.Reload();
            App.SaveSettings();
            isLoggedIn = true;
            this.StartBtn.Content = "LOGOUT";
            this.StartBtn.Cursor = Cursors.Arrow;
            this.StatusLight.Fill = Brushes.Green;
        }
        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.isBot = true;
            isBot = true;
            Settings.Default.Save();
            this.WhitelistIn.IsEnabled = false;
            this.AdminsIn.IsEnabled = false;

            this.PasswordIn.Visibility = Visibility.Collapsed;
            this.WhitelistIn.Visibility = Visibility.Collapsed;
            this.AdminsIn.Visibility = Visibility.Collapsed;
            this.WhitelistLabel.Visibility = Visibility.Collapsed;
            this.AdminsLabel.Visibility = Visibility.Collapsed;

            this.DjRoleIn.Visibility = Visibility.Visible;
            this.DjRoleLabel.Visibility = Visibility.Visible;
        }

        private void RadioButton_Click_1(object sender, RoutedEventArgs e)
        {
            Settings.Default.isBot = false;
            isBot = false;
            Settings.Default.Save();
            this.WhitelistIn.IsEnabled = true;
            this.AdminsIn.IsEnabled = true;

            this.PasswordIn.Visibility = Visibility.Visible;
            this.WhitelistIn.Visibility = Visibility.Visible;
            this.AdminsIn.Visibility = Visibility.Visible;
            this.WhitelistLabel.Visibility = Visibility.Visible;
            this.AdminsLabel.Visibility = Visibility.Visible;

            this.DjRoleIn.Visibility = Visibility.Collapsed;
            this.DjRoleLabel.Visibility = Visibility.Collapsed;
        }

        /*
private void UsernameIn_TextChanged(object sender, TextChangedEventArgs e)
{
TextChange textChange = e.Changes.AsEnumerable().FirstOrDefault();
if(textChange.AddedLength > 0 )
Settings.Default.Username += e.OriginalSource.ToString().Split(' ')[1];
else if (textChange.RemovedLength > 0)
Settings.Default.Username = Settings.Default.Username.TrimEnd(e.OriginalSource.ToString().Split(' ')[1].ToCharArray());
}

private void PasswordIn_TextChanged(object sender, TextChangedEventArgs e)
{
TextChange textChange = e.Changes.AsEnumerable().FirstOrDefault();
if (textChange.AddedLength > 0)
Settings.Default.Password += e.OriginalSource.ToString().Split(' ')[1];
else if (textChange.RemovedLength > 0)
Settings.Default.Password = Settings.Default.Password.TrimEnd(e.OriginalSource.ToString().Split(' ')[1].ToCharArray());
}

private void TokenIn_TextChanged(object sender, TextChangedEventArgs e)
{
TextChange textChange = e.Changes.AsEnumerable().FirstOrDefault();
if (textChange.AddedLength > 0)
Settings.Default.Token += e.OriginalSource.ToString().Split(' ')[1];
else if (textChange.RemovedLength > 0)
Settings.Default.Token = Settings.Default.Token.TrimEnd(e.OriginalSource.ToString().Split(' ')[1].ToCharArray());
}
private void PrefixIn_TextChanged(object sender, TextChangedEventArgs e)
{
TextChange textChange = e.Changes.AsEnumerable().FirstOrDefault();
if (textChange.AddedLength > 0)
Settings.Default.Prefix += e.OriginalSource.ToString().Split(' ')[1];
else if (textChange.RemovedLength > 0)
Settings.Default.Prefix = Settings.Default.Prefix.TrimEnd(e.OriginalSource.ToString().Split(' ')[1].ToCharArray());
}
private void OwnerIn_TextChanged(object sender, TextChangedEventArgs e)
{
TextChange textChange = e.Changes.AsEnumerable().FirstOrDefault();
try
{
if (textChange.AddedLength > 0)
  Settings.Default.OwnerId += ulong.Parse(e.OriginalSource.ToString().Split(' ')[1]);
else if (textChange.RemovedLength > 0)
  Settings.Default.OwnerId = ulong.Parse((Settings.Default.OwnerId).ToString().TrimEnd(e.OriginalSource.ToString().Split(' ')[1].ToCharArray()));
}
catch
{

}
}
private void ImageIn_TextChanged(object sender, TextChangedEventArgs e)
{
TextChange textChange = e.Changes.AsEnumerable().FirstOrDefault();
if (textChange.AddedLength > 0)
Settings.Default.Image += e.OriginalSource.ToString().Split(' ')[1];
else if (textChange.RemovedLength > 0)
Settings.Default.Image = Settings.Default.Image.TrimEnd(e.OriginalSource.ToString().Split(' ')[1].ToCharArray());
}

private void UsernameIn_TextInput(object sender, TextCompositionEventArgs e)
{
var x = 0;
}
*/
    }
}
