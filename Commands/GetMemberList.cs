using Discord.Commands;
using Discord.Gateway;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Music_user_bot.Commands
{
    [Command("members")]
    class GetMemberList : CommandBase
    {
        public override void Execute()
        {
            var members = ((DiscordSocketClient)Client).GetGuildMembers(883384983857819708);
            MessageBox.Show("ciao cane");
        }
    }
}
