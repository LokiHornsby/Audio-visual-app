using NAudio.Wave;
using System.IO;
using FftSharp;
using System.Windows.Threading;
using System.Security.Policy;

namespace Audio_visual_app {
    public static class LAVT { // Loki's audio visual toolkit
        // Audio variables
        private static WaveOutEvent? outputDevice;
        private static AudioFileReader? audioFileReader;
        private static byte[] PCMdata;
        private static bool playing = false;

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

            if (audioFileReader != null) {
                audioFileReader.Dispose();
                audioFileReader = null;
            }
        }

        private static void OutputDevice_PlaybackStopped(object? sender, StoppedEventArgs e) {
            StopPlayback();
        }

        /// <summary>
        /// Initialise LAVT (Loki's audio visual toolkit)
        /// </summary>
        /// <param name="filename"></param>
        public static void initialiseAudioVisualToolkit(string filename) {
            // output device
            outputDevice = new WaveOutEvent();
            outputDevice.PlaybackStopped += OutputDevice_PlaybackStopped;

            // audio file reader
            audioFileReader = new AudioFileReader(filename);
            outputDevice.Init(audioFileReader);

            // PCM data
            PCMdata = new byte[audioFileReader.Length];
            audioFileReader.Read(PCMdata, 0, PCMdata.Length);//File.ReadAllBytes(audioFileReader.FileName);
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
        /// <param name="s">position to seek to</param>
        public static void Seek(int s) {
            audioFileReader.CurrentTime = new TimeSpan (
                0,
                0,
                0,
                0,
                s
            );
        }

        /// <summary>
        /// Gets a sample of pcm data
        /// </summary>
        /// <param name="ms"></param>
        /// <param name="samplesize"></param>
        /// <returns></returns>
        public static double[] GetPCMSample() {
            // Define sample array
            double[] sample = new double[64];

            // Fill array with pcm data
            for (int i = 0; i < sample.Length; i++) {
                sample[i] = PCMdata[(sample.Length * (int)(GetCurrentTime().TotalMilliseconds / 4)) + i];
            }

            // Return array
            return sample;
        }

        /// <summary>
        /// Peform FFT on input sample
        /// </summary>
        /// <returns></returns>
        public static System.Numerics.Complex[] PerformFFT(double[] signal) {
            // Shape the signal using a Hanning window
            var window = new FftSharp.Windows.Hanning();
            double[] windowed = window.Apply(signal);

            // Calculate the FFT as an array of complex numbers
            System.Numerics.Complex[] spectrum = FFT.Forward(signal);

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
             return FFT.FrequencyScale(GetPowers(spectrum).Length, audioFileReader.WaveFormat.SampleRate);
        }

        /// <summary>
        /// Get onsets in a sample
        /// </summary>
        public static double[] GetOnsets(double[] powerFFT, int sensitivity) {
            double[] onsets = new double[powerFFT.Length];
            int max = (int)powerFFT.Max();

            for (int i = 0; i < onsets.Length; i++) {
                if (powerFFT[i] > max / 2) {
                    onsets[i] = powerFFT[i];
                } else {
                    onsets[i] = 0;
                }
            }

            return onsets;
        }
    }
}