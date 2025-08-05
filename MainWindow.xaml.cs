using NAudio.Extras;
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
        string filename = "";

        // timers
        DispatcherTimer timerTime = new System.Windows.Threading.DispatcherTimer();
        DispatcherTimer timerGUI = new System.Windows.Threading.DispatcherTimer();

        // slider
        float seconds = 0f;

        // current index
        float samplepos = 0f;

        // visuals
        int i = 0;

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
                player.PlaybackStopped += OutputDevice_PlaybackStopped;

                // checks
                check2.IsChecked = true;

                check3.Content = "BPM: " + GetBPM();
                check3.IsChecked = true;

                check1.IsEnabled = false;
                check2.IsEnabled = false;
                check3.IsEnabled = false;

                // configure slider
                slider1.Maximum = reader.TotalTime.TotalSeconds;
                slider1.Value = 0;
                slider1.IsMoveToPointEnabled = false;
                slider1.IsEnabled = false;

                // timers
                timerTime.Tick += new EventHandler(UpdateTime);
                timerTime.Interval = new TimeSpan(0, 0, 0, 1, 0, 0); // seconds / 100 = 10 milliseconds

                timerGUI.Tick += new EventHandler(UpdateGUI);
                timerGUI.Interval = new TimeSpan(0, 0, 0, 0, 0, 1); // seconds / 100 = 10 milliseconds
                timerGUI.Start();

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
            timerTime.Stop();
            B1.Content = "Start";
            slider1.Value = 0;
            i = 0;
            samplepos = 0;
            seconds = 0;
        }

        void start() {
            bands[0].Gain = (int)band1.Value;
            bands[1].Gain = (int)band2.Value;
            bands[2].Gain = (int)band3.Value;
            bands[3].Gain = (int)band4.Value;
            bands[4].Gain = (int)band5.Value;
            equalizer.Update();

            UpdateSamples();

            StartPlayback();
            timerTime.Start();
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
            if (!isPlaying) {
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
        private void UpdateTime(object? sender, EventArgs e) {
            // current position
            seconds += 1f;
            samplepos = format.SampleRate * seconds;
        }

        /// <summary>
        /// Update GUI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateGUI(object? sender, EventArgs e) {
            // slider
            slider1.Value = seconds;

            // get time
            label1.Content = (int)seconds + " : " + samplepos;

            // audio calculations
            double[] signal = Array.ConvertAll(
                pcm.Skip((int)samplepos).Take(sampleSize).ToArray(),
                x => (double)x
            );

            double[] window = getWindow(signal);
            System.Numerics.Complex[] fft = PerformFFT(window);
            double[] magnitudes = GetMagnitudes(fft);

            // onset
            check1.IsChecked = magnitudes.Sum() > 0;
            check1.Content = "Onset: " + check1.IsChecked;

            // frequency
            check2.Content = "Frequency: " + "?";

            // Visuals
            i = 0;

            foreach (var c in canvas1.Children.OfType<Line>()) {
                c.Y2 = 15 + window[i];

                if (i < sampleSize - 1) {
                    i++;
                }
            }
        }

        private void band5_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (isPlaying) { stop(); }
        }

        private void band4_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (isPlaying) { stop(); }
        }

        private void band3_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (isPlaying) { stop(); }
        }

        private void band2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (isPlaying) { stop(); }
        }

        private void band1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (isPlaying) { stop(); }
        }
    }
}