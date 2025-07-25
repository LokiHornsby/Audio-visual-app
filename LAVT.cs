using FftSharp;
using NAudio.Wave;
using NAudio.Wave.Asio;

namespace Audio_visual_app {
    public static class LAVT { // Loki's audio visual toolkit
        // Audio variables
        private static WaveOutEvent outputDevice;
        private static Mp3FileReader mp3reader;
        private static NAudio.Wave.Mp3FileReaderBase basemp3read;

        // Playing?
        private static bool playing = false;

        // PCM data
        private static WaveStream PCMstream;
        private static bool gotPCMData;

        /// <summary>
        /// Initialise LAVT (Loki's audio visual toolkit)
        /// </summary>
        /// <param name="filename"></param>
        public static void initialiseAudioVisualToolkit(string filename) {
            // output device
            outputDevice = new WaveOutEvent();
            outputDevice.PlaybackStopped += OutputDevice_PlaybackStopped;

            // audio file reader
            mp3reader = new Mp3FileReader(filename);
            outputDevice.Init(mp3reader);

            // Get array of pcm samples
            PCMstream = NAudio.Wave.WaveFormatConversionStream.CreatePcmStream(mp3reader);
            gotPCMData = false;
        }

        public static WaveStream GetPCMStream() {
            return PCMstream;
        }

        /// <summary>
        /// Get a byte of data from current position in mp3filereader
        /// </summary>
        private static byte[] GetByte() {         
            byte[] buffer = new byte[16];
            int bytesread = mp3reader.Read(buffer, 0, buffer.Length);
            
            return buffer;
        }

        /// <summary>
        /// Convert a byte to a double (PCM)
        /// </summary>
        public static double GetBytePCM() {
            return BitConverter.ToDouble(GetByte(), 0);
        }

        /// <summary>
        /// Get a collection of PCM samples
        /// </summary>
        public static double[] GetPCMSample(int size) {
            double[] sample = new double[size];

            for (int i = 0; i < size; i++) {
                sample[i] = GetBytePCM();
            }

            return sample;
        }

        public static double[] ReadSecond() {
            WaveStream stream = GetPCMStream();
            return GetPCMSample(stream.WaveFormat.SampleRate / 4); // 1 second
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

            if (mp3reader != null) {
                mp3reader.Dispose();
                mp3reader = null;
            }
        }

        /// <summary>
        /// Get current position of audio
        /// </summary>
        /// <returns></returns>
        public static TimeSpan GetCurrentTime() {
            return mp3reader.CurrentTime;
        }

        /// <summary>
        /// Get total length of the audio
        /// </summary>
        /// <returns></returns>
        public static TimeSpan GetTotalTime() {
            return mp3reader.TotalTime;
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
            mp3reader.Position = 0;
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
            mp3reader.CurrentTime = new TimeSpan (
                0,
                0,
                0,
                0,
                s
            );
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
             return FFT.FrequencyScale(GetPowers(spectrum).Length, mp3reader.WaveFormat.SampleRate);
        }

        /// <summary>
        /// Get onsets in a sample
        /// </summary>
        public static double[] GetOnset(double[] powerFFT, int sensitivity) {
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