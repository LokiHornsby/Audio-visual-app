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
using System.Windows.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace Audio_visual_app {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        // Audio variables
        static WaveOutEvent? outputDevice;
        static AudioFileReader? audioFile;

        string? filename;

        private bool playing = false;
        DispatcherTimer? dispatcherTimer;

        public MainWindow(){
            InitializeComponent();

            filename = selectfile();
            if (filename != null) { initialise();  }
        }

        string? selectfile() {
            Console.WriteLine("Selecting file...");

            // Configure open file dialog box
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "Audio files|*.wav;*.mp3"; // Filter files by extension

            // Show open file dialog box
            dialog.ShowDialog();

            return dialog.FileName;
        }

        void initialise() {
            // Store PCM Data
            //WaveStream ws = new Mp3FileReader(filename);
            //WaveStream pcm = WaveFormatConversionStream.CreatePcmStream(ws);
            //int interval = pcm.WaveFormat.Channels * pcm.WaveFormat.SampleRate * pcm.WaveFormat.BitsPerSample / 8;

            // Start a timer
            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            dispatcherTimer.Start();

            // initialise audio variables
            outputDevice = new WaveOutEvent();
            outputDevice.PlaybackStopped += OnPlaybackStopped;
            
            audioFile = new AudioFileReader(filename);
            outputDevice.Init(audioFile);

            // set slider
            slider1.Maximum = audioFile.TotalTime.TotalMilliseconds;
        }

        /// <summary>
        /// When playback stops
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
       void OnPlaybackStopped(object? sender, StoppedEventArgs args){
            //outputDevice.Dispose();
            //outputDevice = null;
            //audioFile.Dispose();
            //audioFile = null;

            playing = false;
            audioFile.Position = 0;
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e) {
            if (playing) {
                label1.Content = audioFile.CurrentTime;
                slider1.Value = audioFile.CurrentTime.TotalMilliseconds;
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            //if (playing && audioFile.Position != (long)slider1.Value) {
                //dispatcherTimer.Stop();
                //audioFile.Position = (long)slider1.Value;
                //playing = false;
            //}
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) {
            // Play audio
            outputDevice.Play();
            playing = true;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e) {
            outputDevice.Stop();
            playing = false;
        }

        private void Button_Click_3(object sender, RoutedEventArgs e) {
            outputDevice.Pause();
            playing = false;
        }
    }
}