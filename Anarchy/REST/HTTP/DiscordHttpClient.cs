using Newtonsoft.Json.Linq;
using System.Threading;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Net.Http;
using Leaf.xNet;
using System.Net;
using System.Threading.Tasks;
using OpenQA.Selenium.Chrome;
using TempoWithGUI;
using Music_user_bot;
using System.Security.Authentication;

namespace Discord
{
    public class DiscordHttpClient
    {
        private readonly DiscordClient _discordClient;
        public string BaseUrl => DiscordHttpUtil.BuildBaseUrl(_discordClient.Config.ApiVersion, _discordClient.Config.SuperProperties.ReleaseChannel);
        public string _fingerprint { get; set; }
        public DiscordHttpClient(DiscordClient discordClient)
        {
            _discordClient = discordClient;
            /*
            if (!(Settings.Default.Fingerprint != null && Settings.Default.Fingerprint != ""))
                Settings.Default.Fingerprint = GetFingerprint().GetAwaiter().GetResult();
            _fingerprint = Settings.Default.Fingerprint;
            */
        }
        public async Task<string> GetFingerprint()
        {
            HttpClient client = new HttpClient();
            var response_fingerprint = await client.SendAsync(new HttpRequestMessage()
            {
                Method = new System.Net.Http.HttpMethod("GET"),
                RequestUri = new Uri("https://discord.com/api/v9/experiments")
            });
            var resp_fingerprint = new DiscordHttpResponse((int)response_fingerprint.StatusCode, response_fingerprint.Content.ReadAsStringAsync().Result);
            var fingerprint = resp_fingerprint.Body.Value<String>("fingerprint");

            return fingerprint;
        }

        /// <summary>
        /// Sends an HTTP request and checks for errors
        /// </summary>
        /// <param name="method">HTTP method to use</param>
        /// <param name="endpoint">API endpoint (fx. /users/@me)</param>
        /// <param name="payload">JSON content</param>

        private static async void JoinGuildAsync(string token, string invite, DiscordMessage message)
        {
            Task Join = new Task(() =>
            {
                string fun = $"{"\""}\\{"\""} Not A;Brand\\\";v=\\\"99\\\", \\\"Chromium\\\";v=\\\"90\\\", \\\"Google Chrome\\\";v=\\\"90\\\"";
                var options = new ChromeOptions();
                options.AddArgument("headless");
                options.AddArgument("--no-sandbox");
                options.AddArgument("--disable-gpu");
                options.AddArgument("--log-level=3");
                options.AddArgument("--disable-crash-reporter");
                options.AddArgument("--disable-extensions");
                options.AddArgument("--disable-in-process-stack-traces");
                options.AddArgument("--disable-logging");
                options.AddArgument("--disable-dev-shm-usage");
                var driverService = ChromeDriverService.CreateDefaultService();
                driverService.HideCommandPromptWindow = true;
                var driver = new ChromeDriver(driverService, options);

                try
                {
                    string login = "(function() { window.gay = \"" + token + "\"; window.localStorage = document.body.appendChild(document.createElement `iframe`).contentWindow.localStorage; window.setInterval(() => window.localStorage.token = `\"${window.gay}\"`);window.location.reload();})();";
                    driver.Navigate().GoToUrl("https://discord.com/login");
                    driver.ExecuteScript(login);
                    driver.ExecuteScript("fetch(\"https://discord.com/api/v9/invites/" + invite + "\", {\"headers\": { \"accept\": \"*/*\", \"accept-language\": \"en-US\", \"authorization\":\"" + token + "\", \"sec-ch-ua\":" + fun + "\", \"sec-ch-ua-mobile\": \"?0\",    \"sec-fetch-dest\": \"empty\",    \"sec-fetch-mode\": \"cors\",    \"sec-fetch-site\": \"same-origin\", \"x-context-properties\": \"eyJsb2NhdGlvbiI6IkpvaW4gR3VpbGQiLCJsb2NhdGlvbl9ndWlsZF9pZCI6IjgyMDMyODI4NzAxMTQ3MTM5MCIsImxvY2F0aW9uX2NoYW5uZWxfaWQiOiI4MjAzMjgyODcwMzI5NjcyMjkiLCJsb2NhdGlvbl9jaGFubmVsX3R5cGUiOjB9\",\"x-super-properties\": \"eyJvcyI6IldpbmRvd3MiLCJicm93c2VyIjoiQ2hyb21lIiwiZGV2aWNlIjoiIiwic3lzdGVtX2xvY2FsZSI6ImVuLVVTIiwiYnJvd3Nlcl91c2VyX2FnZW50IjoiTW96aWxsYS81LjAgKFdpbmRvd3MgTlQgMTAuMDsgV2luNjQ7IHg2NCkgQXBwbGVXZWJLaXQvNTM3LjM2IChLSFRNTCwgbGlrZSBHZWNrbykgQ2hyb21lLzkwLjAuNDQzMC44NSBTYWZhcmkvNTM3LjM2IiwiYnJvd3Nlcl92ZXJzaW9uIjoiOTAuMC40NDMwLjg1Iiwib3NfdmVyc2lvbiI6IjEwIiwicmVmZXJyZXIiOiJodHRwczovL3JlcGwuaXQvIiwicmVmZXJyaW5nX2RvbWFpbiI6InJlcGwuaXQiLCJyZWZlcnJlcl9jdXJyZW50IjoiIiwicmVmZXJyaW5nX2RvbWFpbl9jdXJyZW50IjoiIiwicmVsZWFzZV9jaGFubmVsIjoic3RhYmxlIiwiY2xpZW50X2J1aWxkX251bWJlciI6ODMwNDAsImNsaWVudF9ldmVudF9zb3VyY2UiOm51bGx9\"},  \"referrer\": \"https://discord.com/channels/@me\",  \"referrerPolicy\": \"strict-origin-when-cross-origin\",\"body\": null,\"method\": \"POST\",\"mode\": \"cors\",\"credentials\": \"include\"});");
                    //App.SendMessage(message, "Joined the server!");
                }
                catch (Exception ex) { throw new Exception(ex.Message, ex); }//App.SendMessage(message, "Could not join the server"); 
                driver.Close();
                driver.Dispose();
            });
            Join.Start();
        }

