using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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

namespace TempoWithGUI.MVVM.View
{
    /// <summary>
    /// Interaction logic for Accounts.xaml
    /// </summary>
    public partial class Accounts : UserControl
    {
        public static int credit { get; set; }
        public Accounts()
        {
            InitializeComponent();
            credit = Shop.GetCurrentCoins(App.api_key);
            if(credit != 0)
                CurrentCredit.Text = credit.ToString();
        }   

        private void Buy_Click(object sender, RoutedEventArgs e)
        {
            int cost = GetTokens(int.Parse(QuantityIn.Text));
            int cc = int.Parse(CurrentCredit.Text);
            int nc = cc - cost;
            CurrentCredit.Text = nc.ToString();
            tokens.boughtTokens = true;
        }

        private int GetTokens(int n = 1)
        {
            int cost = 0;
            while (true)
            {
                try
                {
                    using (StreamWriter stream = new StreamWriter(App.strWorkPath + "\\tokens\\tokens.txt", true))
                    {
                        var q = n.ToString();
                        string url = "https://unknown-people.it/api/discord?quantity=" + q;
                        if ((bool)ButtonMail.IsChecked)
                            url += "&onlymail=true";
                        HttpClient client = new HttpClient();
                        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + App.api_key);
                        string token = "";
                        var response_context = client.SendAsync(new HttpRequestMessage()
                        {
                            Method = new HttpMethod("GET"),
                            RequestUri = new Uri(url)
                        }).GetAwaiter().GetResult();
                        for (int i = 0; i < n; i++)
                        {
                            try
                            {
                                var jtoken = JToken.Parse(response_context.Content.ReadAsStringAsync().Result);
                                var json = JObject.Parse(jtoken.ToString());
                                var tokens = new Tokens();
                                tokens.token = json[i.ToString()].Value<string>("token");
                                tokens.email = json[i.ToString()].Value<string>("email");
                                tokens.password = json[i.ToString()].Value<string>("password");
                                tokens.creation = json[i.ToString()].Value<string>("creation");
                                if ((bool)ButtonFull.IsChecked)
                                    tokens.country = json[i.ToString()].Value<string>("country");
                                else
                                    tokens.country = "NULL";
                                cost = json.Value<int>("cost");

                                token = "T:" + tokens.token + ":" + tokens.email + ":" + tokens.password + ":" + tokens.creation + ":" + tokens.country;
                                stream.WriteLine(token);
                            }
                            catch
                            {
                                MessageBox.Show("You do not seem to have enough credit for this transaction");
                                return 0;
                            }
                        }
                    }
                    return cost;
                }
                catch (InvalidOperationException) { Thread.Sleep(1000); }
            }
        }

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
