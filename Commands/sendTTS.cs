using Discord.Commands;
using Discord;
using Discord.Gateway;
using System;
using System.Threading.Tasks;
using Discord.Media;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System.IO;
using System.Threading;
using TempoWithGUI;

namespace Music_user_bot.Commands
{
    [Command("say")]
    class SendTTSCommand : CommandBase
    {
        [Parameter("message to tts")]
        public string message { get; set; }
        public static bool isTTSon { get; set; }

        public override void Execute()
        {
            if (!App.TrackLists.TryGetValue(Message.Guild.Id, out var list)) {}
            else if (list.Running)
            {
                App.SendMessage(Message, "You can't use tts commands while playing music!");
                return;
            }
            isTTSon = true;
            Task.Run(() => SynthesizeAudioAsync(message, Client, Message));
        }
        static async Task SynthesizeAudioAsync(string TTS, DiscordSocketClient Client, DiscordMessage Message)
        {
            if (!Directory.Exists(App.strWorkPath + "\\temp"))
                Directory.CreateDirectory(App.strWorkPath + "\\temp");
            var path = App.strWorkPath + "\\temp\\current_tts.wav";
            path = path.Replace('\\', '/');
            File.Delete(path);

            var config = SpeechConfig.FromSubscription(Settings.Default.APIkey, "westeurope");
            config.SpeechSynthesisLanguage = Settings.Default.TTSlang;
            config.SpeechSynthesisVoiceName = Settings.Default.TTSvoice;
            var audioConfig = AudioConfig.FromWavFileOutput(path);

            var synthesizer = new SpeechSynthesizer(config, audioConfig);

            await synthesizer.SpeakTextAsync(TTS);

            var result = DiscordVoiceUtils.GetTTS(path);
            var voiceClient = Client.GetVoiceClient(Message.Guild.Id);

            if (voiceClient.Microphone == null)
            {
                Client.GetVoiceStates(Message.Author.User.Id).GuildVoiceStates.TryGetValue(Message.Guild.Id, out var theirState);

                try
                {
                    var channel = (VoiceChannel)Client.GetChannel(theirState.Channel.Id);

                    if (voiceClient.State < MediaConnectionState.Ready || voiceClient.Channel.Id != channel.Id)
                        voiceClient.Connect(channel.Id);
                }
                catch (Exception)
                {
                    App.SendMessage(Message, "You need to be in a voice channel for me to join you :tired_face:");
                }
            }
            while (voiceClient.Microphone == null)
                Thread.Sleep(1);
            voiceClient.Microphone.CopyFrom(result);

            audioConfig.Dispose();
            synthesizer.Dispose();
            isTTSon = false;
        }
    }
}
