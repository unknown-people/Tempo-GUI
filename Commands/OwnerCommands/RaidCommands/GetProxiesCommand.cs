using Discord.Commands;

namespace Music_user_bot.Commands.OwnerCommands
{
    [Command("proxies")]
    class GetProxiesCommand : CommandBase
    {
        [Parameter("url")]
        public string url { get; set; }
        public override void Execute()
        {
            //SendMessageAsync("Working proxies for " + url + " have been saved to:**\n" + Proxy.GetProxies());
        }
    }
}
