
using Discord.Commands;
using System;
using TempoWithGUI;

namespace Music_user_bot.Commands
{
    [Command("stop")]
    class StopCommand : CommandBase
    {
        public override void Execute()
        {
            TrackQueue list = App.TrackLists[Message.Guild.Id];
            AudioTrack currentSong = TrackQueue.currentSong;
            try
            {
                Stop();
                currentSong.CancellationTokenSource.Cancel();
            }
            catch (IndexOutOfRangeException)
            {
                Message.Channel.SendMessage("Queue is already empty");
                return;
            }
        }

        public void Stop()
        {
            App.TrackLists[Message.Guild.Id] = new TrackQueue(Client, Message.Guild.Id);
            TrackQueue.isStopping = true;
            TrackQueue.autoplay = false;
        }
    }
}
