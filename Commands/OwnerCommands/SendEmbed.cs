﻿using Discord.Commands;
using Discord.Gateway;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Music_user_bot.Commands
{
    [Command("embed")]
    class SendEmbedCommand : CommandBase
    {
        [Parameter("message")]
        public string message { get; set; }
        public override void Execute()
        {
            SendMessageAsync(message);
        }
    }
}
