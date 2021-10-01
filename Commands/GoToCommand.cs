using Discord.Commands;
using TempoWithGUI;


namespace Music_user_bot
{
    [Command("goto")]
    class GoToCommand : CommandBase
    {
        [Parameter("song index")]
        public int song_index { get; set; }

        public override void Execute()
        {
            if (App.TrackLists.TryGetValue(Message.Guild.Id, out var list))
            {
                if (song_index < list.Tracks.Count)
                {
                    TrackQueue.goToIndex = song_index;
                    TrackQueue.currentSong.CancellationTokenSource.Cancel();
                    SendMessageAsync("Skipped to track **[" + song_index.ToString() + "]**");
                }
                else
                    SendMessageAsync("Index out of range. See the current queue with **" + Settings.Default.Prefix + "queue**");
            }
            else
            {
                SendMessageAsync("There's no track to skip to");
            }
        }
    }
}
