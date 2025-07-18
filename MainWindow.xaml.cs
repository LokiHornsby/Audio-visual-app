using NAudio.Wave;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Audio_visual_app {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow(){
            InitializeComponent();
        }

        // Audio variables
        static WaveOutEvent? outputDevice;
        static AudioFileReader? audioFile;
        string filename;

        /// <summary>
        /// When playback stops
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
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

        /// <summary>
        /// Get current time of song
        /// </summary>
        /// <param name="af"></param>
        /// <returns></returns>
        private double get_time(AudioFileReader af) {
            return af.CurrentTime / af.TotalTime;
        }

        /// <summary>
        /// When the button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e){
            Console.WriteLine("Selecting file...");

            // Configure open file dialog box
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "Audio files|*.wav;*.mp3"; // Filter files by extension

            // Show open file dialog box
            bool? result = dialog.ShowDialog();

            // Process open file dialog box results
            if (result == true) {
                // Open document
                filename = dialog.FileName;
            }

            // Play file via NAudio
            if (outputDevice == null){
                outputDevice = new WaveOutEvent();
                outputDevice.PlaybackStopped += OnPlaybackStopped;
            }

            if (audioFile == null){
                audioFile = new AudioFileReader(filename);
                outputDevice.Init(audioFile);
            }

            // Play audio
            outputDevice.Play();

            // Get current time
            get_time(audioFile);

            // Store PCM Data
            WaveStream ws = new Mp3FileReader(filename);
            WaveStream pcm = WaveFormatConversionStream.CreatePcmStream(ws);
            int interval = pcm.WaveFormat.Channels * pcm.WaveFormat.SampleRate * pcm.WaveFormat.BitsPerSample / 8;
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {

        }
    }
}