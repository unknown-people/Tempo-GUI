using Discord;
using Discord.Commands;
using Discord.Gateway;
using Discord.Media;
using TempoWithGUI;

namespace Music_user_bot.Commands
{
    [Command("artist")]
    class ArtistCommand : CommandBase
    {
        [Parameter("artist")]
        public string artist { get; private set; }
        public override void Execute()
        {
            if (TrackQueue.Message == null)
                TrackQueue.Message = Message;
            var targetConnected = Client.GetVoiceStates(Message.Author.User.Id).GuildVoiceStates.TryGetValue(Message.Guild.Id, out var theirState);

            var voiceClient = Client.GetVoiceClient(Message.Guild.Id);
            var channel = (VoiceChannel)Client.GetChannel(theirState.Channel.Id);

            if (!App.TrackLists.TryGetValue(Message.Guild.Id, out var list))
                list = App.TrackLists[Message.Guild.Id] = new TrackQueue(Client, Message.Guild.Id);
            var playlist = Spotify.GetArtistPlaylist(artist);
            foreach (var track in playlist)
            {
                list.Tracks.Add(track);
            }

            bool isMuted = false;
            if (TrackQueue.isSilent)
                isMuted = true;
            if (voiceClient.State < MediaConnectionState.Ready || voiceClient.Channel == null || voiceClient.Channel.Id != channel.Id)
                voiceClient.Connect(channel.Id, new VoiceConnectionProperties() { Muted = isMuted, Deafened = false });
            else if (!list.Running)
                list.Start();
            SendMessageAsync("Added the top " + playlist.Count + " songs from " + artist);
        }
    }
}
