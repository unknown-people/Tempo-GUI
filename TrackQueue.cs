using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Gateway;
using Discord.Media;
using YoutubeExplode.Videos.Streams;
using System;
using YoutubeExplode;
using System.IO;
using System.Threading;
using YoutubeExplode.Videos;
using TempoWithGUI;
using System.Net.Http;
using Discord.Commands;
using YoutubeExplode.Search;

namespace Music_user_bot
{
    public class TrackQueue
    {
        public List<string> Tracks { get; private set; }
        public bool Running { get; set; }
        public static DiscordMessage Message { get; set; }
        public static int seekTo { get; set; }
        public static int FFseconds { get; internal set; }
        public static float speed { get; set; }
        public static string followSongId { get; set; }
        public static int goToIndex { get; set; }
        public static bool isLooping { get; set; }
        public static bool isPaused { get; set; }
        public static DateTime pauseTime { get; set; }
        public static int pauseTimeSec { get; set; }
        public DateTime start_time { get; set; }
        public DiscordMessage last_message { get; set; }
        public Stream _stream { get; set; }
        public static int stream_volume { get; set; }
        public static AudioTrack currentSong { get; set; }
        public static VideoSearchResult currentVideo { get; set; }
        public static TimeSpan currentSongTime { get; set; }
        public static bool displayMessage { get; set; }
        public static bool deleteMessage { get; set; }
        public static bool isEarrape { get; set; }
        public static bool isSilent { get; set; } = false;
        public static bool speedChanged = false;
        public static bool earrapeChanged = false;
        public static bool isStopping { get; set; }
        public static bool isAddingTracks { get; set; }
        public static bool isVolumeChanged { get; internal set; }
        public static bool autoplay { get; set; }
        public FFmpeg ffmpeg { get; set; }

        private DiscordSocketClient _client;
        private ulong _guildId;

        public TrackQueue(DiscordSocketClient client, ulong guildId)
        {
            _client = client;
            _guildId = guildId;
            Tracks = new List<string>();
            isLooping = false;
            stream_volume = 100;
            speed = 1.0f;
            displayMessage = false;
        }

