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
    public partial class MainWindow : Window {
        // file
        string filename = "";

        // active
        bool active = false;

        // timing
        int seconds = 0;
        int milliseconds = 0;
        int wait = 100;
        System.Timers.Timer timerGUI;

        // quality
        int quality = 10;

        // visuals
        int i;
        int linepos;

        // loaded
        bool loaded = false;

        void setLoading() {
            loaded = false;

            // Loading
            B1.IsEnabled = false;

            check1.IsEnabled = false;
            check1.IsChecked = false;

            check1.Content = "Loading...";
            check2.IsEnabled = false;
            check2.IsChecked = false;

            check2.Content = "Loading...";
            check3.IsEnabled = false;
            check3.IsChecked = false;

            check3.Content = "Loading...";
            slider1.IsEnabled = false;

            band1.IsEnabled = false;
            band2.IsEnabled = false;
            band3.IsEnabled = false;
            band4.IsEnabled = false;
            band5.IsEnabled = false;

            qslider.Maximum = 200; // 200 milliseconds : 5 data samples
            qslider.Minimum = 20;  // 100 : 10
                                   // 50 : 20
                                   // 20 : 50

            qslider.IsEnabled = true;
            //qslider.IsEnabled = false;
            qslider.IsMoveToPointEnabled = true;

            // draw waveform
            canvas1.Children.Clear();
            canvas1.VerticalAlignment = VerticalAlignment.Bottom;
            canvas1.HorizontalAlignment = HorizontalAlignment.Left;

            for (int i = 0; i < LAVT.samplesize; i++) {
                Line line = new Line();
                line.StrokeThickness = 1;
                line.Stroke = System.Windows.Media.Brushes.Black;
                line.X1 = i;
                line.X2 = i;
                line.Y1 = 0;
                line.Y2 = 15;
                canvas1.Children.Add(line);
            }
        }

        public void setQuality(int q) {
            quality = q;
            LAVT.setData(q);

            wait = 1000 / q;
            setTimer(wait);
        }

        public void setTimer(int ms) {
            if (timerGUI != null) { timerGUI.Stop(); timerGUI = null; }
            timerGUI = new System.Timers.Timer(ms);
            timerGUI.Elapsed += UpdateGUI;
            timerGUI.Enabled = true;
            timerGUI.AutoReset = true;
            timerGUI.Start();
        }

        /// <summary>
        /// Initialise window
        /// </summary>
        public MainWindow(){
            // initialise WPF (XAML)
            InitializeComponent();

            // Select file
            filename = selectfile();

            // If file exists
            if (filename != "") {
                // set GUI to loading
                setLoading();

                // Intialise toolkit
                LAVT.Initialise(filename);
            } else {
                MessageBox.Show("No file selected.", "No file", MessageBoxButton.OK, MessageBoxImage.Warning);
                LAVT.dump();
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
            // stop
            LAVT.StopPlayback();
            B1.Content = "Start";

            // reset
            seconds = 0;
            milliseconds = 0;
        }

        void start() {
            // update
            LAVT.bands[0].Gain = (int)band1.Value;
            LAVT.bands[1].Gain = (int)band2.Value;
            LAVT.bands[2].Gain = (int)band3.Value;
            LAVT.bands[3].Gain = (int)band4.Value;
            LAVT.bands[4].Gain = (int)band5.Value;
            LAVT.equalizer.Update();

            // start
            LAVT.StartPlayback();
            B1.Content = "Stop";
        }

        /// <summary>
        /// Toggle playback state
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Toggle(object sender, RoutedEventArgs e) {
            if (!active) {
                start();
            } else {
                stop();
            }

            active = !active;
        }

        void setLoaded() {
            B1.IsEnabled = true;

            check1.IsChecked = true;
            check1.Content = "Loaded.";
            check2.IsChecked = true;
            check2.Content = "Loaded.";
            check3.IsChecked = true;
            check3.Content = "Loaded.";
            band1.IsEnabled = true;
            band2.IsEnabled = true;
            band3.IsEnabled = true;
            band4.IsEnabled = true;
            band5.IsEnabled = true;

            check3.Content = "BPM: " + LAVT.BPM;
            slider1.Maximum = LAVT.duration;

            qslider.IsEnabled = true;
        }

        /// <summary>
        /// Update GUI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateGUI(object? sender, EventArgs e) {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {

            qslider.Value = quality;

            // Load GUI
            if (LAVT.analysed && !loaded) {
                // Set GUI to loaded
                setLoaded();
                loaded = true;
            }

            // Loaded GUI
            if (loaded) {
                if (seconds < LAVT.duration && active) {
                    // Select data
                    LAVT.data_struct d = LAVT.data[seconds][milliseconds / wait]; // ERROR

                    // use data
                    check1.IsChecked = d.onset;
                    check1.Content = "Onset: " + check1.IsChecked;
                    check2.Content = "Frequency: " + d.frequency;

                    // get visuals
                    Line[] l = canvas1.Children.OfType<Line>().ToArray();

                    // update each line according to pcm data
                    for (int j = 0; j < LAVT.samplesize; j++) {
                        l[j].Y2 = d.sample[j];
                    }

                    // Timing
                    milliseconds += wait;

                    if (milliseconds >= 1000 - wait) {
                        seconds += 1;
                        milliseconds = 0;
                    }

                    // GUI
                    slider1.Value = seconds;
                    label1.Content = seconds + " (" + milliseconds + "ms / 1 second" + ") / " + LAVT.duration;
                } else if (active) {
                    Toggle(null, null);
                }
            }

            }));
        }


        private void band5_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (active) { Toggle(null, null); }
        }

        private void band4_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (active) { Toggle(null, null); }
        }

        private void band3_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (active) { Toggle(null, null); }
        }

        private void band2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (active) { Toggle(null, null); }
        }

        private void band1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (active) { Toggle(null, null); }
        }

        private void qualityvalue(object sender, RoutedPropertyChangedEventArgs<double> e) {
            // load
            if (active) { Toggle(null, null); }
            setLoading();

            // set quality
            var sl = sender as Slider;
            quality = (int)sl.Value;
            
            Thread thread = new Thread(() => {
                setQuality(quality);
            });

            thread.Start();
        }
    }
}