
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
                TrackQueue.isPaused = true;
                SendMessageAsync("Paused current track");
            }
        }
    }
}
