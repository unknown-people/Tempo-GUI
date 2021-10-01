using Discord.Commands;
using TempoWithGUI;

namespace Music_user_bot.Commands
{
    [Command("voicetts")]
    class ChangeVoiceTTSCommand : CommandBase
    {
        [Parameter("voice", true)]
        public string voice { get; set; }

        public override void Execute()
        {
            if (!App.isOwner(Message) && !App.isAdmin(Message))
            {
                SendMessageAsync("You need to be the owner or an administrator to execute this command!");
                return;
            }
            if (voice == null)
            {
                SendMessageAsync("Currently supported voices:\n**it-IT** - Diego / Isabella\n" + "**en-US** - Cristopher / Michelle");
                return;
            }
            switch (voice.ToLower())
            {
                case "diego":
                    Settings.Default.TTSlang = "it-IT";
                    Settings.Default.TTSvoice = "it-IT-DiegoNeural";
                    SendMessageAsync("Current voice set to Diego - IT");
                    break;
                case "isabella":
                    Settings.Default.TTSlang = "it-IT";
                    Settings.Default.TTSvoice = "it-IT-IsabellaNeural";
                    SendMessageAsync("Current voice set to Isabella - IT");
                    break;
                case "michelle":
                    Settings.Default.TTSlang = "en-US";
                    Settings.Default.TTSvoice = "en-US-MichelleNeural";
                    SendMessageAsync("Current voice set to Michelle - EN");
                    break;
                case "christopher":
                    Settings.Default.TTSlang = "en-US";
                    Settings.Default.TTSvoice = "en-US-ChristopherNeural";
                    SendMessageAsync("Current voice set to Christopher - EN");
                    break;
            }
        }
    }
}