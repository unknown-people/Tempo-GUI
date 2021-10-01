
using Discord;
using Discord.Commands;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using TempoWithGUI;

namespace Music_user_bot.Commands
{
    [Command("copy")]
    class CopyCommand : CommandBase
    {
        [Parameter("userId")]
        public string userId { get; set; }
        public override void Execute()
        {
            if (!App.isOwner(Message) )
            {
                SendMessageAsync("You need to be the owner to execute this command!");
                return;
            }
            if (App.BlockBotCommand(Message))
            {
                SendMessageAsync("You need to use a user token to execute this command!");
                return;
            }
            if(!ulong.TryParse(userId, out var Id))
            {
                var artist = Spotify.GetArtist(userId);
                if(artist != null)
                {
                    var avatar = artist.Images[0].Url;
                    var username = artist.Name;
                    try
                    {
                        Message.Guild.SetNickname(username);
                    }
                    catch (DiscordHttpException)
                    {
                        SendMessageAsync("Could not change guild username");
                    }
                    var path = App.strWorkPath + "\\avatar_new.png";
                    path = path.Replace('\\', '/');
                    Bitmap avatar_bitmap = null;
                    try
                    {
                        avatar_bitmap = SaveImage(avatar, ImageFormat.Png);
                    }
                    catch (ExternalException)
                    {
                        SendMessageAsync("Could not get the profile picture");
                    }
                    catch (ArgumentNullException)
                    {
                        ;
                    }
                    try
                    {
                        Client.User.ChangeProfile(new UserProfileUpdate()
                        {
                            Avatar = avatar_bitmap
                        }) ;
                        SendMessageAsync("Hi, I'm " + username + " :sunglasses:");
                    }
                    catch (DiscordHttpException)
                    {
                        Message.Channel.SendMessage("Could not change avatar, probably rate limited. Try again in a few minutes");
                    }
                }
                else
                {
                    SendMessageAsync("Could not find the specified artist :( guess I'll just be Tempo");
                }
            }
            if (userId.ToString().Length == 18)
            {
                App.userToCopy = Id;
                App.userToCopyDiscrim = Client.GetUser(App.userToCopy).Discriminator;

                var avatar = Client.GetUser(Id).Avatar;
                var username = Client.GetUser(Id).Username;
                var guild_username = Message.Guild.GetMember(Id).Nickname;
                var path = App.strWorkPath + "\\avatar_new.png";

                path = path.Replace('\\', '/');
                Bitmap avatar_bitmap = null;
                try
                {
                    avatar_bitmap = SaveImage(avatar.Url, ImageFormat.Png);
                }
                catch (ExternalException)
                {
                    ;
                }
                catch (ArgumentNullException)
                {
                    ;
                }

                try
                {
                    Client.User.ChangeProfile(new UserProfileUpdate()
                    {
                        Username = username,
                        Password = Settings.Default.Password,
                        Avatar = avatar_bitmap
                    });
                }
                catch (DiscordHttpException)
                {
                    try
                    {
                        Message.Channel.SendMessage("Could not change username");

                        Client.User.ChangeProfile(new UserProfileUpdate()
                        {
                            Avatar = avatar_bitmap
                        });
                    }
                    catch (DiscordHttpException)
                    {
                        try
                        {
                            Message.Channel.SendMessage("Could not change avatar");
                            Client.User.ChangeProfile(new UserProfileUpdate()
                            {
                                Username = username,
                                Password = Settings.Default.Password,
                            });
                        }
                        catch (DiscordHttpException)
                        {
                            ;
                        }
                    }
                }
                try
                {
                    Message.Guild.SetNickname(guild_username);
                }
                catch (DiscordHttpException)
                {
                    SendMessageAsync("Could not change guild username");
                }
                SendMessageAsync("Now copying <@" + App.userToCopy + ">");
            }
        }

        public Bitmap SaveImage(string imageUrl, ImageFormat format)
        {
            WebClient client = new WebClient();
            Stream stream = client.OpenRead(imageUrl);
            Bitmap bitmap; bitmap = new Bitmap(stream);

            stream.Flush();
            stream.Close();
            client.Dispose();
            return bitmap;
        }
    }
}