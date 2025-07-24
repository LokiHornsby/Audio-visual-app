using FftSharp;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.ApplicationServices;
using Microsoft.VisualBasic.Devices;
using NAudio.Wave;
using NAudio.Wave.Compression;
using NAudio.Wave.SampleProviders;
using System;
using System.IO;
using System.Security.Policy;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using static System.Net.Mime.MediaTypeNames;

namespace Audio_visual_app {
    public static class LAVT { // Loki's audio visual toolkit
        // Audio variables
        private static WaveOutEvent outputDevice;
        private static Mp3FileReader mp3reader;
        
        // Playing?
        private static bool playing = false;

        // PCM data
        private static WaveStream PCMstream;
        private static double[][] PCMSamples;
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
            GetPCMArray();
            
        }

        public static bool hasPCMData() {
            return gotPCMData && GetCurrentTime().TotalSeconds < GetTotalTime().TotalSeconds;
        }

        public static double[][] GetPCMSamples() {
            return PCMSamples;
        }

        private static byte[] GetByte() {         
            byte[] buffer = new byte[8];
            PCMstream.Read(buffer);
            
            return buffer;
        }

        public static double GetBytePCM() {
            return BitConverter.ToDouble(GetByte(), 0);
        }

        public static double[] GetPCMSample(int size) {
            double[] sample = new double[size];

            for (int i = 0; i < size; i++) {
                sample[i] = GetBytePCM();
            }

            return sample;
        }

        /// <summary>
        /// Get a sample of pcm data
        /// </summary>
        private static void GetPCMArray() {
            // convert the byte array to a single double
            

            /*
            // amount of bytes that make up a second.
            int bytesecond = (PCMstream.WaveFormat.Channels * PCMstream.WaveFormat.SampleRate * PCMstream.WaveFormat.BitsPerSample);
            
            // amount of doubles that make up a second
            int doublesecond = bytesecond / 8;

            // amount of seconds available
            int seconds = (int)Math.Floor(GetTotalTime().TotalSeconds);

            // Full pcm array
            double[][] samples = new double[seconds][];

            */

            // DEBUG; raw bytes
            //byte[] audiobytes = new byte[seconds * bytesecond];
            //int f = 0;

            // index through each second
            /*for (int i = 0; i < samples.Length; i++) {
                //if (GetCurrentTime().TotalSeconds != GetTotalTime().TotalSeconds) {
                    // converted bytes to pcm array (1 second worth of data)
                    double[] convertedtoPCM = new double[doublesecond];

                    // list through a seconds worth of byte data
                    for (int j = 0; j < convertedtoPCM.Length; j++) {
                
                        

                        // Store the value
                        convertedtoPCM[j] = val;

                        // DEBUG
                        /*for (int k = 0; k < buffer.Length; k++) {
                            audiobytes[f] = buffer[k];
                            f++;
                        }
                    }

                    // save converted byte array to samples
                    samples[i] = convertedtoPCM;
                //}*/

            // DEBUG; writes bytes back to wav file
            //WaveFormat waveFormat = new WaveFormat(pcmstream.WaveFormat.SampleRate, pcmstream.WaveFormat.BitsPerSample, pcmstream.WaveFormat.Channels);
            //using (WaveFileWriter writer = new WaveFileWriter(
            //    "C:\\Users\\mydre\\OneDrive\\Documents\\Audio-visual-app\\Test\\test.wav", waveFormat)) {
            //    writer.WriteData(audiobytes, 0, audiobytes.Length);
            //}

            // PCM variables
            //PCMSamples = samples;
            //gotPCMData = true;

            // Reset audio file reader position
            //mp3reader.Position = 0;
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