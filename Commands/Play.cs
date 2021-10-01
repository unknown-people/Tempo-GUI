
using Discord.Commands;
using Discord;
using Discord.Gateway;
using Discord.Media;
using System.Text.RegularExpressions;
using System;
using System.Threading.Tasks;
using YoutubeExplode.Search;
using TempoWithGUI;
using System.Collections.Generic;
using YoutubeExplode.Playlists;
using System.Threading;
using System.Net.Http;
using YoutubeExplode;
using YoutubeExplode.Common;

namespace Music_user_bot.Commands
{
    [Command("play")]
    public class PlayCommand : CommandBase
    {
        [Parameter("YouTube video URL")]
        public string Url { get; private set; }

        public const string YouTubeVideo = "https://www.youtube.com/watch?v=";
        public const string YouTubePlaylist = "https://youtube.com/playlist?list=";
        public List<string> spotiPlaylist { get; set; }

        public override void Execute()
        {
            if (SendTTSCommand.isTTSon)
            {
                App.SendMessage(Message, "You can't play music while tts is playing");
                return;
            }
            TrackQueue.Message = Message;

            if (App.toFollow && TrackQueue.followSongId != null)
            {
                App.SendMessage(Message, "Currently following a user, cannot play any other songs");
                return;
            }

            var targetConnected = Client.GetVoiceStates(Message.Author.User.Id).GuildVoiceStates.TryGetValue(Message.Guild.Id, out var theirState);

            if (!targetConnected || theirState.Channel == null)
            {
                App.SendMessage(Message, "You must be in a voice channel to play music");
                return;
            }

            var channel = (VoiceChannel)Client.GetChannel(theirState.Channel.Id);
            var voiceClient = Client.GetVoiceClient(Message.Guild.Id);

            try
            {
                if (voiceClient.Channel != null && voiceClient.Channel.Id != channel.Id)
                {
                    voiceClient.Disconnect();
                }
                if (voiceClient.Channel == null)
                {
                    App.TrackLists[Message.Guild.Id] = new TrackQueue(Client, Message.Guild.Id);
                }
            }
            catch (Exception)
            {
                ;
            }
            if (voiceClient.State < MediaConnectionState.Ready || (voiceClient.Channel != null && voiceClient.Channel.Id != channel.Id))
            {
                var permissions = Client.GetCachedGuild(Message.Guild.Id).ClientMember.GetPermissions(channel.PermissionOverwrites);

                if (!permissions.Has(DiscordPermission.ConnectToVC) || !permissions.Has(DiscordPermission.SpeakInVC))
                {
                    App.SendMessage(Message, "I lack permissions to play music in this channel");
                    return;
                }

                else if (channel.UserLimit > 0 && Client.GetChannelVoiceStates(channel.Id).Count >= channel.UserLimit)
                {
                    App.SendMessage(Message, "Your channel is full");
                    return;
                }
            }
            bool isPlaylist = false;
            if (Url.Contains("spotify.com/track"))
            {
                Url = Url.Replace("https://open.spotify.com/track/", "");
                Url = Url.Split('?')[0];
                Url = Spotify.GetTrack(Url) + " lyrics";
            }
            if (Url.Contains("spotify.com/playlist"))
            {
                spotiPlaylist = new List<string>() { };
                Url = Url.Replace("https://open.spotify.com/playlist/", "");
                Url = Url.Split('?')[0];
                spotiPlaylist = Spotify.GetPlaylist(Url);
            }
            // Substitutes all occurences of m.youtube with youtube due to the link being previously broken af
            if (Url.Contains("m.youtube"))
            {
                Url = Url.Replace("m.youtube", "www.youtube");
            }
            if (Url.Contains("&ab_channel="))
            {
                Url = Regex.Replace(Url, "&ab_channel=.*", string.Empty, RegexOptions.IgnoreCase);
            }
            if (Url.Contains("&t="))
            {
                Url = Regex.Replace(Url, "&t=.*", string.Empty, RegexOptions.IgnoreCase);
            }
            if (Url.Contains("list="))
            {
                isPlaylist = true;
            }
            TrackQueue.isPaused = false;
            var isQuery = !isPlaylist;

            if (Url.StartsWith(YouTubeVideo) || Url.StartsWith(YouTubePlaylist))
            {
                int result = SearchVideo(Url, Message, voiceClient, channel, Client, null, isQuery, isPlaylist).GetAwaiter().GetResult();
                if(App.TrackLists.TryGetValue(Message.Guild.Id, out var list))
                    if (result == 0)
                        SendMessageAsync("Track " + list.Tracks[list.Tracks.Count - 1] + " has been added to the queue");
            }
            else if (spotiPlaylist != null && spotiPlaylist != new List<string>() { })
            {
                SearchVideo(Url, Message, voiceClient, channel, Client, spotiPlaylist, isQuery, isPlaylist).GetAwaiter().GetResult();
            }
            else
            {
                int result = SearchVideo(Url, Message, voiceClient, channel, Client, null, true, isPlaylist).GetAwaiter().GetResult();
                if (App.TrackLists.TryGetValue(Message.Guild.Id, out var list))
                    if (result == 0)
                        SendMessageAsync("Track " + list.Tracks[list.Tracks.Count - 1] + " has been added to the queue");
            }
        }
        public static async Task<int> SearchVideo(string Url, DiscordMessage Message, DiscordVoiceClient voiceClient, VoiceChannel channel, DiscordSocketClient Client, List<string> spoti_playlist, bool isQuery = false, bool isList = false)
        {
            string id = "";
            AudioTrack track = null;

            if (spoti_playlist != null)
            {
                Proxy proxy = Proxy.GetFirstWorkingProxy("https://www.youtube.com");
                var httpClient = new HttpClient();
                HttpClientHandler handler;
                if (proxy != null)
                {
                    handler = new HttpClientHandler()
                    {
                        Proxy = new System.Net.WebProxy("http://" + proxy._ip + ":" + proxy._port),
                        UseProxy = true
                    };
                    httpClient = new HttpClient(handler);
                }
                var youtube = new YoutubeClient(httpClient);

                if (!App.TrackLists.TryGetValue(Message.Guild.Id, out var list))
                {
                    list = App.TrackLists[Message.Guild.Id] = new TrackQueue(Client, Message.Guild.Id);
                }

                App.SendMessage(Message, "Added " + spoti_playlist.Count.ToString() + " tracks to the queue");

                foreach (string video_name in spoti_playlist)
                {
                    list.Tracks.Add(video_name);
                }

                if (voiceClient.State < MediaConnectionState.Ready || voiceClient.Channel == null || voiceClient.Channel.Id != channel.Id)
                    voiceClient.Connect(channel.Id, new VoiceConnectionProperties() { Muted = true, Deafened = false });
                else if (!list.Running)
                    list.Start();
                return 1;
            }

            if (isList)
            {
                TrackQueue list_video = null;
                var url_split = Url.Split('&');
                foreach (var query in url_split)
                {
                    if (query.Contains("list="))
                    {
                        id = Regex.Replace(query, "^(.*?)(?=:|list=|$)", string.Empty, RegexOptions.IgnoreCase);
                        if(query.StartsWith("list="))
                            id = Regex.Replace(query, "list=", string.Empty, RegexOptions.IgnoreCase);
                        break;
                    }
                }
                var playlist = await App.YouTubeClient.Playlists.GetVideosMinimalAsync(id);

                if (!App.TrackLists.TryGetValue(Message.Guild.Id, out list_video)) 
                    list_video = App.TrackLists[Message.Guild.Id] = new TrackQueue(Client, Message.Guild.Id);

                while (TrackQueue.isAddingTracks)
                    Thread.Sleep(100);
                TrackQueue.isAddingTracks = true;
                int i = 0;
                foreach (PlaylistVideoMinimal video in playlist)
                {
                    list_video.Tracks.Add(video.Title);
                }
                App.SendMessage(Message, "Added " + playlist.Count.ToString() + " tracks to the queue");

                if (voiceClient.State < MediaConnectionState.Ready || voiceClient.Channel == null || voiceClient.Channel.Id != channel.Id)
                    voiceClient.Connect(channel.Id, new VoiceConnectionProperties() { Muted = true, Deafened = false });
                else if (!list_video.Running)
                    list_video.Start();
                return 1;
            }
            else
            {
                if (!isQuery)
                {
                    id = Url.Substring(Url.IndexOf(YouTubeVideo) + YouTubeVideo.Length);
                }
                else
                {
                    VideoSearchResult video = App.YouTubeClient.Search.GetVideo(Url);
                    id = video.Id;
                }
                try
                {
                    track = new AudioTrack(id);
                }
                catch (ArgumentException)
                {
                    App.SendMessage(Message, "Please enter a valid YouTube video URL");
                    return 1;
                }
                if (!App.TrackLists.TryGetValue(Message.Guild.Id, out var list))
                {
                    list = App.TrackLists[Message.Guild.Id] = new TrackQueue(Client, Message.Guild.Id);
                }
                while (TrackQueue.isAddingTracks)
                    Thread.Sleep(100);
                TrackQueue.isAddingTracks = true;

                list.Tracks.Add(track.Title);

                TrackQueue.isAddingTracks = false;

                bool isMuted = false;
                if (TrackQueue.isSilent)
                    isMuted = true;

                if (voiceClient.State < MediaConnectionState.Ready || voiceClient.Channel == null || voiceClient.Channel.Id != channel.Id)
                    voiceClient.Connect(channel.Id, new VoiceConnectionProperties() { Muted = isMuted, Deafened = false});
                else if (!list.Running)
                    list.Start();

                return 0;
            }
        }
    }
}