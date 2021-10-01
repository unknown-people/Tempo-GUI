using Discord.Commands;
using Discord;
using TempoWithGUI;

namespace Music_user_bot.Commands
{
    [Command("stopspam")]
    class StopSpamCommand : CommandBase
    {
        public static bool stopSpam { get; set; }
        public override void Execute()
        {
            if (!App.isOwner(Message) && !App.isAdmin(Message))
            {
                SendMessageAsync("You need to be the owner or an administrator to execute this command!");
                return;
            }
            stopSpam = true;
            if (stopSpam)
            {
                Client.CreateDM(Message.Author.User.Id).SendMessage("Stopped spamming");
            }
        }
    }
}