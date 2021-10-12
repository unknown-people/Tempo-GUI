
using Discord.Commands;

namespace Music_user_bot.Commands
{
    [Command("pause")]
    class PauseCommand : CommandBase
    {
        public override void Execute()
        {
            if (TrackQueue.isPaused)
            {
                SendMessageAsync("Current track is already paused");
            }
            else
            {
                if(TrackQueue.currentSong != null)
                {
                    TrackQueue.isPaused = true;
                    SendMessageAsync("Paused current track");
                }
                else
                {
                    SendMessageAsync("There are no tracks currently playing");
                }
            }
        }
    }
}
