using Microsoft.VisualBasic.Devices;
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
                dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
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

        private void DrawData(double[] data) {
            /*// Clear canvas
            canvas1.Children.Clear();

            // Setup polygon object
            Polygon myPolygon = new Polygon();
            myPolygon.Stroke = System.Windows.Media.Brushes.Black;
            myPolygon.Fill = System.Windows.Media.Brushes.Aquamarine;
            myPolygon.StrokeThickness = 1;
            myPolygon.HorizontalAlignment = HorizontalAlignment.Center;
            myPolygon.VerticalAlignment = VerticalAlignment.Center;

            // Define points
            PointCollection points = new PointCollection();

            // Draw points
            for (int i = 0; i < data.Length; i++) {
                System.Windows.Point p = new System.Windows.Point(i, data[i] / 10000);
                points.Add(p);
            }

            System.Windows.Point a = new System.Windows.Point(data.Length+1, 50);
            points.Add(a);

            System.Windows.Point b = new System.Windows.Point(0, 50);
            points.Add(b);

            // Add points
            myPolygon.Points = points;
            canvas1.Children.Add(myPolygon);*/
        }

        private void enable() {
            audioLoading = false;
            slider1.IsEnabled = true;
            B1.IsEnabled = true;
            B2.IsEnabled = true;
            B3.IsEnabled = true;
            slider1.Value = 0;
        }

        /// <summary>
        /// Update GUI on timer tick
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dispatcherTimer_Tick(object sender, EventArgs e) {
            // current time
            TimeSpan current = GetCurrentTime();

            // Loading
            check1.IsChecked = !audioLoading;
            slider1.Value = current.TotalMilliseconds;

            // If audio is loading
            if (audioLoading) {
                // Percentage label
                label1.Content = ((current.TotalMilliseconds / GetTotalTime().TotalMilliseconds) * 100).ToString("0") + "%";
                
                // loading tag
                check1.Content = "Loading...";
                
                // populate samples
                double[] sample = GetPCMSample(1024);

                for (int i = 0; i < sample.Length; i++) {
                    onset.Add(sample[i] > 0);
                }

                // end of load
                if (GetCurrentTime() == GetTotalTime()) { enable(); }
            } else {
                // playback label
                label1.Content = GetCurrentTime();

                // loaded
                check1.Content = "Loaded.";

                // current index
                int index = (int)current.TotalSeconds;
                check2.IsChecked = onset[index];
            }

            // Get a sample
            //

            // Perform an fft on it
            //System.Numerics.Complex[] FFT = PerformFFT(sample);

            // Check if it's an onset
            //int sensitivity = 10;
            //double[] powers = GetPowers(FFT);
            //check1.IsChecked = powers.Max() > sensitivity;

            /////////////////////////////////////////////////////////////////
            //check1.Content = "power max: " + powers.Max().ToString("0.0");

            // Get sample and perform FFT
            /*if (hasPCMData()) {
                double[] sample = GetPCMSamples()[(int)Math.Floor(GetCurrentTime().TotalSeconds)];
            }*/

            /*System.Numerics.Complex[] FFT = PerformFFT(sample);

            // Power
            double[] powers = GetPowers(FFT);
            //check1.IsChecked = powers.Max() > 0;
            //check1.Content = "power max: " + powers.Max().ToString("0.0");

            // Frequency
            double[] frequencies = GetFrequencies(FFT);
            double frequency = 0;
            frequency = frequencies[Array.IndexOf(powers, powers.Max())];

            check3.IsChecked = true;
            check3.Content = "Frequency: " + frequency.ToString("0.0");*/
            /////////////////////////////////////////////////////////////////
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