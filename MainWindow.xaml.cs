using Microsoft.VisualBasic.Devices;
using NAudio.Wave.Compression;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Audio_visual_app {
    using static LAVT;

    public partial class MainWindow : Window {
        // file name
        string filename;

        // timer
        static DispatcherTimer dispatcherTimer;

        // audio is loading
        bool audioLoading;

        // events
        public List<bool> onset = new List<bool>();

        /// <summary>
        /// Initialise window
        /// </summary>
        public MainWindow(){
            // initialise WPF
            InitializeComponent();

            // Select file
            filename = selectfile();

            // If file exists
            if (filename != "") {
                // LAVT (Loki's audio visual toolkit)
                initialiseAudioVisualToolkit(filename);

                // Start a timer
                dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
                dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
                dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 0, 1);
                dispatcherTimer.Start();

                // set slider
                slider1.Maximum = GetTotalTime().TotalMilliseconds;
                slider1.Minimum = 0;
                slider1.IsMoveToPointEnabled = true;

                // disable UI
                slider1.IsEnabled = false;
                B1.IsEnabled = false;
                B2.IsEnabled = false;
                B3.IsEnabled = false;
                check1.IsEnabled = false;
                check1.Content = "Loading...";
                check2.IsEnabled = false;
                check3.IsEnabled = false;
         
                // audio
                audioLoading = true;
            } else {
                MessageBox.Show("No file selected.", "No file", MessageBoxButton.OK, MessageBoxImage.Warning);
                dump();
                Application.Current.Shutdown();
            }
        }

        /// <summary>
        /// Select a file
        /// </summary>
        /// <returns></returns>
        string? selectfile() {
            Console.WriteLine("Selecting file...");

            // Configure open file dialog box
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "Audio files|*.mp3"; // Filter files by extension

            // Show open file dialog box
            dialog.ShowDialog();

            return dialog.FileName;
        }

        private void enable() {
            audioLoading = false;
            slider1.Value = 0;
            slider1.IsEnabled = true;
            B1.IsEnabled = true;
            B2.IsEnabled = true;
            B3.IsEnabled = true;
            check1.IsChecked = true;
            check1.Content = "Loaded.";
        }

        /// <summary>
        /// Update GUI on timer tick
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dispatcherTimer_Tick(object sender, EventArgs e) {
            // current time
            TimeSpan current = GetCurrentTime();

            // Slider
            slider1.Value = current.TotalMilliseconds;

            // Playback label
            label1.Content = GetCurrentTime();

            // If audio is loading
            if (audioLoading) {
                if (GetCurrentTime() != GetTotalTime()) {
                    double[] second = ReadSecond();
                    double[] sample = new double[1024];

                    for (int i = 0; i < sample.Length; i++) {
                        sample[i] = second[i];
                    }

                    System.Numerics.Complex[] FFT = PerformFFT(sample);
                    double[] mags = GetMagnitudes(FFT);

                    onset.Add(mags.Max() > 0);
                } else {
                    // end of load
                    enable();
                }
            } else {
                // current index
                int index = (int)(current.TotalSeconds);
                check2.IsChecked = onset[index];
                check2.Content = (index).ToString() + "/" + onset.Count.ToString();
                check3.Content = GetPCMStream().WaveFormat.BitsPerSample;
            }
        }

        /// <summary>
        /// When the slider changes value
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            Slider slider = sender as Slider;

            // if the the slider isn't equal to current position of the audio
            if (slider1.Value != GetCurrentTime().TotalMilliseconds && !audioLoading) {
                Seek((int)slider1.Value); // seek the audio to the sliders position
            }
        }
        
        /// <summary>
        /// Play
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_1(object sender, RoutedEventArgs e) {
            if (!audioLoading) { 
                StartPlayback(); 
            }
        }

        /// <summary>
        /// Pause
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_2(object sender, RoutedEventArgs e) {
            if (!audioLoading) {
                PausePlayback();
            }
        }

        /// <summary>
        /// Stop
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_3(object sender, RoutedEventArgs e) {
            if (!audioLoading) {
                StopPlayback();
            }
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e) {

        }

        

        private void check1_Checked(object sender, RoutedEventArgs e) {

        }
    }
}