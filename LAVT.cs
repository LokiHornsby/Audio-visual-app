using NAudio.Wave;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Audio_visual_app {
    public static class LAVT {
        // Audio variables
        private static WaveOutEvent? outputDevice;
        private static AudioFileReader? audioFile;

        private static bool playing = false;

        /// <summary>
        /// Initialise NAudio
        /// </summary>
        /// <param name="filename"></param>
        public static void initialiseNAudio(string? filename) {
            if (filename != null) {
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
        }

        /// <summary>
        /// Remove milliseconds from time span object
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static TimeSpan StripMilliseconds(TimeSpan time) {
            return new TimeSpan(time.Hours, time.Minutes, time.Seconds);
        }

        /// <summary>
        /// Get current position of audio
        /// </summary>
        /// <returns></returns>
        public static TimeSpan GetCurrentTime() {
            return audioFile.CurrentTime;
        }

        /// <summary>
        /// Get total length of the audio
        /// </summary>
        /// <returns></returns>
        public static TimeSpan GetTotalTime() {
            return audioFile.TotalTime;
        }

        /// <summary>
        /// When the audio stops
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private static void OnPlaybackStopped(object? sender, StoppedEventArgs args) {
            StopPlayback();
        }

        /// <summary>
        /// Stop the audio
        /// </summary>
        public static void StopPlayback() {
            playing = false;
            audioFile.Position = 0;
            outputDevice.Stop();
        }

        /// <summary>
        /// Pause the audio
        /// </summary>
        public static void PausePlayback() {
            playing = false;
            outputDevice.Pause();
        }

        /// <summary>
        /// Start the audio
        /// </summary>
        public static void StartPlayback() {
            playing = true;
            outputDevice.Play();
        }

        /// <summary>
        /// Seek to a place in the audio
        /// </summary>
        /// <param name="s"></param>
        public static void Seek(int s) {
            //audioFile.Position = (long)slider1.Value;
            audioFile.CurrentTime = new TimeSpan(
                0,
                0,
                s
            );
        }

        /// <summary>
        /// Get 1024 bytes from current position in audio
        /// </summary>
        /// <returns></returns>
        public static int[] getSample() {


            return new int[1024];
        }
    }
}