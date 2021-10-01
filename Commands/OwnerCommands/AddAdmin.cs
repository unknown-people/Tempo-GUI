
using Discord.Commands;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Gateway;
using Discord.Media;
using System.Threading;
using YoutubeExplode.Videos.Streams;
using System.IO;
using System.Text.RegularExpressions;
using System;
using TempoWithGUI;

namespace Music_user_bot.Commands
{
    [Command("adda")]
    class AddAdmin : CommandBase
    {
        [Parameter("User ID")]
        public ulong IDtoAdd { get; private set; }

        public override void Execute()
        {
            try
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

                if (IDtoAdd.ToString().Length == 18)
                {
                    Admin.AddToAl(IDtoAdd);
                    SendMessageAsync("Added <@" + IDtoAdd.ToString() + "> to the Admins");
                }
                else SendMessageAsync("Usage: adda [userID]");
            }
            catch (Exception)
            {
                SendMessageAsync("Usage: adda [userID]");
            }
        }
    }
}
