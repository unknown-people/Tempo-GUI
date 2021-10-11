using Music_user_bot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using TempoWithGUI;

namespace Discord.Media
{
    public static class DiscordVoiceUtils
    {
        public static Stream GetAudioStream(string path)
        {
            if (!File.Exists("ffmpeg.exe"))
                throw new FileNotFoundException("ffmpeg.exe was not found");

            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                Arguments = $"-nostats -loglevel -8 -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true
            });

            return process.StandardOutput.BaseStream;
        }
        public static Stream GetTTSStream(string path, float speed = 1.0f)
        {
            if (!File.Exists("ffmpeg.exe"))
                throw new FileNotFoundException("ffmpeg.exe was not found");
            float volume = 1.5f;
            if (TrackQueue.isEarrape)
                volume *= 100;
            string volume_string = volume.ToString().Replace(',', '.');

            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                Arguments = $"-nostats -loglevel -8 -i \"{path}\" -filter:a \"volume={volume_string}\" -ac 2 -f s16le -ar {(int)(48000 / speed)} pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            });

            return process.StandardOutput.BaseStream;
        }
        public static Stream GetAudioStream(string path, float offset, int duration, int volume = 100, float speed = 1.0f)
        {
            if (!File.Exists("ffmpeg.exe"))
                throw new FileNotFoundException("ffmpeg.exe was not found");
            var ffmpeg_path = (App.strWorkPath + "\\ffmpeg.exe").Replace("\\", "/");

            float volume_stream = (float)volume / 100;
            if (TrackQueue.isEarrape)
                volume_stream = volume;
            string volume_string = volume_stream.ToString().Replace(',', '.');

            var proc = new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                Arguments = $"-nostats -loglevel -8 -t {(duration * speed).ToString().Replace(',', '.')} -ss {offset.ToString().Replace(',', '.')} " +
                $"-i \"{path}\" -filter:a \"volume={volume_string}\" -ac 2 -f s16le -ar {(int)(48000 / speed)} pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            var process = Process.Start(proc);
            process.PriorityClass = ProcessPriorityClass.High;
            process.PriorityBoostEnabled = true;
            return process.StandardOutput.BaseStream;
        }
        public static byte[] GetAudio(string path)
        {
            using (var memStream = new MemoryStream())
            {
                GetAudioStream(path).CopyTo(memStream);
                return memStream.ToArray();
            }
        }
        public static Stream GetVideoStream(string path, float offset, int duration, int volume = 100, float speed = 1.0f)
        {
            if (!File.Exists("ffmpeg.exe"))
                throw new FileNotFoundException("ffmpeg.exe was not found");

            float volume_stream = (float)volume / 100;
            if (TrackQueue.isEarrape)
                volume_stream = volume;
            string volume_string = volume_stream.ToString().Replace(',', '.');
            var argument = $"-i " + '"' + path + '"' + " -r 20 -f h264 -frames:v 100 pipe:1";
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                Arguments = argument,
                UseShellExecute = false,
                RedirectStandardOutput = true
            });
            process.PriorityClass = ProcessPriorityClass.RealTime;
            process.PriorityBoostEnabled = true;
            return process.StandardOutput.BaseStream;
        }
        public static byte[] GetVideo(string path, float offset, int duration, int volume, float speed = 1.0f)
        {
            using (var memStream = new MemoryStream())
            {
                GetVideoStream(path, offset, duration, volume, speed).CopyTo(memStream);
                return memStream.ToArray();
            }
        }
        public static byte[] GetAudio(string path, float offset, int duration, int volume, float speed = 1.0f)
        {
            var stream = GetAudioStream(path, offset, duration, volume, speed);
            using (var br = new BinaryReader(stream))
            {
                return br.ReadBytes(192000 * duration);
            }
        }
        public static byte[] GetTTS(string path)
        {
            var stream = GetAudioStream(path);
            using (var br = new BinaryReader(stream))
            {
                return br.ReadBytes(192000 * 20);
            }
        }
        [Obsolete("This method is obsolete. Please use GetAudioStream instead", true)]
        public static byte[] ReadFromFile(string path)
        {
            return null;
        }

        private static List<int> FindNALUnitIndexes(byte[] byteStream)
        {
            List<int> indexes = new List<int>();

            byte[] startCode = new byte[] { 0, 0, 0, 1 };
            int currentStartCodeChar = 0;

            for (int i = 0; i < byteStream.Length; i++)
            {
                if (byteStream[i] == startCode[currentStartCodeChar])
                {
                    currentStartCodeChar++;

                    if (currentStartCodeChar == startCode.Length)
                        indexes.Add(i + 1);
                    else
                        continue; // don't wanna reset till we're done lel
                }
                
                currentStartCodeChar = 0;
            }

            return indexes;
        }

        // used for DiscordLiveStream.Write(). WIP
        private static byte[][] NALBytestreamToPackets(byte[] byteStream)
        {
            List<int> startIndexes = FindNALUnitIndexes(byteStream);

            byte[][] nalUnits = new byte[startIndexes.Count][];

            for (int i = 0; i < startIndexes.Count; i++)
            {
                int count;

                if (i == startIndexes.Count - 1)
                    count = byteStream.Length - startIndexes[i];
                else
                    count = startIndexes[i + 1] - startIndexes[i] - 4;

                nalUnits[i] = new byte[count];
                Buffer.BlockCopy(byteStream, startIndexes[i], nalUnits[i], 0, count);
            }

            byte[] starts = new byte[nalUnits.Length];

            for (int i = 0; i < starts.Length; i++)
                starts[i] = nalUnits[i][0];

            return nalUnits;
        }
    }
}
