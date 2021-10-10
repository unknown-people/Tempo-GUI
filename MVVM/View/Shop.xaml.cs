using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Windows;
using System.Windows.Input;

namespace TempoWithGUI.MVVM.View
{
    /// <summary>
    /// Interaction logic for Shop.xaml
    /// </summary>
    public partial class Shop : Window
    {
        public Shop()
        {
            InitializeComponent();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        public static int GetCurrentCoins(string api_key)
        {
            string url = "https://unknown-people.it/api/accounts";
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + App.api_key);
            var response = client.SendAsync(new HttpRequestMessage()
            {
                Method = new HttpMethod("GET"),
                RequestUri = new Uri(url)
            }).GetAwaiter().GetResult();
            if (response.StatusCode.ToString() == "BadRequest")
                return 0;
            var jtoken = JToken.Parse(response.Content.ReadAsStringAsync().Result);
            var json = JObject.Parse(jtoken.ToString());
            return json.Value<int>("credit");
        }
    }
}
