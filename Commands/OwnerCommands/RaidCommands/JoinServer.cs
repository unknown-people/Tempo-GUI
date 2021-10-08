using System;
using Discord.Commands;
using Discord;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using TempoWithGUI;

namespace Music_user_bot.Commands
{
    [Command("joinserver")]
    class JoinServerCommand : CommandBase
    {
        [Parameter("Guild invite")]
        public string invite { get; set; }
        public override void Execute()
        {
            if (!App.isOwner(Message))
            {
                SendMessageAsync("You need to be the owner to execute this command!");
                return;
            }
            if (App.BlockBotCommand(Message))
            {
                SendMessageAsync("You need to use a user token to execute this command!");
                return;
            }
            string invite_code = null;
            ulong guildId = 0;
            try
            {
                invite_code = Regex.Replace(invite, "https://discord.gg/", string.Empty, RegexOptions.IgnoreCase);

                guildId = ulong.Parse(GetInviteGuildAsync(invite_code).GetAwaiter().GetResult());

                if (IsInGuild(Client, guildId))
                    SendMessageAsync("You're already in the guild");

            }
            catch { SendMessageAsync("Couldn't join guild.\n\nUsage: " + CommandHandler.Prefix + "joinserver [invite/code]"); return; }
            int i = 0;
            while (true)
            {
                if (i > 10)
                    break;
                try
                {
                    Client.JoinGuild(invite_code);
                    var ver = Client.GetGuildVerificationForm(guildId, invite_code);
                }
                catch (Exception ex)
                {
                    SendMessageAsync("Couldn't join guild.\n\nUsage: " + CommandHandler.Prefix + "joinserver [invite/code]");
                }
                i++;
            }
        }
        public async Task<string> GetInviteGuildAsync(string inv_code)
        {
            string request_url = "https://discord.com/api/v9/invites/" + inv_code + "?inputValue=" + inv_code + "&with_counts=true&with_expiration=true";
            HttpClient client = new HttpClient();
            var response_context = await client.SendAsync(new HttpRequestMessage()
            {
                Method = new HttpMethod("GET"),
                RequestUri = new Uri(request_url)
            });
            var resp_context = new DiscordHttpResponse((int)response_context.StatusCode, response_context.Content.ReadAsStringAsync().Result);
            var json = JObject.Parse(resp_context.Body.ToString());

            return json["guild"].Value<string>("id");
        }
        public bool IsInGuild(DiscordClient Client, ulong guildId)
        {
            foreach (var guild in Client.GetGuilds())
            {
                if (guildId == guild.Id)
                    return true;
            }
            return false;
        }
    }
}
