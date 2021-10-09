using Discord;
using Discord.Commands;
using Discord.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TempoWithGUI.Commands
{
    [Command("call")]
    class CallCommand : CommandBase
    {
        [Parameter("userId")]
        public string user_id { get; set; }
        public override void Execute()
        {
            var userId = ulong.Parse(user_id);
            var voiceClient = Client.GetPrivateVoiceClient();
            var channel = Client.CreateDM(userId);
            Client.StartCall(channel.Id);
            if (voiceClient.State < MediaConnectionState.Ready || voiceClient.Channel == null || voiceClient.Channel.Id != channel.Id)
                voiceClient.Connect(channel.Id, new VoiceConnectionProperties() { Muted = false, Deafened = false });
        }
    }
}
