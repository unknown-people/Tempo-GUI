using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Music_user_bot.Commands
{
    [Command("seek")]
    class SeekCommand : CommandBase
    {
        [Parameter("timestamp to skip to")]
        public string amount { get; set; }
        public override void Execute()
        {
            TimeSpan time_stamp = TimeSpan.Zero;
            try
            {
                time_stamp = TrackQueue.StringToTimeSpan(amount);
            }
            catch (Exception)
            {
                SendMessageAsync("Insert a valid timestamp. Ex: 0:30, 1:30:20, 47");
            }
            TrackQueue.seekTo = (int)time_stamp.TotalSeconds;
            var message = Message.Channel.SendMessage("Seeked to " + amount);

            Task.Run(() =>
            {
                var start = DateTime.Now;
                while (true)
                {
                    var now = DateTime.Now;
                    if ((now - start).TotalSeconds > 5.0)
                    {
                        message.Delete();
                        break;
                    }
                }
            });
        }
    }
}
