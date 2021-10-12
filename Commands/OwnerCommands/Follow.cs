using Discord.Commands;
using Discord;
using Discord.Gateway;
using System;
using TempoWithGUI;
using System.Threading;

namespace Music_user_bot.Commands
{
    [Command("follow")]
    class FollowCommand : CommandBase
    {
        [Parameter("userId", optional: true)]
        public ulong userId { get; set; }
        public override void Execute()
        {
            if (!App.isOwner(Message))
            {
                return;
            }
            TrackQueue.Message = Message;

            if (userId == 0 || userId.ToString().Length != 18)
            {
                App.toFollow = false;
                TrackQueue.isLooping = false;
                TrackQueue.followSongId = null;

                SendMessageAsync("Not following anyone anymore.\n\n" +
                    "Usage: " + CommandHandler.Prefix + "follow [userId]");
            }
            else
            {
                if (App.toFollow)
                {
                    App.toFollow = false;
                    Thread.Sleep(500);
                }
                App.toFollow = true;

                SendMessageAsync("Now following <@" + userId.ToString() + ">");

                Thread follow = new Thread(() => FollowUser(userId, Message));
                follow.Start();
            }
        }

        private void FollowUser(ulong userId, DiscordMessage Message)
        {
            try
            {
                bool already_searched = false;
                AudioTrack to_loop = null;
                while (App.toFollow)
                {
                    try
                    {
                        while (Client.State < GatewayConnectionState.Connected)
                            Thread.Sleep(100);
                        var voiceClient = Client.GetVoiceClient(Message.Guild.Id);
                        var targetConnected = Client.GetVoiceStates(userId).GuildVoiceStates.TryGetValue(Message.Guild.Id, out var theirState);
                        var channel = (VoiceChannel)Client.GetChannel(theirState.Channel.Id);
                        var permissions = Client.GetCachedGuild(Message.Guild.Id).ClientMember.GetPermissions(channel.PermissionOverwrites);

                        if (voiceClient.Channel == null)
                        {
                            if (!App.TrackLists.TryGetValue(Message.Guild.Id, out var list))
                                App.TrackLists[Message.Guild.Id] = new TrackQueue(Client, Message.Guild.Id);

                            bool isMuted = false;
                            if (TrackQueue.isSilent)
                                isMuted = true;
                            voiceClient.Connect(channel.Id, new Discord.Media.VoiceConnectionProperties {Muted = isMuted, Deafened= false});
                            already_searched = false;
                        }
                        if (voiceClient.Channel.Id != channel.Id)
                        {
                            bool isMuted = false;
                            if (TrackQueue.isSilent)
                                isMuted = true;
                            try
                            {
                                if (voiceClient.State == Discord.Media.MediaConnectionState.Ready)
                                    voiceClient.Disconnect();
                                voiceClient.Connect(channel.Id, new Discord.Media.VoiceConnectionProperties { Muted = isMuted, Deafened = false });
                            }
                            catch(Exception ex) {
                                Thread.Sleep(10);
                            }
                            already_searched = false;
                            continue;
                        }
                        if (!permissions.Has(DiscordPermission.ConnectToVC) || !permissions.Has(DiscordPermission.SpeakInVC))
                        {
                            Thread.Sleep(100);
                            continue;
                        }
                        while (channel.UserLimit > 0 && Client.GetChannelVoiceStates(channel.Id).Count >= channel.UserLimit)
                        {
                            Thread.Sleep(100);
                            if (Client.GetChannelVoiceStates(channel.Id).Count <= channel.UserLimit)
                                throw new InvalidOperationException("Channel is full");
                        };
                        if (TrackQueue.followSongId != null && !already_searched)
                        {
                            if (!App.TrackLists.TryGetValue(Message.Guild.Id, out var list))
                                list = App.TrackLists[Message.Guild.Id] = new TrackQueue(Client, Message.Guild.Id);
                            TrackQueue.isLooping = true;
                            if (to_loop == null)
                                to_loop = new AudioTrack(TrackQueue.followSongId);
                            list.Tracks.Add(to_loop.Title);
                            if (!list.Running)
                            {
                                list.Start();
                                already_searched = true;
                            }
                        }
                    }
                    catch (Exception exc)
                    {
                        continue;
                    }
                }
            }
            catch (Exception)
            {
                SendMessageAsync("Be sure to use a valid user ID");
            }
        }
    }
}
