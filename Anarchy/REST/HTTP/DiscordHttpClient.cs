﻿using Newtonsoft.Json.Linq;
using System.Threading;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using Leaf.xNet;
using System.Net;
using System.Threading.Tasks;
using OpenQA.Selenium.Chrome;
using TempoWithGUI;
using Music_user_bot;
using System.Security.Authentication;
using System.IO;

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

        private async Task<DiscordHttpResponse> SendAsync(Leaf.xNet.HttpMethod method, string endpoint, object payload = null, ulong? guildId = null, ulong? channelId = null)
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
                    DiscordHttpResponse resp = null;

                    if (method == Leaf.xNet.HttpMethod.POST && (endpoint.Contains("/invites/") || endpoint == "https://discord.com/api/v9/users/@me/channels"))
                    {
                        HttpRequest request = new HttpRequest()
                        {
                            KeepTemporaryHeadersOnRedirect = false,
                            EnableMiddleHeaders = false,
                            AllowEmptyHeaderValues = false
                            //SslProtocols = SslProtocols.Tls12
                        };
                        request.Proxy = _discordClient.Proxy;
                        /*
                        request.Proxy = new HttpProxyClient(_discordClient.Proxy.Host, _discordClient.Proxy.Port);
                        if (_discordClient.Proxy.Username != null && _discordClient.Proxy.Username != "")
                            request.Proxy = new HttpProxyClient(_discordClient.Proxy.Host, _discordClient.Proxy.Port, _discordClient.Proxy.Username, _discordClient.Proxy.Password);
                        */
                        request.ClearAllHeaders();
                        request.AddHeader("Accept", "*/*");
                        request.AddHeader("Accept-Encoding", "gzip, deflate");
                        request.AddHeader("Accept-Language", "it");
                        request.AddHeader("Authorization", _discordClient.Token);
                        request.AddHeader("Connection", "keep-alive");
                        request.AddHeader("Cookie", "__cfduid=db537515176b9800b51d3de7330fc27d61618084707; __dcfduid=ec27126ae8e351eb9f5865035b40b75d");
                        request.AddHeader("DNT", "1");
                        request.AddHeader("origin", "https://discord.com");
                        request.AddHeader("Referer", "https://discord.com/channels/@me");
                        request.AddHeader("TE", "Trailers");
                        request.AddHeader("User-Agent", _discordClient.Config.SuperProperties.UserAgent);
                        request.AddHeader("X-Super-Properties", _discordClient.Config.SuperProperties.ToBase64());

                        HttpResponse response;
                        if(endpoint == "https://discord.com/api/v9/users/@me/channels")
                        {
                            request.AddHeader("x-context-properties", "e30=");
                        }
                        else
                        {
                            var context_pr = "{" + $"\"location\":\"Join Guild\",\"location_guild_id\":\"{guildId}\",\"location_channel_id\":\"{channelId}\",\"location_channel_type\":0" + "}";
                            var encoded_pr = Base64Encode(context_pr);
                            request.AddHeader("X-Context-Properties", encoded_pr);
                        }
                        request.AddHeader("Content-Length", ASCIIEncoding.UTF8.GetBytes(json).Length.ToString());
                        response = request.Post(endpoint, json, "application/json");
                        resp = new DiscordHttpResponse((int)response.StatusCode, response.ToString());
                    }
                    else if(method == Leaf.xNet.HttpMethod.PATCH && endpoint == "https://discord.com/api/v9/users/@me")
                    {
                        if (!json.Contains("avatar"))
                        {
                            var json_arr = json.Split(',');
                            json = json_arr[0] + "," + json_arr[3] + "}";
                        }
                        HttpRequest request = new HttpRequest()
                        {
                            KeepTemporaryHeadersOnRedirect = false,
                            EnableMiddleHeaders = false,
                            AllowEmptyHeaderValues = false
                            //SslProtocols = SslProtocols.Tls12
                        };
                        request.Proxy = _discordClient.Proxy;
                        request.ClearAllHeaders();
                        request.AddHeader("Accept", "*/*");
                        request.AddHeader("Accept-Encoding", "gzip, deflate");
                        request.AddHeader("Accept-Language", "it");
                        request.AddHeader("Authorization", _discordClient.Token);
                        request.AddHeader("Connection", "keep-alive");
                        request.AddHeader("Cookie", "__cfduid=db537515176b9800b51d3de7330fc27d61618084707; __dcfduid=ec27126ae8e351eb9f5865035b40b75d");
                        request.AddHeader("DNT", "1");
                        request.AddHeader("origin", "https://discord.com");
                        request.AddHeader("Referer", "https://discord.com/channels/@me");
                        request.AddHeader("TE", "Trailers");
                        request.AddHeader("User-Agent", _discordClient.Config.SuperProperties.UserAgent);
                        request.AddHeader("X-Super-Properties", _discordClient.Config.SuperProperties.ToBase64());
                        request.AddHeader("Content-Length", ASCIIEncoding.UTF8.GetBytes(json).Length.ToString());
                        request.AddHeader("X-Debug-Options", "bugReporterEnabled");
                        if (Fingerprint.fingerprint == null)
                            Fingerprint.GetFingerprint().GetAwaiter().GetResult();
                        request.AddHeader("X-Fingerprint", Fingerprint.fingerprint);
                        var response = request.Patch(endpoint, json, "application/json");
                        resp = new DiscordHttpResponse((int)response.StatusCode, response.ToString());
                    }
                    else if (_discordClient.Proxy == null || _discordClient.Proxy.Type == ProxyType.HTTP)
                    {
                        HttpClient client = new HttpClient(new HttpClientHandler() { Proxy = _discordClient.Proxy == null ? null : new WebProxy(_discordClient.Proxy.Host, _discordClient.Proxy.Port) });
                        if (_discordClient.Proxy != null && _discordClient.Proxy.Username != null)
                        {
                            ICredentials credentials = new NetworkCredential(_discordClient.Proxy.Username, _discordClient.Proxy.Password);
                            var proxyURI = new Uri(string.Format("{0}:{1}", "http://" + _discordClient.Proxy.Host, _discordClient.Proxy.Port));
                            client = new HttpClient(new HttpClientHandler() { Proxy = _discordClient.Proxy == null ? null : new WebProxy(proxyURI, true, null, credentials)});
                        }
                        var token = _discordClient.Token;

                        if (_discordClient.Token != null)
                            client.DefaultRequestHeaders.Add("Authorization", _discordClient.Token);
                        if (_discordClient.Token.StartsWith("Bot ") || (_discordClient.User != null && _discordClient.User.Type == DiscordUserType.Bot))
                            client.DefaultRequestHeaders.Add("User-Agent", "Anarchy/0.8.1.0");
                        else
                        {
                            client.DefaultRequestHeaders.Add("User-Agent", _discordClient.Config.SuperProperties.UserAgent);
                            client.DefaultRequestHeaders.Add("Accept-Language", "it");
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

                        if (endpoint.Contains("https://discord.com/api/v9/channels/") && endpoint.Contains("messages") && method == Leaf.xNet.HttpMethod.POST)
                        {
                            var ep = endpoint.Split('/');
                            ulong guild_id = 0;
                            foreach (string chunk in ep)
                            {
                                if (ulong.TryParse(chunk, out guild_id))
                                {
                                    break;
                                }
                            }
                            msg.AddHeader("origin", "https://discord.com");
                            msg.AddHeader("Accept", "*/*");
                            msg.AddHeader("Cookie", "__cfduid=db537515176b9800b51d3de7330fc27d61618084707; __dcfduid=ec27126ae8e351eb9f5865035b40b75d; locale=it");
                            msg.AddHeader(HttpHeader.Referer, "https://discord.com/channels/@me/" + guild_id);
                        }

                        var response = msg.Raw(method, endpoint, hasData ? new Leaf.xNet.StringContent(json) : null);

                        resp = new DiscordHttpResponse((int)response.StatusCode, response.ToString());
                    }
                    if (endpoint.Contains("member-verification?") && method == Leaf.xNet.HttpMethod.GET)
                    {
                        var ep = endpoint.Split('/');
                        ulong guild_id = 0;
                        foreach(string chunk in ep)
                        {
                            if(ulong.TryParse(chunk, out guild_id))
                            {
                                break;
                            }
                        }
                        endpoint = $"/guilds/{guild_id}/requests/@me";
                        if (!endpoint.StartsWith("https"))
                            endpoint = DiscordHttpUtil.BuildBaseUrl(_discordClient.Config.ApiVersion, _discordClient.Config.SuperProperties.ReleaseChannel) + endpoint;

                        HttpRequest request = new HttpRequest()
                        {
                            KeepTemporaryHeadersOnRedirect = false,
                            EnableMiddleHeaders = false,
                            AllowEmptyHeaderValues = false
                        };

                        request.Proxy = _discordClient.Proxy;

                        var form = JsonConvert.DeserializeObject<GuildVerificationForm>(JsonConvert.SerializeObject(resp.Body));

                        var time_zone = int.Parse(form.Version.Split('+')[1].Split(':')[0]);
                        StringBuilder sb = new StringBuilder(form.Version);
                        int h = int.Parse(sb[11].ToString() + sb[12].ToString());
                        h = h - time_zone;
                        if (h < 1)
                            h = 24 - h;
                        //sb.Replace("+02:00", "000+00:00");
                        //form.Version = sb.ToString();
                        var hString = h.ToString();
                        if(hString.Length < 2)
                        {
                            while(hString.Length < 2)
                            {
                                hString = "0" + hString;
                            }
                        }
                        var time_zone_string = time_zone.ToString();
                        if (time_zone_string.Length < 2)
                        {
                            while (time_zone_string.Length < 2)
                            {
                                time_zone_string = "0" + time_zone_string;
                            }
                        }
                        string buf1 = form.Version.Substring(0, 11) + hString + form.Version.Substring(13);
                        form.Version = buf1.Replace("+" + time_zone_string + ":00", "000+00:00");
                        string json_string = "";
                        try
                        {
                            form.Fields[0].Response = true;
                        }
                        catch { }
                        form.Description = null;
                        var desc = "{\"description\":null,";
                        json_string = JsonConvert.SerializeObject(form, Formatting.None);
                        if (!resp.Body.ToString().Contains(",\"description\":null,\"automations\":null"))
                        {
                            if (resp.Body.ToString().Contains("\"description\":null"))
                            {
                                json_string = json_string.Replace(",\"automations\":null", "");
                            }
                            else
                            {
                                json_string = json_string.Replace(",\"description\":null,\"automations\":null", "");
                            }
                        }
                        json_string = "{" + json_string.Substring(desc.Length);
                        request.ClearAllHeaders();
                        request.AddHeader("Accept", "*/*");
                        request.AddHeader("Accept-Encoding", "gzip, deflate");
                        request.AddHeader("Accept-Language", "it");
                        request.AddHeader("Authorization", _discordClient.Token);
                        //request.AddHeader("Content-Type", "application/json");
                        request.AddHeader("Content-Length", (ASCIIEncoding.UTF8.GetBytes(json_string).Length).ToString());
                        request.AddHeader("Cookie", "__cfduid=db537515176b9800b51d3de7330fc27d61618084707; __dcfduid=ec27126ae8e351eb9f5865035b40b75d");
                        request.AddHeader("origin", "https://discord.com");
                        request.AddHeader("Referer", $"https://discord.com/channels/{guild_id}");
                        request.AddHeader("TE", "Trailers");
                        request.AddHeader("User-Agent", _discordClient.Config.SuperProperties.UserAgent);
                        request.AddHeader("X-Debug-Options", "bugReporterEnabled");
                        //request.AddHeader("x-fingerprint", "903995807798296608.8ycsY24VnE7UWwSqpXx2yeE-AfM");
                        request.AddHeader("X-Super-Properties", _discordClient.Config.SuperProperties.ToBase64());

                        var response = request.Put(endpoint, json_string, "application/json");

                        //var resp1 = new DiscordHttpResponse((int)response.StatusCode, response.ToString());
                    }

                    DiscordHttpUtil.ValidateResponse(resp.StatusCode, resp.Body);
                    return resp;
                }
                catch (Exception ex) when (ex is HttpException || ex is HttpRequestException || ex is TaskCanceledException)
                {
                    if (ex.Message.EndsWith("429"))
                        throw;
                    if (retriesLeft == 0)
                        throw new DiscordConnectionException();

                    retriesLeft--;
                }
                catch (RateLimitException ex)
                {
                    if (_discordClient.Config.RetryOnRateLimit)
                        Thread.Sleep(ex.RetryAfter);
                    else
                        throw new DiscordHttpException(new DiscordHttpError(DiscordError.CannotExecuteInDM, "Rate Limit"));
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


        public async Task<DiscordHttpResponse> PostAsync(string endpoint, object payload = null, ulong? guildId = null, ulong? channelId = null)
        {
            return await SendAsync(Leaf.xNet.HttpMethod.POST, endpoint, payload, guildId, channelId);
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
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
    }

    public static class Fingerprint
    {
        [JsonProperty("assignments")]
        public static Array assignments { get; set; } = null;
        [JsonProperty("fingerprint")]
        public static string fingerprint { get; set; } = null;
        public static async Task GetFingerprint()
        {
            string request_url = "https://discord.com/api/v9/experiments";
            HttpClient client = new HttpClient();
            var response_context = await client.SendAsync(new HttpRequestMessage()
            {
                Method = new System.Net.Http.HttpMethod("GET"),
                RequestUri = new Uri(request_url)
            });
            var resp_context = new DiscordHttpResponse((int)response_context.StatusCode, response_context.Content.ReadAsStringAsync().Result);
            var json = JObject.Parse(resp_context.Body.ToString());
            Fingerprint.fingerprint = json.Value<string>("fingerprint");
        }
    }
}