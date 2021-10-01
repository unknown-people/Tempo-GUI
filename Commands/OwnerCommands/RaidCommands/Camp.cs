using Discord.Commands;
using Discord;
using Discord.Gateway;
using System;
using TempoWithGUI;
using System.Threading;

namespace Music_user_bot.Commands
{
    [Command("camp")]
    class CampCommand : CommandBase
    {
        [Parameter("channel", optional: true)]
        public ulong channelId { get; set; }
        public VoiceChannel channel { get; set; }
        public override void Execute()
        {
            if (!App.isOwner(Message))
            {
                SendMessageAsync("You need to be the owner to execute this command!");
                return;
            }
            if (channelId == 0 || channelId.ToString().Length != 18)
            {
                App.isCamping = false;

                SendMessageAsync("Not camping any channel anymore.\n\n" +
                    "Usage: " + CommandHandler.Prefix + "camp [channelId]");
            }
            else
            {
                App.isCamping = true;
                channel = (VoiceChannel)Client.GetChannel(channelId);
                SendMessageAsync("Now following camping " + channel.Name);

                Thread camp = new Thread(() => CampChannel(Message));
                camp.Priority = ThreadPriority.Lowest;
                camp.Start();
            }
        }

        private void CampChannel(DiscordMessage Message)
        {
            try
            {
                while (App.isCamping)
                {
                    try
                    {
                        var voiceClient = Client.GetVoiceClient(Message.Guild.Id);

                        if (channel == null || voiceClient.Channel == null)
                        {
                            if (!App.TrackLists.TryGetValue(Message.Guild.Id, out var list))
                                App.TrackLists[Message.Guild.Id] = new TrackQueue(Client, Message.Guild.Id);
                            bool isMuted = false;
                            if (TrackQueue.isSilent)
                                isMuted = true;
                            voiceClient.Connect(channel.Id, new Discord.Media.VoiceConnectionProperties { Muted = isMuted, Deafened = false });
                        }
                        if (voiceClient.Channel.Id != channel.Id)
                        {
                            voiceClient.Disconnect();
                            continue;
                        }
                        while (channel.UserLimit > 0 && Client.GetChannelVoiceStates(channel.Id).Count >= channel.UserLimit)
                        {
                            Thread.Sleep(100);
                            if (Client.GetChannelVoiceStates(channel.Id).Count <= channel.UserLimit)
                                throw new InvalidOperationException("Channel is full");
                        };
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }
            catch (Exception)
            {
                SendMessageAsync("Be sure to use a valid channel ID");
            }
        }
    }
}