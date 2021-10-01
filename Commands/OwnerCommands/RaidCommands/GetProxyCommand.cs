using Discord.Commands;

namespace Music_user_bot.Commands.OwnerCommands
{
    [Command("proxy")]
    class GetProxyCommand : CommandBase
    {
        [Parameter("url")]
        public string url { get; set; }
        public override void Execute()
        {
            SendMessageAsync("Started looking for proxies, this may take a while");
            if (url.EndsWith("/"))
                url = url.TrimEnd('/');
            Proxy proxy = Proxy.GetFirstWorkingProxy(url);
            if (proxy != null)
            {
                url = url.Replace("https://", "").Replace("https://", "");
                SendMessageAsync("Working proxy for " + url + ":\n**" + proxy._port + ":" + proxy._ip + "**");
            }
            else
                SendMessageAsync("No working proxies found for this url");
        }
    }
}
