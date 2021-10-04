using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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

namespace TempoWithGUI.MVVM.View
{
    /// <summary>
    /// Interaction logic for tokens.xaml
    /// </summary>
    public partial class tokens : UserControl
    {
        public static string api_key { get; set; }
        public tokens()
        {
            InitializeComponent();
            if (!File.Exists(App.strWorkPath + "\\tokens\\tokens.txt"))
                File.Create(App.strWorkPath + "\\tokens\\tokens.txt");

            SetTokens();
        }
        private void SetTokens()
        {
            var tokens = new List<DiscordToken>();
            for (int i = 0; i < 20; i++)
                tokens.Add(new DiscordToken(false, "token", "T"));
            ListTokens.ItemsSource = tokens;
        }
        private void GetTokens(int n)
        {
            using (StreamWriter stream = new StreamWriter(App.strWorkPath + "\\tokens\\tokens.txt", true))
            {
                string url = "https://unknown-people.it/api/discord";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + api_key);

                for (int i = 0; i < n; i++)
                {
                    string token = "";
                    var response_context = client.SendAsync(new HttpRequestMessage()
                    {
                        Method = new HttpMethod("GET"),
                        RequestUri = new Uri(url)
                    }).GetAwaiter().GetResult();

                    var jtoken = JToken.Parse(response_context.Content.ReadAsStringAsync().Result);
                    var json = JObject.Parse(jtoken.ToString());
                    var tokens = new Tokens();
                    tokens.token = json["0"].Value<string>("token");
                    tokens.email = json["0"].Value<string>("email");
                    tokens.password = json["0"].Value<string>("password");
                    tokens.creation = json["0"].Value<string>("creation");

                    token = tokens.token + ":" + tokens.email + ":" + tokens.password + ":" + tokens.creation;
                    stream.WriteLine(token);
                }
            }
        }
    }
}
