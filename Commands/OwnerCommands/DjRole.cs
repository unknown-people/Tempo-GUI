using Discord.Commands;
using System;
using TempoWithGUI;

namespace Music_user_bot.Commands
{
    [Command("djrole")]
    class SetDjRoleCommand : CommandBase
    {
        [Parameter("new dj role")]
        public string new_dj_role_id { get; private set; }
        public override void Execute()
        {
            try
            {
                if (!App.isOwner(Message))
                {
                    SendMessageAsync("You need to be the owner to execute this command!");
                    return;
                }
                Settings.Default.Dj_role = ulong.Parse(new_dj_role_id);
                SendMessageAsync("prefix has been changed to: " + new_dj_role_id);
            }
            catch (Exception)
            {
                SendMessageAsync("Couldn't save new prefix. Try again");
            }
        }
    }
}

