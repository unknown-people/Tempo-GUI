using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Music_user_bot.Commands
{
    [Command("ff")]
    class FFCommand : CommandBase
    {
        [Parameter("amount of time to skip")]
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
            TrackQueue.FFseconds = (int)time_stamp.TotalSeconds;
            var message = Message.Channel.SendMessage("Skipped " + TrackQueue.FFseconds.ToString() + " seconds");

            Task.Run(() =>
            {
                var start = DateTime.Now;
                while (true)
                {
                    var now = DateTime.Now;
                    if ((now - start).TotalSeconds > 10.0)
                    {
                        message.Delete();
                        break;
                    }
                }
            });
        }
    }
}
