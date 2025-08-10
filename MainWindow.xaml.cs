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
        // file name
        string filename = "";

        // active
        bool active = false;

        // timing
        int seconds = 0;
        int milliseconds = 0;
        System.Timers.Timer timerGUI;

        // visuals
        int i;
        int linepos;

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
                // LAVT (Loki's audio visual toolkit)
                LAVT.Initialise(filename);

                // GUI
                slider1.Maximum = LAVT.duration;

                timerGUI = new System.Timers.Timer(100); // 0.1 second
                timerGUI.Elapsed += UpdateGUI;
                timerGUI.Enabled = true;
                timerGUI.AutoReset = true;

                // GUI (update these on a thread)
                //check3.Content = "BPM: " + GetBPM();
                check1.IsEnabled = false;
                check1.IsChecked = false;
                check2.IsEnabled = false;
                check2.IsChecked = true;
                check3.IsEnabled = false;
                check3.IsChecked = true;
                slider1.IsEnabled = false;

                check3.Content = "BPM: " + LAVT.BPM;

                // draw waveform
                canvas1.VerticalAlignment = VerticalAlignment.Bottom;
                canvas1.HorizontalAlignment = HorizontalAlignment.Left;

                for (int i = 0; i < 128; i++) {
                    Line line = new Line();
                    line.StrokeThickness = 1;
                    line.Stroke = System.Windows.Media.Brushes.Black;
                    line.X1 = i;
                    line.X2 = i;
                    line.Y1 = 0;
                    line.Y2 = 15;
                    canvas1.Children.Add(line);
                }
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

        /// <summary>
        /// Update GUI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateGUI(object? sender, EventArgs e) {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                if (active && seconds < LAVT.duration) {
                    // GUI
                    slider1.Value = seconds;
                    label1.Content = seconds + "." + milliseconds + " / " + LAVT.duration;

                    // Select data
                    LAVT.data_struct d = LAVT.data[seconds][milliseconds];

                    // use data
                    check1.IsChecked = d.onset;
                    check1.Content = "Onset: " + check1.IsChecked;
                    check2.Content = "Frequency: " + d.frequency;

                    // get visuals
                    Line[] l = canvas1.Children.OfType<Line>().ToArray();

                    // update each line according to pcm data
                    for (int j = 0; j < 128; j++) {
                        l[j].Y2 = d.pcm[j]; // NO OUTPUT
                    }

                    // increment time
                    milliseconds += 1;
                    if (milliseconds == 10) { milliseconds = 0; seconds += 1; }
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
    }
}