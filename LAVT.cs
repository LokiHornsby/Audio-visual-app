using FftSharp;
using NAudio.Wave;
using System.IO;
using System.IO.Pipes;
using System.Text;

namespace Audio_visual_app {
    public static class LAVT { // Loki's audio visual toolkit
        // Audio variables
        private static WaveOutEvent outputDevice;
        private static Mp3FileReader reader;

        // arrays
        public static short[] pcm;
        public static double[][] samples;
        public static double[][] windows;
        public static System.Numerics.Complex[][] ffts;
        public static double[][] frequencies;
        public static double[][] magnitudes;
        public static double[][] powers;

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

            byte[] buffer = new byte[getReader().Length];
            int read = getReader().Read(buffer, 0, buffer.Length);

            // sample
            pcm = new short[read / 2];
            Buffer.BlockCopy(buffer, 0, pcm, 0, read);

            // samples
            samples = new double[(int)(pcm.Length / getSampleSize())][];

            for (int j = 0; j < pcm.Length / getSampleSize(); j++) {
                // define a sample
                samples[j] = new double[getSampleSize()];

                // fill it with data
                for (int i = 0; i < samples[j].Length; i++) {
                    samples[j][i] = pcm[(getSampleSize() * j) + i];
                }
            }

            // populate with data
            windows = new double[samples.Length][];
            ffts = new System.Numerics.Complex[samples.Length][];
            frequencies = new double[samples.Length][];
            magnitudes = new double[samples.Length][];
            powers = new double[samples.Length][];

            for (int i = 0; i < samples.Length; i++) {
                windows[i] = getWindow(samples[i]);
                ffts[i] = PerformFFT(windows[i]);
                frequencies[i] = GetFrequencies(ffts[i]);
                magnitudes[i] = GetMagnitudes(ffts[i]);
                powers[i] = GetPowers(ffts[i]);
            }
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

        /// <summary>
        /// Get sample size
        /// </summary>
        /// <returns></returns>
        public static int getSampleSize() {
            return 16;
        }

        /// <summary>
        /// Get reader
        /// </summary>
        /// <returns></returns>
        public static Mp3FileReader getReader() {
            return reader;
        }

        /// <summary>
        /// Is value a power of two?
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static bool IsPowerOfTwo(ulong x) {
            return (x != 0) && ((x & (x - 1)) == 0);
        }

        /// <summary>
        /// Get window using input signal
        /// </summary>
        /// <param name="signal"></param>
        /// <returns></returns>
        public static double[] getWindow(double[] signal) {
            // Shape the signal using a Hanning window
            var window = new FftSharp.Windows.Hanning();
            return window.Apply(signal);
        }

        /// <summary>
        /// Peform FFT on input signal
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