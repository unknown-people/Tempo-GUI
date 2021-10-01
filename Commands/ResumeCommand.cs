using Discord.Commands;

namespace Music_user_bot.Commands
{
    [Command("resume")]
    class ResumeCommand : CommandBase
    {
        public override void Execute()
        {
            if (TrackQueue.isPaused)
            {
                TrackQueue.isPaused = false;
                SendMessageAsync("Track resumed");
            }
            else
            {
                SendMessageAsync("Track is already playing");
            }
        }
    }
}