        public static void JoinGuild(string token, string invite, DiscordMessage message = null)
        {
            string fun = $"{"\""}\\{"\""} Not A;Brand\\\";v=\\\"99\\\", \\\"Chromium\\\";v=\\\"90\\\", \\\"Google Chrome\\\";v=\\\"90\\\"";
            var options = new ChromeOptions();
            options.AddArgument("headless");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--log-level=3");
            options.AddArgument("--disable-crash-reporter");
            options.AddArgument("--disable-extensions");
            options.AddArgument("--disable-in-process-stack-traces");
            options.AddArgument("--disable-logging");
            options.AddArgument("--disable-dev-shm-usage");
            var driverService = ChromeDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            var driver = new ChromeDriver(driverService, options);
            try
            {
                string login = "(function() { window.gay = \"" + token + "\"; window.localStorage = document.body.appendChild(document.createElement `iframe`).contentWindow.localStorage; window.setInterval(() => window.localStorage.token = `\"${window.gay}\"`);window.location.reload();})();";
                driver.Navigate().GoToUrl("https://discord.com/login");
                driver.ExecuteScript(login);
                driver.ExecuteScript("fetch(\"https://discord.com/api/v9/invites/" + invite + "\", {\"headers\": { \"accept\": \"*/*\", \"accept-language\": \"en-US\", \"authorization\":\"" + token + "\", \"sec-ch-ua\":" + fun + "\", \"sec-ch-ua-mobile\": \"?0\",    \"sec-fetch-dest\": \"empty\",    \"sec-fetch-mode\": \"cors\",    \"sec-fetch-site\": \"same-origin\", \"x-context-properties\": \"eyJsb2NhdGlvbiI6IkpvaW4gR3VpbGQiLCJsb2NhdGlvbl9ndWlsZF9pZCI6IjgyMDMyODI4NzAxMTQ3MTM5MCIsImxvY2F0aW9uX2NoYW5uZWxfaWQiOiI4MjAzMjgyODcwMzI5NjcyMjkiLCJsb2NhdGlvbl9jaGFubmVsX3R5cGUiOjB9\",\"x-super-properties\": \"eyJvcyI6IldpbmRvd3MiLCJicm93c2VyIjoiQ2hyb21lIiwiZGV2aWNlIjoiIiwic3lzdGVtX2xvY2FsZSI6ImVuLVVTIiwiYnJvd3Nlcl91c2VyX2FnZW50IjoiTW96aWxsYS81LjAgKFdpbmRvd3MgTlQgMTAuMDsgV2luNjQ7IHg2NCkgQXBwbGVXZWJLaXQvNTM3LjM2IChLSFRNTCwgbGlrZSBHZWNrbykgQ2hyb21lLzkwLjAuNDQzMC44NSBTYWZhcmkvNTM3LjM2IiwiYnJvd3Nlcl92ZXJzaW9uIjoiOTAuMC40NDMwLjg1Iiwib3NfdmVyc2lvbiI6IjEwIiwicmVmZXJyZXIiOiJodHRwczovL3JlcGwuaXQvIiwicmVmZXJyaW5nX2RvbWFpbiI6InJlcGwuaXQiLCJyZWZlcnJlcl9jdXJyZW50IjoiIiwicmVmZXJyaW5nX2RvbWFpbl9jdXJyZW50IjoiIiwicmVsZWFzZV9jaGFubmVsIjoic3RhYmxlIiwiY2xpZW50X2J1aWxkX251bWJlciI6ODMwNDAsImNsaWVudF9ldmVudF9zb3VyY2UiOm51bGx9\"},  \"referrer\": \"https://discord.com/channels/@me\",  \"referrerPolicy\": \"strict-origin-when-cross-origin\",\"body\": null,\"method\": \"POST\",\"mode\": \"cors\",\"credentials\": \"include\"});");
                //App.SendMessage(message, "Joined the server!");
            }
            catch (Exception ex) { throw new Exception(ex.Message, ex); 
                if(message != null)
                    App.SendMessage(message, "Selenium exception."); }
            driver.Close();
            driver.Dispose();
        }

        private async Task<DiscordHttpResponse> SendAsync(Leaf.xNet.HttpMethod method, string endpoint, object payload = null)
        {
            if (!endpoint.StartsWith("https"))
                endpoint = DiscordHttpUtil.BuildBaseUrl(_discordClient.Config.ApiVersion, _discordClient.Config.SuperProperties.ReleaseChannel) + endpoint;

            string json = "{}";
            if (payload != null)
            {
                if (payload.GetType() == typeof(string))
                    json = (string)payload;
                else
                    json = JsonConvert.SerializeObject(payload);
            }

            uint retriesLeft = _discordClient.Config.RestConnectionRetries;
            bool hasData = method == Leaf.xNet.HttpMethod.POST || method == Leaf.xNet.HttpMethod.PATCH || method == Leaf.xNet.HttpMethod.PUT || method == Leaf.xNet.HttpMethod.DELETE;

            while (true)
            {
                try
                {
                    DiscordHttpResponse resp;
                    if (method == Leaf.xNet.HttpMethod.POST && endpoint.Contains("/invites/"))
                    {
                        HttpRequest request = new HttpRequest()
                        {
                            KeepTemporaryHeadersOnRedirect = false,
                            EnableMiddleHeaders = false,
                            AllowEmptyHeaderValues = false
                        };


                        if (_discordClient.Proxy != null)
                            request.Proxy = _discordClient.Proxy;

                        request.ClearAllHeaders();
                        request.AddHeader("Accept", "*/*");
                        request.AddHeader("Accept-Encoding", "gzip, deflate");
                        request.AddHeader("Accept-Language", "en-US");
                        request.AddHeader("Authorization", _discordClient.Token);
                        request.AddHeader("Connection", "keep-alive");
                        request.AddHeader("Cookie", "__cfduid=db537515176b9800b51d3de7330fc27d61618084707; __dcfduid=ec27126ae8e351eb9f5865035b40b75d; locale=en-US");
                        request.AddHeader("DNT", "1");
                        request.AddHeader("origin", "https://discord.com");
                        request.AddHeader("Referer", "https://discord.com/channels/@me");
                        request.AddHeader("TE", "Trailers");
                        request.AddHeader("User-Agent", _discordClient.Config.SuperProperties.UserAgent);
                        request.AddHeader("X-Super-Properties", _discordClient.Config.SuperProperties.ToBase64());

                        var response = request.Post(endpoint);

                        resp = new DiscordHttpResponse((int)response.StatusCode, response.ToString());
                    }
                    else if (_discordClient.Proxy == null || _discordClient.Proxy.Type == ProxyType.HTTP)
                    {
                        HttpClient client = new HttpClient(new HttpClientHandler() { Proxy = _discordClient.Proxy == null ? null : new WebProxy(_discordClient.Proxy.Host, _discordClient.Proxy.Port) });
                        var token = _discordClient.Token;

                        if (_discordClient.Token != null)
                            client.DefaultRequestHeaders.Add("Authorization", _discordClient.Token);

                        if (_discordClient.Token.StartsWith("Bot ") || (_discordClient.User != null && _discordClient.User.Type == DiscordUserType.Bot))
                            client.DefaultRequestHeaders.Add("User-Agent", "Anarchy/0.8.1.0");
                        else
                        {
                            client.DefaultRequestHeaders.Add("User-Agent", _discordClient.Config.SuperProperties.UserAgent);
                            client.DefaultRequestHeaders.Add("X-Super-Properties", _discordClient.Config.SuperProperties.ToBase64());
                        }

                        var response = client.SendAsync(new HttpRequestMessage()
                        {
                            Content = hasData ? new System.Net.Http.StringContent(json, Encoding.UTF8, "application/json") : null,
                            Method = new System.Net.Http.HttpMethod(method.ToString()),
                            RequestUri = new Uri(endpoint)
                        }).GetAwaiter().GetResult();

                        resp = new DiscordHttpResponse((int)response.StatusCode, response.Content.ReadAsStringAsync().Result);
                    }
                    else
                    {
                        HttpRequest msg = new HttpRequest
                        {
                            IgnoreProtocolErrors = true,
                            UserAgent = _discordClient.User != null && _discordClient.User.Type == DiscordUserType.Bot ? "Anarchy/0.8.1.0" : _discordClient.Config.SuperProperties.UserAgent,
                            Authorization = _discordClient.Token
                        };

                        if (hasData)
                            msg.AddHeader(HttpHeader.ContentType, "application/json");

                        if (_discordClient.User == null || _discordClient.User.Type == DiscordUserType.User) msg.AddHeader("X-Super-Properties", _discordClient.Config.SuperProperties.ToBase64());
                        if (_discordClient.Proxy != null) msg.Proxy = _discordClient.Proxy;

                        var response = msg.Raw(method, endpoint, hasData ? new Leaf.xNet.StringContent(json) : null);

                        resp = new DiscordHttpResponse((int)response.StatusCode, response.ToString());
                    }

                    DiscordHttpUtil.ValidateResponse(resp.StatusCode, resp.Body);
                    return resp;
                }
                catch (Exception ex) when (ex is HttpException || ex is HttpRequestException || ex is TaskCanceledException)
                {
                    if (retriesLeft == 0)
                        throw new DiscordConnectionException();

                    retriesLeft--;
                }
                catch (RateLimitException ex)
                {
                    if (_discordClient.Config.RetryOnRateLimit)
                        Thread.Sleep(ex.RetryAfter);
                    else
                        throw;
                }
            }
        }

        private async Task<DiscordHttpResponse> SendAsyncJoin(Leaf.xNet.HttpMethod method, string endpoint, object payload = null)
        {
            if (!endpoint.StartsWith("https"))
                endpoint = DiscordHttpUtil.BuildBaseUrl(_discordClient.Config.ApiVersion, _discordClient.Config.SuperProperties.ReleaseChannel) + endpoint;
            string inv_code = "https://discord.com/api/v9/invites/";
            if (endpoint.Contains("/invites/"))
                inv_code = endpoint.Substring(inv_code.Length);

            string json = "{}";
            if (payload != null)
            {
                if (payload.GetType() == typeof(string))
                    json = (string)payload;
                else
                    json = JsonConvert.SerializeObject(payload);
            }

            uint retriesLeft = _discordClient.Config.RestConnectionRetries;
            bool hasData = method == Leaf.xNet.HttpMethod.POST || method == Leaf.xNet.HttpMethod.PATCH || method == Leaf.xNet.HttpMethod.PUT || method == Leaf.xNet.HttpMethod.DELETE;

            while (true)
            {
                try
                {
                    DiscordHttpResponse resp;

                    if (_discordClient.Proxy == null || _discordClient.Proxy.Type == ProxyType.HTTP)
                    {
                        HttpClient client = new HttpClient(new HttpClientHandler() { Proxy = _discordClient.Proxy == null ? null : new WebProxy(_discordClient.Proxy.Host, _discordClient.Proxy.Port) });
                        var context = ContextProperties.GetContextProperties(inv_code).GetAwaiter().GetResult();

                        if (_discordClient.Token != null)
                            client.DefaultRequestHeaders.Add("authorization", _discordClient.Token);
                        if (_discordClient.User != null && _discordClient.User.Type == DiscordUserType.Bot)
                            client.DefaultRequestHeaders.Add("User-Agent", "Anarchy/0.8.1.0");
                        else
                        {
                            client.DefaultRequestHeaders.Add("accept", "*/*");
                            client.DefaultRequestHeaders.Add("accept-language", "it");
                            client.DefaultRequestHeaders.Add("origin", "https://discord.com");
                            client.DefaultRequestHeaders.Add("referer", "https://discord.com/channels/@me");
                            client.DefaultRequestHeaders.Add("sec-fetch-dest", "empty");
                            client.DefaultRequestHeaders.Add("sec-fetch-mode", "cors");
                            client.DefaultRequestHeaders.Add("sec-fetch-site", "same-origin");
                            client.DefaultRequestHeaders.Add("sec-gpc", "1");
                            client.DefaultRequestHeaders.Add("User-Agent", _discordClient.Config.SuperProperties.UserAgent);
                            client.DefaultRequestHeaders.Add("X-Context-Properties", context);
                            client.DefaultRequestHeaders.Add("X-Debug-Options", "bugReporterEnabled");
                            client.DefaultRequestHeaders.Add("X-Fingerprint", _fingerprint);
                            client.DefaultRequestHeaders.Add("X-Super-Properties", _discordClient.Config.SuperProperties.ToBase64());
                        }

                        var response = await client.SendAsync(new HttpRequestMessage()
                        {
                            Content = hasData ? new System.Net.Http.StringContent(json, Encoding.UTF8, "application/json") : null,
                            Method = new System.Net.Http.HttpMethod(method.ToString()),
                            RequestUri = new Uri(endpoint)
                        });
                        client.DefaultRequestHeaders.Clear();

                        resp = new DiscordHttpResponse((int)response.StatusCode, response.Content.ReadAsStringAsync().Result);
                    }
                    else
                    {
                        HttpRequest msg = new HttpRequest
                        {
                            IgnoreProtocolErrors = true,
                            UserAgent = _discordClient.User != null && _discordClient.User.Type == DiscordUserType.Bot ? "Anarchy/0.8.1.0" : _discordClient.Config.SuperProperties.UserAgent,
                            Authorization = _discordClient.Token
                        };

                        if (hasData)
                            msg.AddHeader(HttpHeader.ContentType, "application/json");

                        if (_discordClient.User == null || _discordClient.User.Type == DiscordUserType.User) msg.AddHeader("X-Super-Properties", _discordClient.Config.SuperProperties.ToBase64());
                        if (_discordClient.Proxy != null) msg.Proxy = _discordClient.Proxy;

                        var response = msg.Raw(method, endpoint, hasData ? new Leaf.xNet.StringContent(json) : null);

                        resp = new DiscordHttpResponse((int)response.StatusCode, response.ToString());
                    }

                    DiscordHttpUtil.ValidateResponse(resp.StatusCode, resp.Body);
                    return resp;
                }
                catch (Exception ex) when (ex is HttpException || ex is HttpRequestException || ex is TaskCanceledException)
                {
                    if (retriesLeft == 0)
                        throw new DiscordConnectionException();

                    retriesLeft--;
                }
                catch (RateLimitException ex)
                {
                    if (_discordClient.Config.RetryOnRateLimit)
                        Thread.Sleep(ex.RetryAfter);
                    else
                        throw;
                }
            }
        }


        public async Task<DiscordHttpResponse> GetAsync(string endpoint)
        {
            return await SendAsync(Leaf.xNet.HttpMethod.GET, endpoint);
        }


        public async Task<DiscordHttpResponse> PostAsync(string endpoint, object payload = null)
        {
            return await SendAsync(Leaf.xNet.HttpMethod.POST, endpoint, payload);
        }

        public async Task<DiscordHttpResponse> PostAsyncJoin(string endpoint, object payload = null)
        {
            return await SendAsyncJoin(Leaf.xNet.HttpMethod.POST, endpoint, payload);
        }


        public async Task<DiscordHttpResponse> DeleteAsync(string endpoint, object payload = null)
        {
            return await SendAsync(Leaf.xNet.HttpMethod.DELETE, endpoint, payload);
        }


        public async Task<DiscordHttpResponse> PutAsync(string endpoint, object payload = null)
        {
            return await SendAsync(Leaf.xNet.HttpMethod.PUT, endpoint, payload);
        }


        public async Task<DiscordHttpResponse> PatchAsync(string endpoint, object payload = null)
        {
            return await SendAsync(Leaf.xNet.HttpMethod.PATCH, endpoint, payload);
        }
    }
    public class Fingerprint
    {
        [JsonProperty("assignments")]
        public Array assignments { get; set; }
        [JsonProperty("fingerprint")]
        public string fingerprint { get; set; }
    }
}