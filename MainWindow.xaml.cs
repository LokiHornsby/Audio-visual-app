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
    using static LAVT;

    public partial class MainWindow : Window {
        string? filename;
        static DispatcherTimer? dispatcherTimer;

        /// <summary>
        /// Initialise window
        /// </summary>
        public MainWindow(){
            InitializeComponent();

            filename = selectfile();
            if (filename != null) { initialiseNAudio(filename); }

            // Start a timer
            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 0, 1);
            dispatcherTimer.Start();

            // set slider
            slider1.Maximum = GetTotalTime().TotalSeconds;
            slider1.IsMoveToPointEnabled = true;
        }

        /// <summary>
        /// Select a file
        /// </summary>
        /// <returns></returns>
        string? selectfile() {
            Console.WriteLine("Selecting file...");

            // Configure open file dialog box
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "Audio files|*.wav;*.mp3"; // Filter files by extension

            // Show open file dialog box
            dialog.ShowDialog();

            return dialog.FileName;
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

            slider1.Value = 0;
        }

        /// <summary>
        /// Update GUI on timer tick
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dispatcherTimer_Tick(object sender, EventArgs e) {
            label1.Content = StripMilliseconds(GetCurrentTime());
            slider1.Value = GetCurrentTime().TotalSeconds;
        }

        /// <summary>
        /// When the slider changes value
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            Slider slider = sender as Slider;

            // if the the slider isn't equal to current position of the audio
            if (slider1.Value != GetCurrentTime().TotalSeconds) {
                Seek((int)slider1.Value); // seek the audio to the sliders position
            }
        }
        
        /// <summary>
        /// Play
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_1(object sender, RoutedEventArgs e) {
            StartPlayback();
        }

        /// <summary>
        /// Pause
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_2(object sender, RoutedEventArgs e) {
            PausePlayback();
        }

        /// <summary>
        /// Stop
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_3(object sender, RoutedEventArgs e) {
            StopPlayback();
        }

        /// <summary>
        /// Onset
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RadioButton_Checked(object sender, RoutedEventArgs e) {

        }
    }
}