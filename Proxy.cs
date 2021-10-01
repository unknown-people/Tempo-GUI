using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using TempoWithGUI;
using System.Threading;
using System.Threading.Tasks;

namespace Music_user_bot
{
    class Proxy
    {
        public string _ip { get; private set; }
        public string _port { get; private set; }
        public static List<Proxy> working_proxies { get; set; }
        public static List<Proxy> ssl_working_proxies { get; set; }

        public static string proxies_file_path { get; set; }
        public static string ssl_proxies_file_path { get; set; }

        public const string proxies_txt = "http_proxies.txt";
        public const string ssl_proxies_txt = "https_proxies.txt";
        public Proxy(string ip, string port)
        {
            _ip = ip;
            _port = port;
        }
        public static List<bool> TestProxies(string url, List<Proxy> proxies)
        {
            var test_results = new List<bool>() { };
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            foreach(Proxy proxy in proxies)
            {
                request.Proxy = new WebProxy(proxy._ip, int.Parse(proxy._port));
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/39.0.2171.95 Safari/537.36";
                request.Timeout = 2000;

                try
                {
                    WebResponse response = request.GetResponse();
                }
                catch (Exception)
                {
                    test_results.Add(false);
                    continue;
                }
                test_results.Add(true);
            }
            return test_results;
        }
        public static bool TestProxy(string url, Proxy proxy)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            request.Proxy = new WebProxy(proxy._ip, int.Parse(proxy._port));
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/39.0.2171.95 Safari/537.36";
            request.Timeout = 2000;

            try
            {
                WebResponse response = request.GetResponse();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
        public static void GetProxies(string url)
        {
            if (working_proxies == null)
                working_proxies = new List<Proxy>() { };
            if(!Directory.Exists(App.strWorkPath + @"\proxies"))
            {
                Directory.CreateDirectory(App.strWorkPath + @"\proxies");
            }
            proxies_file_path = App.strWorkPath + @"\proxies\" + proxies_txt;

            while (true)
            {
                try
                {
                    File.Delete(proxies_file_path);
                    break;
                }
                catch { Thread.Sleep(100); }
            }

            using (var client = new WebClient())
            {
                while (true)
                {
                    try
                    {
                        client.DownloadFile("https://api.proxyscrape.com/v2/?request=getproxies&protocol=http&timeout=2000&country=all&ssl=all&anonymity=all&simplified=true", proxies_file_path);
                        break;
                    }
                    catch { Thread.Sleep(100); }
                }
            }
            FilterProxies(proxies_file_path, url);

        }
        public static void GetSSLProxies(string url)
        {
            if (ssl_working_proxies == null)
                ssl_working_proxies = new List<Proxy>() { };
            if (!Directory.Exists(App.strWorkPath + @"\proxies"))
            {
                Directory.CreateDirectory(App.strWorkPath + @"\proxies");
            }
            ssl_proxies_file_path = App.strWorkPath + @"\proxies\" + ssl_proxies_txt;

            while (true)
            {
                try
                {
                    File.Delete(ssl_proxies_file_path);
                    break;
                }
                catch { Thread.Sleep(100); }
            }

            using (var client = new WebClient())
            {
                client.DownloadFile("https://api.proxyscrape.com/v2/?request=getproxies&protocol=http&timeout=10000&country=all&ssl=yes&anonymity=all&simplified=true", ssl_proxies_file_path);
            }
            FilterSSLProxies(ssl_proxies_file_path, url);
        }
        public static void FilterProxies(string url)
        {
            int i = 0;
            var buffer_proxies = working_proxies;
            foreach(Proxy proxy in working_proxies)
            {
                if (!TestProxy(url, proxy))
                {
                    buffer_proxies.RemoveAt(i);
                }
                i++;
            }
            working_proxies = buffer_proxies;
        }
        public static void FilterSSLProxies(string url)
        {
            int i = 0;
            var buffer_proxies = ssl_working_proxies;
            foreach (Proxy proxy in ssl_working_proxies)
            {
                if (!TestProxy(url, proxy))
                {
                    buffer_proxies.RemoveAt(i);
                }
                i++;
            }
            ssl_working_proxies = buffer_proxies;
        }
        public static void FilterProxies(string file_path, string url)
        {
            using (var sr = new StreamReader(file_path))
            {
                string proxy_string = sr.ReadLine();
                Proxy proxy = new Proxy(proxy_string.Split(':')[0], proxy_string.Split(':')[1]);
                if (TestProxy(url, proxy))
                {
                    working_proxies.Add(proxy);
                }
            }
        }
        public static void FilterSSLProxies(string file_path, string url)
        {
            using (var sr = new StreamReader(file_path))
            {
                string proxy_string = sr.ReadLine();
                Proxy proxy = new Proxy(proxy_string.Split(':')[0], proxy_string.Split(':')[1]);
                if (TestProxy(url, proxy))
                {
                    ssl_working_proxies.Add(proxy);
                }
            }
        }
        public static Proxy GetFirstWorkingProxy(string url)
        {
            foreach(Proxy proxy in working_proxies)
            {
                if (TestProxy(url, proxy))
                {
                    return proxy;
                }
            }
            GetProxies(url);
            foreach (Proxy proxy in working_proxies)
            {
                if (TestProxy(url, proxy))
                {
                    return proxy;
                }
            }
            return null;
        }
        public static Proxy GetFirstWorkingSSLProxy(string url)
        {
            foreach (Proxy proxy in ssl_working_proxies)
            {
                if (TestProxy(url, proxy))
                {
                    Task.Run(() => FilterSSLProxies(url));
                    return proxy;
                }
            }
            Task.Run(() => FilterSSLProxies(url));
            return null;
        }
    }
}
