using Discord;
using Discord.Commands;
using TempoWithGUI;

namespace Music_user_bot.Commands
{
    [Command("changename")]
    class ChangeNameCommand : CommandBase
    {
        [Parameter("new name")]
        public string new_name { get; set; }
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
            try
            {
                Settings.Default.Username = new_name;
                SendMessageAsync("Name correctly set. To see the changes restart Tempo.exe");
            }
            catch (DiscordHttpException)
            {
                SendMessageAsync("Couldn't set name, try restarting the program and check the password, or you may have been rate limited, so please try again later");
            }
        }
    }
}
