using FftSharp;
using NAudio.Wave;
using System.IO;
using System.IO.Pipes;
using System.Text;

namespace Audio_visual_app {
    public static class LAVT { // Loki's audio visual toolkit
        // Audio variables
        public static WaveOutEvent outputDevice;
        public static Mp3FileReader reader;
        public static WaveFormat format;
        public static int sampleSize = 1024;

        // raw data
        static short[] pcm;

        // Playing?
        private static bool playing = false;

        /// <summary>
        /// Initialise LAVT (Loki's audio visual toolkit)
        /// </summary>
        /// <param name="filename"></param>
        public static void initialiseAudioVisualToolkit(string filename) {
            // output device
            outputDevice = new WaveOutEvent();

            // audio file reader
            reader = new Mp3FileReader(filename);
            reader.ToSampleProvider();
            outputDevice.Init(reader);

            // format
            format = new WaveFormat(reader.WaveFormat.SampleRate, reader.WaveFormat.Channels);

            byte[] buffer = new byte[reader.Length];
            int read = reader.Read(buffer, 0, buffer.Length);

            // sample
            pcm = new short[read / 2];
            Buffer.BlockCopy(buffer, 0, pcm, 0, read);
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
        

        /// <summary>
        /// Stop the audio
        /// </summary>
        public static void StopPlayback() {
            playing = false;
            reader.Position = 0;
            outputDevice.Stop();
        }

        /// <summary>
        /// Start the audio
        /// </summary>
        public static void StartPlayback() {
            playing = true;
            reader.Position = 0;
            outputDevice.Play();
        }

        public static double[] getSignal(int offset) {
            double[] signal = new double[sampleSize];

            for (int i = 0; i < signal.Length; i++) {
                signal[i] = (double)pcm[offset + i];
            }

            return signal;
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
             return FFT.FrequencyScale(GetPowers(spectrum).Length, format.SampleRate);
        }
    }
}