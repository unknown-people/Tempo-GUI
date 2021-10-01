using Discord.Commands;

namespace Music_user_bot.Commands
{
    [Command("loop")]
    class LoopCommand : CommandBase
    {
        public override void Execute()
        {
            TrackQueue.isLooping = !TrackQueue.isLooping;
            if (TrackQueue.isLooping)
            {
                SendMessageAsync("Queue is now looping");
            }
            else
            {
                SendMessageAsync("Queue stopped looping");
            }
        }
    }
}
