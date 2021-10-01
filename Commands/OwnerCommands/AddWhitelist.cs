
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
using TempoWithGUI;
using System;
using TempoWithGUI;

namespace Music_user_bot.Commands
{
    [Command("addw")]
    class AddWhitelist : CommandBase
    {
        [Parameter("User ID")]
        public ulong IDtoAdd { get; private set; }

        public override void Execute()
        {
            try
            {
                if (!App.isOwner(Message) && !App.isAdmin(Message) )
                {
                    SendMessageAsync("You need to be the owner or an administrator to execute this command!");
                    return;
                }
                if (App.BlockBotCommand(Message))
                {
                    SendMessageAsync("You need to use a user token to execute this command!");
                    return;
                }

                if (IDtoAdd.ToString().Length == 18)
                {
                    Whitelist.AddToWL(IDtoAdd);
                    SendMessageAsync("Added <@" + IDtoAdd.ToString() + "> to whitelist");
                }
                else SendMessageAsync("Usage: addw [userID]");
            }
            catch (Exception)
            {
                SendMessageAsync("Usage: addw [userID]");
            }
        }
    }
}
