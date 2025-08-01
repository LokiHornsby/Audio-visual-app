using NAudio.Gui;
using NAudio.Wave;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Audio_visual_app {
    using static LAVT;

    public partial class MainWindow : Window {
        // file name
        string filename;

        // slider
        float seconds;
        DispatcherTimer sliderTimer;
        
        // current index
        int samplepos;

        // visuals
        int i;
        double[] window;
        DispatcherTimer visualTimer;

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
                outputDevice.PlaybackStopped += OutputDevice_PlaybackStopped;

                // configure slider
                slider1.Maximum = reader.TotalTime.TotalSeconds;
                slider1.Value = 0;
                slider1.IsMoveToPointEnabled = false;
                slider1.IsEnabled = false;

                // start timer
                sliderTimer = new System.Windows.Threading.DispatcherTimer();
                sliderTimer.Tick += new EventHandler(UpdateSlider);
                sliderTimer.Interval = new TimeSpan(0, 0, 0, 1, 0, 0);

                // draw waveform
                for (int i = 0; i < sampleSize; i++) {
                    Line line = new Line();
                    line.StrokeThickness = 1;
                    line.Stroke = System.Windows.Media.Brushes.Black;
                    line.X1 = i;
                    line.X2 = i;
                    line.Y1 = 10;
                    line.Y2 = 15;
                    canvas1.Children.Add(line);
                }

                // start waveform visualisation timer
                visualTimer = new System.Windows.Threading.DispatcherTimer();
                visualTimer.Tick += new EventHandler(UpdateVisuals);
                visualTimer.Interval = new TimeSpan(0, 0, 0, 1, 0, 0);
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

        void stop() {
            StopPlayback();
            sliderTimer.Stop();
            visualTimer.Stop();
            B1.Content = "Start";
            slider1.Value = 0;
            i = 0;
            samplepos = 0;
            seconds = 0;

            UpdateSlider(null, null);
            UpdateVisuals(null, null);
        }

        void start() {
            StartPlayback();
            sliderTimer.Start();
            visualTimer.Start();
            B1.Content = "Stop";
        }

        /// <summary>
        /// When the song ends
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OutputDevice_PlaybackStopped(object? sender, StoppedEventArgs e) {
            stop();
        }

        /// <summary>
        /// Toggle playback state
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Toggle(object sender, RoutedEventArgs e) {
            if (!isPlaying()) {
                start();
            } else {
                stop();
            }
        }

        /// <summary>
        /// Timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateSlider(object? sender, EventArgs e) {
            // slider
            slider1.Value = seconds;

            // get time
            label1.Content = seconds;

            // current position
            seconds += 1;
            samplepos += format.SampleRate;
            i = 0;

            // audio calculations
            double[] signal = getSignal((int)samplepos);
            window = getWindow(signal);
            System.Numerics.Complex[] fft = PerformFFT(window);

            // 
            check1.IsChecked = GetMagnitudes(fft).Sum() > 0;
        }

        public void UpdateVisuals(object? sender, EventArgs e) {
            foreach (var c in canvas1.Children.OfType<Line>()) {
                c.Y2 = 15 + window[i];

                if (i != sampleSize - 1) {
                    i++;
                } else {
                    i = 0;
                }
            }
        }
    }
}