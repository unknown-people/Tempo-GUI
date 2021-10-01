
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

namespace Music_user_bot
{
    [Command("dela")]
    class DelAdmin : CommandBase
    {
        [Parameter("User ID")]
        public ulong IDtoDel { get; private set; }

        public override void Execute()
        {
            try
            {
                if (!App.isOwner(Message) && !App.isAdmin(Message))
                {
                    SendMessageAsync("You need to be the owner or an administrator to execute this command!");
                    return;
                }
                if (App.BlockBotCommand(Message))
                {
                    SendMessageAsync("You need to use a user token to execute this command!");
                    return;
                }
                if (IDtoDel.ToString().Length == 18)
                {
                    Admin.RemoveFromAl(IDtoDel);
                    SendMessageAsync("Removed <@" + IDtoDel.ToString() + "> from admins");
                }
                else SendMessageAsync("Usage: dela [userID]");
            }
            catch (Exception)
            {
                SendMessageAsync("Usage: dela [userID]");
            }
        }
    }
}