using Discord.Commands;
using Discord;
using TempoWithGUI;

namespace Music_user_bot.Commands
{
    [Command("stopspamdm")]
    class StopSpamDmCommand : CommandBase
    {
        public static bool stopSpamDm { get; set; }
        public override void Execute()
        {
            if (!App.isOwner(Message) && !App.isAdmin(Message))
            {
                SendMessageAsync("You need to be the owner or an administrator to execute this command!");
                return;
            }
            stopSpamDm = true;
            if(stopSpamDm)
            {
                Client.CreateDM(Message.Author.User.Id).SendMessage("Stopped spamming dm");
            }
        }
    }
}
