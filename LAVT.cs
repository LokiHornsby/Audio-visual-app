using FftSharp;
using NAudio.Wave;
using System.IO;
using System.IO.Pipes;

namespace Audio_visual_app {
    public static class LAVT { // Loki's audio visual toolkit
        // Audio variables
        private static WaveOutEvent outputDevice;
        private static Mp3FileReader reader;

        // Naudio extras plugin
        private static IWavePlayer playbackDevice;
        private static WaveStream fileStream;
        public static double[] data;

        // Playing?
        private static bool playing = false;

        /// <summary>
        /// Initialise LAVT (Loki's audio visual toolkit)
        /// </summary>
        /// <param name="filename"></param>
        public static void initialiseAudioVisualToolkit(string filename) {
            // output device
            outputDevice = new WaveOutEvent();
            outputDevice.PlaybackStopped += OutputDevice_PlaybackStopped;

            // audio file reader
            reader = new Mp3FileReader(filename);
            outputDevice.Init(reader);

            /*var inputStream = new AudioFileReader(filename);
            fileStream = inputStream;
            var aggregator = new SampleAggregator(inputStream);
            aggregator.NotificationCount = inputStream.WaveFormat.SampleRate / 100;
            aggregator.PerformFFT = true;
            aggregator.FftCalculated += FftCalculated;
            aggregator.MaximumCalculated += MaximumCalculated;
            playbackDevice.Init(aggregator);*/
        }

        public static bool GetOnset(double[] sample) {
            return (sample.Sum() > 0);
        }

        /// <summary>
        /// Is the audio playing?
        /// </summary>
        /// <returns></returns>
        public static bool isPlaying() {
            return playing;
        }

        /// <summary>
        /// Dump audio variables
        /// </summary>
        public static void dump() {
            if (outputDevice != null) {
                outputDevice.Dispose();
                outputDevice = null;
            }

            if (reader != null) {
                reader.Dispose();
                reader = null;
            }
        }


        /// <summary>
        /// Get total length of the audio
        /// </summary>
        /// <returns></returns>
        public static TimeSpan GetTotalTime() {
            return reader.TotalTime;
        }

        /// <summary>
        /// when audio is finished playing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OutputDevice_PlaybackStopped(object? sender, StoppedEventArgs e) {
            StopPlayback();
        }

        /// <summary>
        /// Stop the audio
        /// </summary>
        public static void StopPlayback() {
            playing = false;
            reader.Position = 0;
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

        public static long getReadPosition() {
            return reader.Position;
        }

        public static long getReadLength() {
            return reader.Length;
        }

        public static int getSampleSize() {
            return 1024;
        }

        public static int getSampleReadLength() {
            return (int)(getReadLength() / getSampleSize());
        }

        public static int getSampleReadPosition() {
            return (int)(getReadPosition() / getSampleSize());
        }

        public static Mp3FileReader getReader() {
            return reader;
        }

        public static int GetTotalMilliseconds() {
            return (int)Math.Floor(reader.TotalTime.TotalMilliseconds);
        }

        public static int GetCurrentMilliseconds() {
            return (int)Math.Floor(reader.CurrentTime.TotalMilliseconds);
        }

        /// <summary>
        /// Seek to a place in the audio
        /// </summary>
        /// <param name="s">position to seek to</param>
        public static void SeekMilliseconds(int s) {
            reader.CurrentTime = new TimeSpan (
                0,
                0,
                0,
                0,
                s
            );
        }

        public static void setReadPosition(int s) {
            reader.Position = s;
        }

        public static bool IsPowerOfTwo(ulong x) {
            return (x != 0) && ((x & (x - 1)) == 0);
        }

        public static double[] getWindow(double[] signal) {
            // Shape the signal using a Hanning window
            var window = new FftSharp.Windows.Hanning();
            return window.Apply(signal);
        }

        /// <summary>
        /// Peform FFT on input sample
        /// </summary>
        /// <returns></returns>
        public static System.Numerics.Complex[] PerformFFT(double[] signal) {
            // Calculate the FFT as an array of complex numbers
            System.Numerics.Complex[] spectrum = FFT.Forward(signal);

            // return spectrum
            return spectrum;
        }

        /// <summary>
        /// Get FFT magnitudes
        /// </summary>
        /// <param name="spectrum"></param>
        /// <returns></returns>
        public static double[] GetMagnitudes(System.Numerics.Complex[] spectrum) {
            return FFT.Magnitude(spectrum);
        }

        /// <summary>
        /// Get FFT powers
        /// </summary>
        /// <param name="spectrum"></param>
        /// <returns></returns>
        public static double[] GetPowers(System.Numerics.Complex[] spectrum) {
            return FFT.Power(spectrum);
        }

        /// <summary>
        /// Get FFT frequencies
        /// </summary>
        /// <param name="spectrum"></param>
        /// <returns></returns>
        public static double[] GetFrequencies(System.Numerics.Complex[] spectrum) {
             return FFT.FrequencyScale(GetPowers(spectrum).Length, reader.WaveFormat.SampleRate);
        }
    }
}