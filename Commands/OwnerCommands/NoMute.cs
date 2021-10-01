using Discord.Commands;
using System;

namespace Music_user_bot
{
    [Command("nomute")]
    public class NoMuteCommand : CommandBase
    {
        [Parameter("on/off")]
        public static string noMuteString { get; private set; }

        public static bool noMute { get; set; } = false;
        public static ulong channelId { get; private set; }
        public static ulong guildId { get; private set; }
        public static string inviteLink { get; private set; }

        public NoMuteCommand(ulong guild_id, ulong channel_id)
        {
            channelId = channel_id;
            guildId = guild_id;
        }
        public NoMuteCommand()
        {
        }

        public ulong getChannelID()
        {
            return channelId;
        }
        public ulong getGuildID()
        {
            return guildId;
        }
        public string getInviteLink()
        {
            return inviteLink;
        }

        public override void Execute()
        {
            channelId = this.Message.Channel.Id;
            guildId = this.Message.Guild.Id;

            noMuteString = this.Message.Content.Replace("&nomute ", "");
            if (noMuteString.StartsWith("on"))
            {
                try
                {
                    inviteLink = noMuteString.Split(' ')[1];

                    if (inviteLink.StartsWith("https://discord.gg"))
                    {
                        noMute = true;
                        SendMessageAsync("nomute set to true");
                    }
                    else
                    {
                        SendMessageAsync("The link is not valid");
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    SendMessageAsync("Usage: &nomute on [invite-link]");
                }
            }
            else if (noMuteString == "off")
            {
                noMute = false;
                SendMessageAsync("nomute set to false");
            }
            else
            {
                SendMessageAsync("You must choose between 'on [invite]' or 'off'");
            }
        }
    }
}
