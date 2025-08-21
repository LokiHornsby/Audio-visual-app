using FftSharp;
using NAudio.Wave;
using NAudio.Wave.Compression;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Security.Policy;
using System.Text;
using System.Windows;
using static System.Windows.Forms.DataFormats;

namespace Audio_visual_app {
    public static class LAVT { // Loki's audio visual toolkit
        // Audio
        static Mp3FileReader reader;
        static IWavePlayer player;

        // raw data
        public static int duration;
        public static int samplesize = 64;
        public static int chunksize;
        public static data_struct[][] data;
        public static double BPM;
        static WaveFormat format;
        static WaveStream pcmstream;
        static int drl;

        public struct data_struct {
            public double[] sample;
            public double[] powers;
            public double[] frequencies;
            public double[] magnitudes;
            public bool onset;
            public double frequency;

            // constructor
            public data_struct(
                double[] _sample,
                double[] _powers,
                double[] _frequencies,
                double[] _magnitudes,
                bool _onset, 
                double _frequency) {

                sample = _sample;
                powers = _powers;
                frequencies = _frequencies;
                magnitudes = _magnitudes;
                onset = _onset;
                frequency = _frequency;
            }
        }

        // loaded variables
        public static bool initialised = false;
        public static bool analysed = false;

        public static void setData(int size, int sensitivity) {
            if (initialised) {
                analysed = false;

                // data
                chunksize = size;
                data = new data_struct[duration][];

                for (int i = 0; i < data.Length; i++) {
                    // create array of length "size"
                    data[i] = new data_struct[size];

                    for (int j = 0; j < data[i].Length; j++) {
                        // calculate and move to position
                        int pos = (i * drl) + ((j * drl) / data[i].Length);
                        reader.Position = pos;

                        // read bytes
                        byte[] buffer = new byte[drl / size];
                        pcmstream.ReadExactly(buffer, 0, buffer.Length);

                        // convert to pcm
                        double[] pcm = new double[buffer.Length / 4];

                        for (int q = 0; q < pcm.Length; q += 4) {
                            Int16 channel1 = BitConverter.ToInt16(buffer, q);
                            Int16 channel2 = BitConverter.ToInt16(buffer, q + 2); // channel2 is ignored

                            pcm[q / 4] = channel1;
                        }

                        // apply a window to the pcm data
                        var window = new FftSharp.Windows.Hanning();
                        pcm = window.Apply(pcm);

                        // get a sample
                        double[] sample = new double[samplesize];

                        for (int q = 0; q < sample.Length; q++) {
                            sample[q] = pcm[q];
                        }

                        // Perform FFT
                        System.Numerics.Complex[] fft = FFT.Forward(sample);

                        // Perform calculations
                        double[] powers = FFT.Power(fft);
                        double[] frequencies = FFT.FrequencyScale(powers.Length, format.SampleRate);
                        double[] magnitudes = FFT.Magnitude(fft);

                        // Store data
                        data[i][j] = new data_struct(
                            sample,
                            powers,
                            frequencies,
                            magnitudes,
                            powers.Sum() > sensitivity,
                            frequencies[Array.IndexOf(powers, powers.Max())]
                        );

                        // calculate BPM
                        if (i < 60 && data[i][j].onset) BPM += 1;
                    }
                }

                reader.Position = 0;
                analysed = true;
            }
        }

        public static void Initialise(string filename) {
            initialised = false;

            // audio
            reader = new Mp3FileReader(filename);

            // player
            player = new WaveOutEvent();
            player.Init(reader);

            // calculations
            format = new WaveFormat(reader.WaveFormat.SampleRate, reader.WaveFormat.Channels);
            pcmstream = NAudio.Wave.WaveFormatConversionStream.CreatePcmStream(reader);
            drl = format.SampleRate * format.Channels * (format.BitsPerSample / 8);
            duration = (int)reader.Length / drl;

            // loaded
            initialised = true;
        }

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

        public static void seek(int s) {
            reader.Position = drl * s;
        }

        public static void StopPlayback() {
            player.Stop();
            reader.Position = 0;
        }

        public static void PausePlayback() {
            player.Pause();
        }

        public static void StartPlayback() {
            player.Play();
        }
    }
}