        public async void Start()
        {
            Running = true;
            //Not working due to DiscordHTTPException emoji not found
            /*
            Thread info_message = new Thread(() =>
            {
                while (Running)
                {
                    if (displayMessage)
                    {
                        last_message = Message.Channel.SendMessage("**Now playing:**\n" + currentVideo.Title);
                        last_message.AddReaction(":rewind:");
                        last_message.AddReaction(":arrow_forward:");
                        last_message.AddReaction(":pause_button:");
                        last_message.AddReaction(":fast_forward:");
                        while (displayMessage)
                        {
                            var reactions_rewind = last_message.GetReactions(new ReactionQuery() { 
                                ReactionName = ":rewind:"
                            });
                            var reactions_play = last_message.GetReactions(new ReactionQuery()
                            {
                                ReactionName = ":arrow_forward:"
                            });
                            var reactions_pause = last_message.GetReactions(new ReactionQuery()
                            {
                                ReactionName = ":pause_button:"
                            });
                            var reactions_ff = last_message.GetReactions(new ReactionQuery()
                            {
                                ReactionName = ":fast_forward:"
                            });
                        }
                    }
                    else
                        continue;
                    while (!deleteMessage)
                        Thread.Sleep(1);
                    last_message.Delete();
                }
            });
            */
            /*
            Thread info_message = new Thread(() =>
            {
                while (Running)
                {
                    if (displayMessage)
                    {
                        var duration = currentVideo.Duration.Value;
                        string duration_string = duration.ToString();
                        last_message = Message.Channel.SendMessage("**Now playing:**\n" + currentVideo.Title + "\n" +
                            ":white_circle:─────────────────────────────    " + "00:00:00 / "+ duration_string);
                        var line_base = "──────────────────────────────    ";
                        int circle_pos = 0;
                        float tick_time = (float)duration.TotalSeconds / 30;

                        while(DiscordVoiceInput.current_time_tracker < (int)duration.TotalSeconds && !currentSong.CancellationTokenSource.Token.IsCancellationRequested)
                        {
                            duration = currentVideo.Duration.Value;
                            duration_string = duration.ToString();

                            circle_pos = (int)(DiscordVoiceInput.current_time_tracker / tick_time);

                            string before = line_base.Substring(0, circle_pos);
                            string after = line_base.Remove(line_base.Length - (circle_pos + 1)) + "    ";
                            var new_message = before + ":white_circle:" + after;

                            var song_time_string = (new TimeSpan(DiscordVoiceInput.current_time_tracker * TimeSpan.TicksPerSecond)).ToString();
                            if(song_time_string.Split('.').Length > 1)
                            {
                                song_time_string = song_time_string.Split('.')[0];
                            }
                            
                            string time_track = song_time_string + " / " + duration_string;
                            string content = "**Now playing:**\n" + currentVideo.Title + "\n" + new_message + time_track;

                            try
                            {
                                if (content.Equals(last_message.Content))
                                    continue;
                                last_message.Edit(new MessageEditProperties()
                                {
                                    Content = content
                                });
                            }
                            catch { break; }
                            Thread.Sleep(50);
                        }
                    }
                }
            });
            */
            Thread track_queue = new Thread(async () =>
            {
                FFseconds = 0;
                seekTo = 0;

                var voiceClient = _client.GetVoiceClient(_guildId);

                last_message = null;
                var last_song_name = "";

                while (voiceClient.State == MediaConnectionState.Ready && Tracks.Count > 0 || autoplay)
                {
                    if(autoplay && Tracks.Count == 0)
                    {
                        var new_track = Spotify.GetRelatedTrack(currentSong.Title);
                        if (new_track != null)
                            Tracks.Add(new_track);
                        else
                        {
                            Message.Channel.SendMessage("I couldn't find any songs related to this one so here's a random one");
                            var vid = new AudioTrack(CommandHandler.GetRandomSong());
                            Tracks.Add(vid.Title);
                        }
                    }
                    if (isStopping)
                    {
                        Tracks = new List<string>();
                        isStopping = false;
                        continue;
                    }
                    if (App.TrackLists.TryGetValue(_guildId, out var list) && goToIndex > 0)
                    {
                        for (int i = 0; i < goToIndex - 2; i++)
                        {
                            list.Tracks.RemoveAt(0);
                        }
                        goToIndex = 0;
                    }
                    
                    VoiceChannel currentChannel = (VoiceChannel)_client.GetChannel(voiceClient.Channel.Id);

                    Proxy proxy = null;
                    var httpClient = new HttpClient();
                    HttpClientHandler handler;
                    var youtube = new YoutubeClient(httpClient);
                    try
                    {
                        currentVideo = App.YouTubeClient.Search.GetVideo(Tracks[0]);
                    }
                    catch
                    {
                        Proxy.GetProxies("https://www.youtube.com");
                        proxy = Proxy.GetFirstWorkingProxy("https://www.youtube.com");

                        if (proxy != null)
                        {
                            handler = new HttpClientHandler()
                            {
                                Proxy = new System.Net.WebProxy("http://" + proxy._ip + ":" + proxy._port),
                                UseProxy = true
                            };
                            httpClient = new HttpClient(handler);
                        }
                        youtube = new YoutubeClient(httpClient);
                        currentVideo = App.YouTubeClient.Search.GetVideo(Tracks[0]);
                    }
                    currentSong = new AudioTrack(currentVideo.Id);
                    last_song_name = Tracks[0];
                    displayMessage = true;

                    TimeSpan duration = TimeSpan.Zero;
                    if (currentVideo.Duration != null)
                    {
                        duration = (TimeSpan)currentVideo.Duration;
                    }

                    start_time = DateTime.Now;
                    pauseTimeSec = 0;
                    string url = GetAudioUrl(currentSong.Id, currentChannel.Bitrate);
                    DiscordVoiceInput.current_time = 0;
                    DiscordVoiceInput.current_time_tracker = 0;

                    try
                    {
                        if (last_message != null)
                            last_message.Delete();
                    }
                    catch (Exception) { }

                    var targetConnected = _client.GetVoiceStates(Message.Author.User.Id).GuildVoiceStates.TryGetValue(Message.Guild.Id, out var theirState);

                    if (CanSendEmbed(_client)){
                        var embed = new EmbedMaker() { Title = _client.User.Username, TitleUrl = "https://discord.gg/bXfjwSeBur", Color = System.Drawing.Color.IndianRed, ThumbnailUrl = _client.User.Avatar.Url };
                        embed.AddField("**Now playing:**\n", currentVideo.Title + "\n\n**Duration:** " + currentVideo.Duration);

                        last_message = Message.Channel.SendMessage(embed);
                    }
                    else
                        last_message = Message.Channel.SendMessage("**Now playing:**\n" + Tracks[0] + "\n");
                    while (voiceClient.Microphone.CopyFrom( url, (int)duration.TotalSeconds, currentSong.CancellationTokenSource.Token))
                    {
                        if (TrackQueue.speedChanged)
                        {
                            TrackQueue.speedChanged = false;
                            continue;
                        }
                        if(FFseconds > 0)
                        {
                            DiscordVoiceInput.current_time += FFseconds;
                            FFseconds = 0;
                            continue;
                        }
                        if(seekTo > 0)
                        {
                            DiscordVoiceInput.current_time = TrackQueue.seekTo;
                            seekTo = 0;
                            continue;
                        }
                        if (earrapeChanged)
                        {
                            earrapeChanged = false;
                            continue;
                        }
                        if (isVolumeChanged)
                        {
                            isVolumeChanged = false;
                            continue;
                        }
                        pauseTime = DateTime.Now;
                        while (isPaused)
                        {
                            Thread.Sleep(10);
                        }
                        pauseTimeSec += (int)(pauseTime - start_time).TotalSeconds;
                        start_time = DateTime.Now;
                        FFseconds = 0;
                        seekTo = 0;
                    }
                    displayMessage = false;
                    try
                    {
                        last_message.Delete();
                    }
                    catch { }
                    try
                    {
                        if (isLooping)
                        {
                            Tracks.Add(Tracks[0]);
                        }
                    }
                    catch (Exception)
                    {
                        ;
                    }

                    Tracks.RemoveAt(0);
                }
                Running = false;
            });
            //info_message.Priority = ThreadPriority.Highest;
            //info_message.Start();
            track_queue.IsBackground = true;

            track_queue.Start();
            track_queue.Priority = ThreadPriority.Highest;
        }
        public static TimeSpan StringToTimeSpan(string input)
        {
            var string_split = input.Split(':');
            var result = "";
            var return_value = TimeSpan.Zero;
            if(string_split.Length == 2)
            {
                var buffer = new string[] { "00" };
                buffer = buffer.Concat(string_split).ToArray();
                result = string.Join(":", buffer);
                return_value = TimeSpan.Parse(result);
            }
            else if(string_split.Length == 3)
            {
                return_value = TimeSpan.Parse(result);
            }
            else if(string_split.Length == 1)
            {
                var buffer0 = new string[] { "00", "00" };
                var buffer = buffer0.Concat(string_split).ToArray();
                result = string.Join(":", buffer);
                return_value = TimeSpan.Parse(result);
            }
            return return_value;
        }
        public async ValueTask<bool> IsSleeping(bool running)
        {
            DateTime last_song = DateTime.Now;
            while (!running)
            {
                DateTime now = DateTime.Now;
                TimeSpan since_last_song = now.Subtract(last_song);
                if ((int)since_last_song.TotalSeconds > 300)
                {
                    return true;
                }
            }
            return false;
        }
        /*
        public static string GetAudioUrl(string videoId)
        {
            var manifest = App.YouTubeClient.Videos.Streams.GetManifestAsync(videoId).Result;

            return manifest.GetAudioOnlyStreams().OrderBy(s => s.Bitrate).Last().Url;
        }
        */
        public static string GetAudioUrl(string videoId, uint channelBitrate)
        {
            var manifest = App.YouTubeClient.Videos.Streams.GetManifestAsync(videoId).Result;

            AudioOnlyStreamInfo bestStream = null;
            foreach (var stream in manifest.GetAudioOnlyStreams().OrderBy(s => s.Bitrate))
            {
                if (bestStream == null || stream.Bitrate > bestStream.Bitrate)
                {
                    bestStream = stream;

                    if (stream.Bitrate.BitsPerSecond > channelBitrate)
                        break;
                }
            }

            return bestStream.Url;
        }
        private string GetVideoUrl(string videoId, uint channelBitrate)
        {
            var manifest = App.YouTubeClient.Videos.Streams.GetManifestAsync(videoId).Result;

            VideoOnlyStreamInfo bestStream = null;
            foreach (var stream in manifest.GetVideoOnlyStreams().OrderBy(s => s.Bitrate))
            {
                if (bestStream == null || stream.Bitrate > bestStream.Bitrate)
                {
                    bestStream = stream;

                    if (stream.Bitrate.BitsPerSecond > channelBitrate)
                        break;
                }
            }

            return bestStream.Url;
        }
        public bool CanSendEmbed(DiscordSocketClient Client)
        {
            var channel = (TextChannel)Client.GetChannel(Message.Channel.Id);
            try
            {
                if (CommandBase.isAdminDict[Message.Guild.Id])
                    return true;
            }
            catch { }
            if (channel.PermissionOverwrites.Count == 0)
                return true;

            foreach (var entry in channel.PermissionOverwrites)
            {
                if (entry.AffectedId == Message.Author.User.Id)
                {
                    var result = entry.GetPermissionState(DiscordPermission.EmbedLinks) == OverwrittenPermissionState.Allow;
                    if (result)
                        return true;
                }
            }
            return false;
        }
    }
}
