using NAudio.Dmo.Effect;
using NAudio.FileFormats.Mp3;
using NAudio.Wave;
using System.IO;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Audio_visual_app {
    public static class LAVT { // Loki's audio visual toolkit
        // Audio variables
        private static WaveOutEvent? outputDevice;
        private static AudioFileReader? audioFileReader;
        private static byte[] PCMdata;
        private static bool playing = false;

        /// <summary>
        /// Dump audio variables
        /// </summary>
        public static void dump() {
            if (outputDevice != null) {
                outputDevice.Dispose();
                outputDevice = null;
            }

            if (audioFileReader != null) {
                audioFileReader.Dispose();
                audioFileReader = null;
            }
        }

        /// <summary>
        /// Initialise LAVT (Loki's audio visual toolkit)
        /// </summary>
        /// <param name="filename"></param>
        public static void initialiseAudioVisualToolkit(string filename) {
            // output device
            outputDevice = new WaveOutEvent();
            outputDevice.PlaybackStopped += OnPlaybackStopped;

            // audio file reader
            audioFileReader = new AudioFileReader(filename);
            outputDevice.Init(audioFileReader);

            // PCM data
            PCMdata = File.ReadAllBytes(audioFileReader.FileName);
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
            return audioFileReader.CurrentTime;
        }

        /// <summary>
        /// Get total length of the audio
        /// </summary>
        /// <returns></returns>
        public static TimeSpan GetTotalTime() {
            return audioFileReader.TotalTime;
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
            audioFileReader.Position = 0;
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
            audioFileReader.CurrentTime = new TimeSpan(
                0,
                0,
                s
            );
        }

        /// <summary>
        /// Get a second of PCM data
        /// </summary>
        /// <returns></returns>
        public static double[] GetPCMSample() {
            double current = GetTotalTime().TotalMilliseconds;

            // Get starting position
            int sampleSize = (int)(PCMdata.Length / current);

            // 1 second Sample of PCM data
            double[] sample = new double[sampleSize];

            // Fill array with PCM data
            for (int i = 0; i < sample.Length; i++) {
                sample[i] = PCMdata[(sampleSize * (int)GetCurrentTime().TotalMilliseconds) + i];
            }

            // Return array
            return sample;
        }

        /// <summary>
        /// Peform FFT on input sample
        /// </summary>
        /// <returns></returns>
        public static byte[] FFT(byte[] input) {
            return new byte[1024];
        }
    }
}