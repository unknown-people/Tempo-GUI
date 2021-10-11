using Discord.Media;
using Music_user_bot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TempoWithGUI
{
    public class FFmpeg
    {
        public string ffmpeg_path { get; set; }
        public StreamWriter input { get; set; }
        public Stream output { get; set; }
        public void Initialize(string path = null)
        {
            if(path != null)
            {
                ffmpeg_path = path;
            }
            else
            {
                ffmpeg_path = App.strWorkPath + "\\ffmpeg.exe";
                ffmpeg_path = ffmpeg_path.Replace("\\", "/");
            }
            var proc = new ProcessStartInfo()
            {
                FileName = "cmd.exe",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true
            };
            var process = Process.Start(proc);
            input = process.StandardInput;
            output = process.StandardOutput.BaseStream;
        }
        public byte[] Execute(string path, float offset, int duration, int volume = 100, float speed = 1.0f)
        {
            if (!File.Exists("ffmpeg.exe"))
                throw new FileNotFoundException("ffmpeg.exe was not found");

            float volume_stream = (float)volume / 100;
            if (TrackQueue.isEarrape)
                volume_stream = volume;
            string volume_string = volume_stream.ToString().Replace(',', '.');

            var args = $"-nostats -loglevel -8 -t {(duration * speed).ToString().Replace(',', '.')} -ss {offset.ToString().Replace(',', '.')} " +
                $"-i \"{path}\" -filter:a \"volume={volume_string}\" -vn -ac 2 -f s16le -ar {(int)(48000 / speed)} pipe:1";

            input.WriteLine(ffmpeg_path + " " + args);
            byte[] ret = new byte[192000 * DiscordVoiceInput.buffer_duration];
            output.Read(ret, 0, ret.Length);
            return ret;
        }
    }
}
