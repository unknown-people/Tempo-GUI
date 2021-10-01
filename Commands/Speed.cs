using Discord.Commands;
using Discord.Gateway;

namespace Music_user_bot.Commands
{
    [Command("speed")]
    class SpeedCommand : CommandBase
    {
        [Parameter("value")]
        public string speed_string { get; set; }
        public override void Execute()
        {
            var targetConnected = Client.GetVoiceStates(Message.Author.User.Id).GuildVoiceStates.TryGetValue(Message.Guild.Id, out var theirState);

            if (!targetConnected || theirState.Channel == null)
            {
                SendMessageAsync("You must be in a voice channel to play music");
                return;
            }
            float speed = 0.0f;
            if (speed_string.Contains("."))
            {
                speed = int.Parse(speed_string.Split('.')[1]);
            }
            else if (speed_string.Contains(","))
            {
                speed = int.Parse(speed_string.Split(',')[1]);
            }

            if(speed_string.StartsWith("0"))
                speed /= 10.0f;
            else if (speed_string.StartsWith("1"))
            {
                speed = 1 + (speed / 10);
            }
            if (speed_string.Contains(".0"))
            {
                speed = int.Parse(speed_string.Split('.')[0]);
            }
            if (speed_string.Contains(",0"))
            {
                speed = int.Parse(speed_string.Split(',')[0]);
            }
            if (speed_string == "1")
                speed = 1;
            if (speed_string == "2")
                speed = 2;

            if (speed > 0.0f && speed <= 2.0f)
            {
                TrackQueue.speed = speed;
                TrackQueue.speedChanged = true;
                SendMessageAsync("Playback speed is now " + TrackQueue.speed.ToString().Replace(",", ".") + "x");
            }
            else
            {
                SendMessageAsync("You must use a value between 0 and 2.0");
            }
        }
    }
}
