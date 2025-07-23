using FftSharp;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.IO;
using System.Security.Policy;
using System.Windows;
using System.Windows.Threading;

namespace Audio_visual_app {
    public static class LAVT { // Loki's audio visual toolkit
        // Audio variables
        private static WaveOutEvent outputDevice;
        private static Mp3FileReader mp3reader;
        private static bool playing = false;
        private static List<byte[]> data; 

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

            // Initialise data list
            data = new List<byte[]>();
        }

        /// <summary>
        /// Get a sample of pcm data
        /// </summary>
        public static double[] GetPCMSample() {
            // creates a pcm stream
            NAudio.Wave.WaveStream pcmstream = NAudio.Wave.WaveFormatConversionStream.CreatePcmStream(mp3reader);
            
            // amount of bytes in a second.
            int second = pcmstream.WaveFormat.Channels * pcmstream.WaveFormat.SampleRate * pcmstream.WaveFormat.BitsPerSample / 8; 
            
            // buffer (millisecond in size)
            byte[] buffer = new byte[second / 60];

            // read ahead by size of buffer
            pcmstream.Read(buffer);

            // define pcm array
            double[] pcm = new double[pcmstream.WaveFormat.BitsPerSample];

            // Convert bytes to floats
            for (int i = 0; i < pcm.Length; i++) {
                pcm[i] = BitConverter.ToDouble(buffer, 0);
            }

            // Reset reader position
            if (GetCurrentTime() != GetTotalTime()) {
                mp3reader.Position -= buffer.Length;
            }

            // Return pcm data
            return pcm;
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