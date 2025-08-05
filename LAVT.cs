using FftSharp;
using NAudio.Wave;
using System.IO;
using System.IO.Pipes;
using System.Text;
using NAudio.Extras;

namespace Audio_visual_app {
    public static class LAVT { // Loki's audio visual toolkit
        // Audio variables
        public static IWavePlayer player;
        public static Mp3FileReader reader;
        public static WaveFormat format;
        public static int sampleSize = 64;
        public static bool isPlaying = false;

        // equalizer
        public static Equalizer equalizer;
        public static EqualizerBand[] bands;
        public static bool equalizerinit;

        // raw data
        public static short[] pcm = new short[sampleSize];

        public static void initEqualizer() {
            // equalizer
            bands = new EqualizerBand[5];

            for (int i = 0; i < 5; i++) {
                bands[i] = new EqualizerBand();
                bands[i].Bandwidth = 0.5f;
                bands[i].Gain = 0f;
            }

            bands[0].Frequency = 200;
            bands[1].Frequency = 400;
            bands[2].Frequency = 1200;
            bands[3].Frequency = 4800;
            bands[4].Frequency = 9600;

            equalizerinit = true;
        }

        public static void UpdateSamples() {
            // read
            byte[] buffer = new byte[reader.Length];
            // int read = equalizer.Read(buffer, 0, buffer.Length);
            int read = reader.Read(buffer, 0, buffer.Length);

            // sample
            pcm = new short[read];
            Buffer.BlockCopy(buffer, 0, pcm, 0, read);

            // reset
            reader.Position = 0;
        }

        /// <summary>
        /// Initialise LAVT (Loki's audio visual toolkit)
        /// </summary>
        /// <param name="filename"></param>
        public static void initialiseAudioVisualToolkit(string filename) {
            if (!equalizerinit) { initEqualizer(); }

            // audio
            reader = new Mp3FileReader(filename);
            equalizer = new Equalizer(reader.ToSampleProvider(), bands);
            player = new WaveOutEvent();
            player.Init(equalizer);

            // format
            format = new WaveFormat(reader.WaveFormat.SampleRate, reader.WaveFormat.Channels);

            // samples
            UpdateSamples();
        }

        /// <summary>
        /// Dump audio variables
        /// </summary>
        public static void dump() {
            if (player != null) {
                player.Dispose();
                player = null;
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
            isPlaying = false;
            reader.Position = 0;
            player.Stop();
        }

        /// <summary>
        /// Start the audio
        /// </summary>
        public static void StartPlayback() {
            isPlaying = true;
            reader.Position = 0;
            player.Play();
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

        public static float GetBPM() {
            int bpm = 0;
            double[] buffer = new double[sampleSize];
            int offset = 0;
            float e = 1.5f;

            for (int i = 0; i < pcm.Length; i++) {
                if (offset < format.SampleRate * 15) {
                    if (i - offset < sampleSize - 1) {
                        buffer[i - offset] = pcm[i];
                    } else {
                        if (GetPowers(PerformFFT(buffer)).Sum() > e) { bpm += 1; }
                        offset = i;
                    }
                }
            }

            return bpm;
        }
    }
}