using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using SpotifyAPI;
using SpotifyAPI.Web;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;

namespace Music_user_bot
{
    class Spotify
    {
        public static SpotifyClient spotify {get;set;}

        public const string clientId = "7c8a47e9ab444aa59af1c4311ca992e7";
        public const string clientSecret = "96ce712677be4fc9b772cb8084368fbe";
        public static string access_token { get; set; }

        public static async Task Login()
        {
            var config = SpotifyClientConfig.CreateDefault();

            var request = new ClientCredentialsRequest(clientId, clientSecret);
            var response = await new OAuthClient(config).RequestToken(request);
            access_token = response.AccessToken;

            spotify = new SpotifyClient(config.WithToken(response.AccessToken));
        }
        public static string GetTrack(string track_id)
        {
            if (spotify == null)
                Login().GetAwaiter().GetResult();
            var tracks = spotify.Tracks;
            var track = tracks.Get(track_id).GetAwaiter().GetResult();
            var youtube_query = track.Artists[0].Name + " - " + track.Name;
            return youtube_query;
        }
        public static List<string> GetPlaylist(string playlist_id)
        {
            if (spotify == null)
                Login().GetAwaiter().GetResult();
            var playlist = new List<string>() { };
            var playlists = spotify.Playlists;

            var playlistGetItemsRequest = new PlaylistGetItemsRequest();
            playlistGetItemsRequest.Fields.Add("items(track(name,type))");
            var playlistItems = playlists.GetItems(playlist_id).GetAwaiter().GetResult();
            
            foreach(var track in playlistItems.Items)
            {
                var query = ((FullTrack)(track.Track)).Artists[0].Name + " - " + ((FullTrack)(track.Track)).Name;
                playlist.Add(query);
            }
            return playlist;
        }
        public static List<string> GetArtistPlaylist(string artist_name)
        {
            if (spotify == null)
                Login().GetAwaiter().GetResult();
            var playlist = new List<string>() { };
            var search = spotify.Search;
            FullArtist artist = null;
            try
            {
                artist = search.Item(new SearchRequest(SearchRequest.Types.Artist, artist_name)).GetAwaiter().GetResult().Artists.Items[0];
            }
            catch
            {
                Login().GetAwaiter().GetResult();
                Thread.Sleep(2000);
                search = spotify.Search;
                artist = search.Item(new SearchRequest(SearchRequest.Types.Artist, artist_name)).GetAwaiter().GetResult().Artists.Items[0];
            }
            var artists = spotify.Artists;
            
            foreach (var track in artists.GetTopTracks(artist.Id.ToString(), new ArtistsTopTracksRequest("IT")).GetAwaiter().GetResult().Tracks)
            {
                playlist.Add(track.Artists[0].Name + " - " + track.Name);
            }
            return playlist;
        }
        public static FullArtist GetArtist(string name)
        {
            if (spotify == null)
                Login().GetAwaiter().GetResult();
            var search = spotify.Search;
            try
            {
                var artist = search.Item(new SearchRequest(SearchRequest.Types.Artist, name)).GetAwaiter().GetResult().Artists.Items[0];
                return artist;
            }
            catch (IndexOutOfRangeException)
            {
                return null;
            }
        }
        public static string GetRelatedTrack(string old_song_name)
        {
            if (spotify == null)
                Login().GetAwaiter().GetResult();
            try
            {
                old_song_name = old_song_name.Split('-')[1].Trim();
            }
            catch (IndexOutOfRangeException) { old_song_name = old_song_name.Trim(); }
            if (old_song_name.Contains("("))
            {
                old_song_name = Regex.Replace(old_song_name, @"\((.*)\)", string.Empty, RegexOptions.IgnoreCase);
            }
            if (old_song_name.Contains("["))
            {
                old_song_name = Regex.Replace(old_song_name, @"\[(.*)\]", string.Empty, RegexOptions.IgnoreCase);
            }
            if (old_song_name.Contains("{"))
            {
                old_song_name = Regex.Replace(old_song_name, @"\{(.*)\}", string.Empty, RegexOptions.IgnoreCase);
            }
            old_song_name = Regex.Replace(old_song_name, "[^0-9a-zA-Z ]+", "");
            var old_song_id = "";
            var old_artist_id = "";
            try
            {
                var old_songs = spotify.Search.Item(new SearchRequest(SearchRequest.Types.Track, old_song_name)).GetAwaiter().GetResult().Tracks.Items;
                FullTrack old_song = null;
                if (old_songs.Count != 0)
                {
                    old_song = old_songs[0];
                }
                else
                    return null;
                old_song_id = old_song.Id;
                old_artist_id = old_song.Artists[0].Id;
            }
            catch (IndexOutOfRangeException)
            {
                return null;
            }
            catch (Exception)
            {
                return null;
            }
            HttpClient client = new HttpClient(new HttpClientHandler());
            client.DefaultRequestHeaders.Add("authorization", $"Bearer {access_token}");

            var response = client.SendAsync(new HttpRequestMessage()
            {
                Method = new HttpMethod("GET"),
                RequestUri = new Uri($"https://api.spotify.com/v1/recommendations/?seed_tracks={old_song_id}&seed_artists={old_artist_id}&limit=1")
            }).GetAwaiter().GetResult();

            var json = JObject.Parse(JToken.Parse(response.Content.ReadAsStringAsync().Result).ToString());
            var song_id = json["tracks"][0].Value<string>("id");
            var tracks = spotify.Tracks;
            var song = tracks.Get(song_id).GetAwaiter().GetResult();
            return song.Artists[0].Name + " - " + song.Name;
        }
    }
}
