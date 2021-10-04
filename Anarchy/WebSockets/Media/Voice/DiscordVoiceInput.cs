using Music_user_bot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Discord.Media
{
    public class DiscordVoiceInput
    {
        private readonly int _packetLoss = 0;

        private OpusEncoder _encoder;
        private readonly object _voiceLock = new object();

        private long _nextTick;
        private ushort _sequence;
        private uint _timestamp;
        private DiscordVoiceClient _client;

        private uint _bitrate = 64000;
        private AudioApplication _audioApp = AudioApplication.Mixed;

        public static byte[] buffer_next;
        public static string path;
        public static float current_time;
        public static int current_time_tracker;
        public static int buffer_duration = 8;

        public uint Bitrate
        {
            get { return _bitrate; }
            set
            {
                _bitrate = value;
                UpdateEncoder();
            }
        }

        public AudioApplication AudioApplication
        {
            get { return _audioApp; }
            set 
            { 
                _audioApp = value;
                UpdateEncoder();
            }
        }

        internal DiscordVoiceInput(DiscordVoiceClient client)
        {
            _client = client;

            UpdateEncoder();
            _nextTick = -1;
        }

        private void UpdateEncoder() => _encoder = new OpusEncoder(_bitrate, _audioApp, _packetLoss);


        public void SetSpeakingState(DiscordSpeakingFlags flags)
        {
            if (_client.State < MediaConnectionState.Ready)
                throw new InvalidOperationException("Client is not currently connected");

            if (TrackQueue.isSilent)
                flags = DiscordSpeakingFlags.Soundshare;
            _client.Connection.Send(DiscordMediaOpcode.Speaking, new DiscordSpeakingRequest()
            {
                State = flags,
                Delay = 0,
                SSRC = _client.Connection.SSRC.Audio
            });
        }


        public int Write(byte[] buffer, int offset)
        {
            if (_client.State < MediaConnectionState.Ready)
                throw new InvalidOperationException("Client is not currently connected");

            lock (_voiceLock)
            {
                try
                {
                    if (_nextTick == -1)
                        _nextTick = Environment.TickCount;
                    else
                    {
                        long distance = _nextTick - Environment.TickCount;

                        if (distance > 0)
                            Thread.Sleep((int)distance);
                    }

                    byte[] opusFrame = new byte[OpusConverter.FrameBytes];
                    int frameSize = OpusConverter.FrameBytes;

                    frameSize = _encoder.EncodeFrame(buffer, offset, opusFrame, 0);

                    byte[] packet = new RTPPacketHeader()
                    {
                        Type = DiscordMediaConnection.SupportedCodecs["opus"].PayloadType,
                        Sequence = _sequence,
                        Timestamp = _timestamp,
                        SSRC = _client.Connection.SSRC.Audio
                    }.Write(_client.Connection.SecretKey, opusFrame, 0, frameSize);

                    _client.Connection.UdpClient.Send(packet, packet.Length);

                    _nextTick += OpusConverter.TimeBetweenFrames;
                    _sequence++;
                    _timestamp += OpusConverter.FrameSamplesPerChannel;
                }
                catch { return offset; }
            }

            return offset + OpusConverter.FrameBytes;
        }
        public int WriteVideo(byte[] buffer, int offset)
        {
            if (_client.State < MediaConnectionState.Ready)
                return 0;
            int frameSize = OpusConverter.FrameBytes;

            lock (_voiceLock)
            {
                if (_nextTick == -1)
                    _nextTick = Environment.TickCount;
                else
                {
                    long distance = _nextTick - Environment.TickCount;

                    if (distance > 0)
                        Thread.Sleep((int)distance);
                }

                byte[] opusFrame = new byte[frameSize];

                Array.Copy(buffer, offset, opusFrame, 0, frameSize);
                
                byte[] packet = new RTPPacketHeader()
                {
                    Type = DiscordMediaConnection.SupportedCodecs["H264"].PayloadType,
                    Sequence = _sequence,
                    Timestamp = _timestamp,
                    SSRC = _client.Connection.SSRC.Video
                }.Write(_client.Connection.SecretKey, opusFrame, 0, opusFrame.Length);

                _client.Connection.UdpClient.Send(packet, packet.Length);

                _nextTick += OpusConverter.TimeBetweenFrames;
                _sequence++;
                _timestamp += OpusConverter.FrameSamplesPerChannel;
            }

            return offset + frameSize;
        }
        public int CopyFrom(byte[] buffer, int offset = 0, CancellationToken cancellationToken = default, int streamDuration = 30)
        {
            if (_client.State < MediaConnectionState.Ready)
                throw new InvalidOperationException("Client is not currently connected");

            _nextTick = Environment.TickCount;

            var start = DateTime.Now;

            while (offset < buffer.Length && !cancellationToken.IsCancellationRequested)
            {
                var end = DateTime.Now;
                TimeSpan duration = end.Subtract(start);
                if ((int)duration.TotalSeconds >= streamDuration)
                {
                    return 1;
                }

                try
                {
                    offset = Write(buffer, offset);
                }
                catch (Exception)
                {
                    break;
                }
            }
            return 0;
        }
        public bool CopyFrom(Stream stream, int v, CancellationToken cancellationToken = default, int streamDuration = 0)
        {
            if (_client.State < MediaConnectionState.Ready)
                throw new InvalidOperationException("Client is not currently connected");

            if (!stream.CanRead)
                throw new ArgumentException("Cannot read from stream", "stream");

            _nextTick = -1;
            int read;
            var start = DateTime.Now;

            do
            {
                byte[] buffer = new byte[OpusConverter.FrameBytes];
                read = stream.Read(buffer, 0, buffer.Length);
                int offset = 0;

                while (offset < buffer.Length && !cancellationToken.IsCancellationRequested)
                {
                    var end = DateTime.Now;
                    TimeSpan duration = end.Subtract(start);
                    if ((int)duration.TotalSeconds >= streamDuration)
                    {
                        return true;
                    }

                    try
                    {
                        offset = Write(buffer, offset);
                    }
                    catch (InvalidOperationException)
                    {
                        break;
                    }
                    catch (AccessViolationException)
                    {
                        continue;
                    }
                }
            } 
            while (read != 0);

            return false;
        }
        public bool CopyFrom(string path_input, int duration, CancellationToken cancellationToken = default)
        {
            if (_client.State < MediaConnectionState.Ready)
                return true;

            _nextTick = -1;

            path = path_input;
            if (TrackQueue.pauseTimeSec > 0)
            {
                current_time = TrackQueue.pauseTimeSec - 1;
            }
            
            byte[] buffer = DiscordVoiceUtils.GetAudio(path, current_time, buffer_duration, TrackQueue.stream_volume, TrackQueue.speed);

            bool isBufferReady = false;
            Thread create_buffer_next = new Thread(() =>
            {
                while(true)
                {
                    isBufferReady = false;
                    buffer_next = DiscordVoiceUtils.GetAudio(path, current_time, buffer_duration, TrackQueue.stream_volume, TrackQueue.speed);
                    isBufferReady = true;
                    while (isBufferReady)
                        Thread.Sleep(1);
                }
            });
            create_buffer_next.Priority = ThreadPriority.Highest;

            bool toBreak = false;
            var base_buffer = 192000 * buffer_duration;
            do
            {
                try
                {
                    if (TrackQueue.isPaused)
                    {
                        create_buffer_next.Abort();
                        return true;
                    }
                    current_time += (buffer_duration * TrackQueue.speed);
                    if (!create_buffer_next.IsAlive)
                        create_buffer_next.Start();

                    if (current_time > duration)
                        toBreak = true;

                    int offset = 0;
                    
                    DateTime start = DateTime.Now;
                    while (offset < buffer.Length && !cancellationToken.IsCancellationRequested)
                    {
                        if (TrackQueue.isPaused || TrackQueue.FFseconds > 0 || TrackQueue.speedChanged || TrackQueue.seekTo > 0 || TrackQueue.earrapeChanged || TrackQueue.isVolumeChanged)
                        {
                            create_buffer_next.Abort();
                            return true;
                        }
                        if((DateTime.Now - start).TotalSeconds * TrackQueue.speed > 1.0f)
                        {
                            current_time_tracker += 1;
                            start = DateTime.Now;
                        }
                        try
                        {
                            offset = Write(buffer, offset);
                        }
                        catch (Exception)
                        {
                            break;
                        }
                    }
                    var ticks = 0;
                    while (!isBufferReady)
                    {
                        Thread.Sleep(1);
                        ticks++;
                        if (ticks >= 1000)
                            break;
                    }
                    while (buffer == buffer_next)
                        Thread.Sleep(1);
                    buffer = buffer_next;
                    isBufferReady = false;
                }
                catch (Exception)
                {
                    continue;
                }
            }
            while (!cancellationToken.IsCancellationRequested && !toBreak);
            create_buffer_next.Abort();
            return false;
        }
        public bool CopyFromVideo(string path_input, int duration, CancellationToken cancellationToken = default)
        {
            if (_client.State < MediaConnectionState.Ready)
                throw new InvalidOperationException("Client is not currently connected");

            _nextTick = -1;

            path = path_input;
            if (TrackQueue.pauseTimeSec > 0)
            {
                current_time = TrackQueue.pauseTimeSec - 1;
            }

            byte[] buffer = DiscordVoiceUtils.GetVideo(path, current_time, buffer_duration, TrackQueue.stream_volume, TrackQueue.speed);

            bool isBufferReady = false;
            Thread create_buffer_next = new Thread(() =>
            {
                while (true)
                {
                    isBufferReady = false;
                    buffer_next = DiscordVoiceUtils.GetVideo(path, current_time, buffer_duration, TrackQueue.stream_volume, TrackQueue.speed);
                    isBufferReady = true;
                    while (isBufferReady)
                        Thread.Sleep(1);
                }
            });
            create_buffer_next.Priority = ThreadPriority.Highest;

            bool toBreak = false;

            do
            {
                try
                {
                    if (TrackQueue.isPaused)
                    {
                        create_buffer_next.Abort();
                        return true;
                    }
                    current_time += (buffer_duration * TrackQueue.speed);
                    if (!create_buffer_next.IsAlive)
                        create_buffer_next.Start();

                    if (current_time > duration)
                        toBreak = true;

                    int offset = 0;

                    DateTime start = DateTime.Now;
                    while (offset < buffer.Length && !cancellationToken.IsCancellationRequested)
                    {
                        if (TrackQueue.isPaused || TrackQueue.FFseconds > 0 || TrackQueue.speedChanged || TrackQueue.seekTo > 0)
                        {
                            create_buffer_next.Abort();
                            return true;
                        }
                        if ((DateTime.Now - start).TotalSeconds * TrackQueue.speed > 1.0f)
                        {
                            current_time_tracker += 1;
                            start = DateTime.Now;
                        }
                        try
                        {
                            offset = WriteVideo(buffer, offset);
                        }
                        catch (Exception)
                        {
                            break;
                        }
                    }
                    var ticks = 0;
                    while (!isBufferReady)
                    {
                        Thread.Sleep(1);
                        ticks++;
                        if (ticks >= 1000)
                            break;
                    }
                    while (buffer == buffer_next)
                        Thread.Sleep(1);
                    buffer = buffer_next;
                    isBufferReady = false;
                }
                catch (Exception)
                {
                    continue;
                }
            }
            while (!cancellationToken.IsCancellationRequested && !toBreak);
            create_buffer_next.Abort();
            return false;
        }
        public static bool IsNullOrEmpty(byte[] array)
        {
            if (array == null || array.Length == 0)
                return true;
            else
                return array.All(item => item == null || item == 0);
        }
    }
}
