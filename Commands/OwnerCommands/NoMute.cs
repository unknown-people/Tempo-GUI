using Discord;
using Discord.Commands;
using Discord.Gateway;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TempoWithGUI;

namespace Music_user_bot
{
    [Command("nomute")]
    public class NoMuteCommand : CommandBase
    {
        [Parameter("on/off")]
        public static string noMuteString { get; set; }
        public static DiscordMessage _message { get; set; }

        public static bool noMute { get; set; } = false;
        public static ulong channelId { get; private set; }
        public static ulong guildId { get; private set; }
        public static string inviteLink { get; private set; }

        public ulong getChannelID()
        {
            return channelId;
        }
        public ulong getGuildID()
        {
            return guildId;
        }
        public static string getInviteLink()
        {
            return inviteLink;
        }

        public override void Execute()
        {
            _message = Message;
            channelId = this.Message.Channel.Id;
            guildId = this.Message.Guild.Id;

            if (noMuteString == null)
                noMuteString = Message.Content.Replace(Settings.Default.Prefix + "nomute ", string.Empty);
            if (noMuteString.StartsWith("on"))
            {
                try
                {
                    inviteLink = noMuteString.Split(' ')[1];

                    if (inviteLink.StartsWith("https://discord.gg"))
                    {
                        if (noMute)
                        {
                            noMute = false;
                            Thread.Sleep(1000);
                        }
                        noMute = true;
                        Task.Run(() =>
                        {
                            while (noMute)
                            {
                                NoMute(Client);
                                Thread.Sleep(1000);
                            }
                        });
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
        private static void NoMute(DiscordSocketClient client)
        {
            var botID = client.User.Id;
            var channelID = NoMuteCommand.channelId;
            var guildID = NoMuteCommand.guildId;

            DiscordVoiceState voiceState = null;

            try
            {
                var voiceStateContainer = client.GetVoiceStates(botID);
                voiceStateContainer.GuildVoiceStates.TryGetValue(guildID, out voiceState);
            }
            catch (KeyNotFoundException)
            {
                _message.Channel.SendMessage("Bot must be connected to a voice channel");
            }

            if (voiceState != null && voiceState.Muted)
            {
                if (getInviteLink() != null)
                {
                    MinimalGuild currentGuild = client.GetGuild(guildID);
                    currentGuild.Leave();
                    Thread.Sleep(500);
                    client.JoinGuild(inviteLink);
                    var voiceClient = client.GetVoiceClient(guildID);

                    voiceClient.Connect(channelID);
                }
            }
        }
    }
}
