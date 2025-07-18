using NAudio.Wave;
using NAudio.Wave.SampleProviders;

/*
namespace Audiovisualapp {
    internal static class Program {
        static WaveOutEvent? outputDevice;
        static AudioFileReader? audioFile;

        static void OnPlaybackStopped(object? sender, StoppedEventArgs args){
            if (outputDevice != null){
                outputDevice.Dispose();
                outputDevice = null;
            }

            if (audioFile != null){
                audioFile.Dispose();
                audioFile = null;
            }
        }

        [STAThread]
        static void Main(string[] args){
            using (Form form = new Form()){
                form.Text = "LAVT"; // Loki's audiovisual toolkit

                // File select button
                Button file_b = new Button();
                file_b.Text = "File";
                form.Controls.Add(file_b);
                file_b.Click += new System.EventHandler(select);

                // File select button
                Button cur_b = new Button();
                cur_b.Text = "current";
                form.Controls.Add(cur_b);

                // Button respond
                void select(object? sender, EventArgs e){
                    Console.WriteLine("Selecting file...");

                    // Show form
                    var openFileDialog = new OpenFileDialog();
                    openFileDialog.Filter = "Audio files|*.mp3;*.wav";
                    var result = openFileDialog.ShowDialog();
                    Console.WriteLine("Selected " + openFileDialog.FileName);

                    // Play file via NAudio
                    if (outputDevice == null){
                        outputDevice = new WaveOutEvent();
                        outputDevice.PlaybackStopped += OnPlaybackStopped;
                    }

                    if (audioFile == null){
                        audioFile = new AudioFileReader(openFileDialog.FileName);
                        outputDevice.Init(audioFile);
                    }

                    outputDevice.Play();

                    Console.WriteLine(audioFile.CurrentTime / audioFile.TotalTime + "%");

                    // Store PCM Data
                    WaveStream ws = new Mp3FileReader(openFileDialog.FileName);
                    WaveStream pcm = WaveFormatConversionStream.CreatePcmStream(ws);
                    int interval = pcm.WaveFormat.Channels * pcm.WaveFormat.SampleRate * pcm.WaveFormat.BitsPerSample/8;
                }

                form.ShowDialog();
            }
        }
    }
}*/