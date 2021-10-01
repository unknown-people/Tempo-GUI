using Discord.Gateway;
using Discord.Commands;
using System;
using TempoWithGUI;

namespace Music_user_bot
{
    [Command("skip")]
    public class SkipCommand : CommandBase
    {
        public override void Execute()
        {
            if (App.CanModifyList(Client, Message))
            {
                var list = App.TrackLists[Message.Guild.Id];
                try
                {
                    TrackQueue.currentSong.CancellationTokenSource.Cancel();
                }
                catch (IndexOutOfRangeException)
                {
                    SendMessageAsync("The queue is empty");
                    return;
                }
            }
        }
    }
}