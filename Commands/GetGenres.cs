using Discord.Commands;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;

namespace Music_user_bot.Commands
{
    [Command("genres")]
    class GetGenresCommand : CommandBase
    {
        private static string GetGenres()
        {
            try
            {
                string request_url = "https://music.catostudios.nl/api/music/genres/";
                WebRequest request = WebRequest.Create(request_url);
                var response = request.GetResponse();
                var resp_stream = response.GetResponseStream();
                StreamReader reader = new StreamReader(resp_stream);

                var jarray = JArray.Parse(reader.ReadToEnd());
                string genres = "";
                foreach (JObject obj in jarray)
                {
                    var genre = obj.Value<string>("genre");
                    genres += "**" + genre + "**\n";
                }
                return genres;
            }
            catch
            {
                return null;
            }
        }

        public override void Execute()
        {
            string genres = GetGenres();
            if (genres != null)
                SendMessageAsync("Available genres are:\n" + genres);
            else
                SendMessageAsync("Couldn't fetch genre list, the APIs are probably down :(");
        }
    }
}
