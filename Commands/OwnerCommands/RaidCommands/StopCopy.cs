
using Discord;
using Discord.Commands;
using System.Drawing;
using TempoWithGUI;

namespace Music_user_bot.Commands
{
    [Command("stopcopy")]
    class StopCopyCommand : CommandBase
    {
        public override void Execute()
        {
            if (!App.isOwner(Message))
            {
                SendMessageAsync("You need to be the owner to execute this command!");
                return;
            }
            if (App.BlockBotCommand(Message))
            {
                SendMessageAsync("You need to use a user token to execute this command!");
                return;
            }

            Message.Guild.SetNickname(Settings.Default.Username);

            if (App.userToCopy != 0)
                SendMessageAsync("Stopped copying <@" + App.userToCopy + ">");
            else
            {
                SendMessageAsync("Not copying anyone yet");
            }
            App.userToCopy = 0;
            var path = App.strWorkPath + "\\propic.png";
            path = path.Replace('\\', '/');
            Bitmap bitmap = new Bitmap(path);
            try
            {
                Client.User.ChangeProfile(new UserProfileUpdate()
                {
                    Avatar = bitmap,
                    Username = Settings.Default.Username,
                    Password = Settings.Default.Password,
                    Biography = "Current owner is " + App.ownerName + "\n" +
                    "Come check out Tempo user-bot!"
                });
            }
            catch (DiscordHttpException)
            {
                try
                {
                    Client.User.ChangeProfile(new UserProfileUpdate()
                    {
                        Username = Settings.Default.Username,
                        Password = Settings.Default.Password,
                        Biography = "Current owner is " + App.ownerName + "\n" +
                        "Come check out Tempo user-bot!"
                    });
                }
                catch (DiscordHttpException)
                {
                    Client.User.ChangeProfile(new UserProfileUpdate()
                    {
                        Avatar = bitmap
                    });
                }
            }
        }
    }
}