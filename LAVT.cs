using FftSharp;
using NAudio.Extras;
using NAudio.Wave;
using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Windows;

namespace Audio_visual_app {
    public static class LAVT { // Loki's audio visual toolkit
        // Audio
        static Mp3FileReader reader;

        public static EqualizerBand[] bands;
        public static Equalizer equalizer;
       
        static IWavePlayer player;

        static WaveFormat format;

        // raw data
        public static WaveStream pcmstream;
        public static int drl;
        public static int duration;
        public static data_struct[][] data;
        public static double BPM; 

        /// <summary>
        /// Initialise LAVT (Loki's audio visual toolkit)
        /// </summary>
        /// <param name="filename"></param>
        public static void Initialise(string filename) {
            // audio
            reader = new Mp3FileReader(filename);

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

            equalizer = new Equalizer(reader.ToSampleProvider(), bands);

            // player
            player = new WaveOutEvent();
            player.Init(equalizer);

            // calculations
            format = new WaveFormat(reader.WaveFormat.SampleRate, reader.WaveFormat.Channels);
            pcmstream = NAudio.Wave.WaveFormatConversionStream.CreatePcmStream(reader);
            drl = format.SampleRate * format.Channels * (format.BitsPerSample / 8);
            duration = (int)reader.Length / drl;

            // data
            data = new data_struct[duration][];

            for (int i = 0; i < data.Length; i++) {
                data[i] = new data_struct[10];
                
                for (int j = 0; j < data[i].Length; j++) {
                    int pos = (i * drl) + ((j * drl) / data[i].Length);
                    data[i][j] = new data_struct(pos);
                    //BPM += data[i].onset.Select(x => x == true).Count();
                }
            }

            //BPM = BPM / duration;

            // reset
            reader.Position = 0;
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
        /// Stop the audio
        /// </summary>
        public static void StopPlayback() {
            player.Stop();
            reader.Position = 0;
        }

        /// <summary>
        /// Start the audio
        /// </summary>
        public static void StartPlayback() {
            player.Play();
        }

        public struct data_struct {
            // Arrays
            public double[] pcm;
            public bool onset;
            public double frequency;

            // constructor
            public data_struct(int _position) {
                // construct
                reader.Position = _position;

                // get a one millisecond sample
                byte[] buffer = new byte[drl / 10];
                pcmstream.ReadExactly(buffer, 0, buffer.Length);

                // convert to pcm
                pcm = new double[buffer.Length / 4];

                int x = 0;

                for (int i = 0; i < pcm.Length; i += 4) {
                    Int16 channel1 = BitConverter.ToInt16(buffer, i);
                    Int16 channel2 = BitConverter.ToInt16(buffer, i + 2); // channel2 is ignored

                    pcm[x] = channel1;
                    x++;
                }

                // apply a window to the pcm data
                var window = new FftSharp.Windows.Hanning();
                pcm = window.Apply(pcm);

                // get sample
                double[] sample = new double[128];

                for (int i = 0; i < sample.Length; i++) {
                    sample[i] = pcm[i];
                }

                // Perform FFT
                System.Numerics.Complex[] fft = FFT.Forward(sample);

                // Perform calculations
                double[] powers = FFT.Power(fft);
                double[] frequencies = FFT.FrequencyScale(powers.Length, format.SampleRate);
                double[] magnitudes = FFT.Magnitude(fft);

                // analysed
                onset = powers.Sum() > powers.Max() / 10;
                frequency = frequencies[Array.IndexOf(powers, powers.Max())];
            }
        }
    }
}