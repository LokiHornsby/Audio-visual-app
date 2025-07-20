using NAudio.Wave;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Audio_visual_app {
    public static class LAVT {
        // Audio variables
        private static WaveOutEvent? outputDevice;
        private static AudioFileReader? audioFile;

        static double hours = 0;
        static double minutes = 0;
        static double seconds = 0;

        private static bool playing = false;

        public static void initialiseNAudio(string filename) {
            // Store PCM Data
            //WaveStream ws = new Mp3FileReader(filename);
            //WaveStream pcm = WaveFormatConversionStream.CreatePcmStream(ws);
            //int interval = pcm.WaveFormat.Channels * pcm.WaveFormat.SampleRate * pcm.WaveFormat.BitsPerSample / 8;

            // initialise audio variables
            outputDevice = new WaveOutEvent();
            outputDevice.PlaybackStopped += OnPlaybackStopped;

            audioFile = new AudioFileReader(filename);
            outputDevice.Init(audioFile);
        }

        public static TimeSpan StripMilliseconds(TimeSpan time) {
            return new TimeSpan(time.Hours, time.Minutes, time.Seconds);
        }

        private static void OnPlaybackStopped(object? sender, StoppedEventArgs args) {
            StopPlayback();
        }

        public static void StopPlayback() {
            playing = false;
            audioFile.Position = 0;
            outputDevice.Stop();
        }

        public static void PausePlayback() {
            playing = false;
            outputDevice.Pause();
        }

        public static void StartPlayback() {
            playing = true;
            outputDevice.Play();
        }

        public static TimeSpan GetCurrentTime() {
            return audioFile.CurrentTime;
        }

        public static TimeSpan GetTotalTime() {
            return audioFile.TotalTime;
        }

        public static void Seek(int s) {
            //audioFile.Position = (long)slider1.Value;
            audioFile.CurrentTime = new TimeSpan(
                0,
                0,
                s
            );
        }
    }
}