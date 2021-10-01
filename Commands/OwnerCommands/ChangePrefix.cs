using Discord;
using Discord.Commands;
using Discord.Gateway;
using System;
using TempoWithGUI;

namespace Music_user_bot.Commands
{
    [Command("prefix")]
    class ChangePrefix : CommandBase
    {
        [Parameter("new prefix")]
        public string new_prefix { get; private set; }
        public override void Execute()
        {
            try
            {
                if (!App.isOwner(Message))
                {
                    SendMessageAsync("You need to be the owner to execute this command!");
                    return;
                }
                Settings.Default.Prefix = new_prefix;
                
                CommandHandler.Prefix = new_prefix;

                var activity = new ActivityProperties();
                activity.Type = ActivityType.Listening;
                activity.Name = Settings.Default.Prefix + "help";

                Client.UpdatePresence(new PresenceProperties()
                {
                    Status = UserStatus.DoNotDisturb,
                    Activity = activity
                });

                SendMessageAsync("prefix has been changed to: " + new_prefix);
            }
            catch (Exception)
            {
                SendMessageAsync("Couldn't save new prefix. Try again");
            }
        }
    }
}
