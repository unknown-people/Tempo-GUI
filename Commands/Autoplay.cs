using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Music_user_bot.Commands
{
    [Command("autoplay")]
    class AutoplayCommand : CommandBase
    {
        public override void Execute()
        {
            TrackQueue.autoplay = !TrackQueue.autoplay;
            if (TrackQueue.autoplay)
            {
                SendMessageAsync("Autoplay is now on!\nPlease note that it will start autoplaying after your queue has done playing.");
            }
            else
            {
                SendMessageAsync("Autoplay is now off.");
            }
        }
    }
}
