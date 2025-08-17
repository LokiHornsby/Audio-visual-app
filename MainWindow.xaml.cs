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
        // load
        bool loaded;

        // active
        bool active = false;

        // timing
        int refreshrate = 10;
        int seconds = 0;
        int milliseconds = 0;
        System.Timers.Timer timerGUI;

        // quality
        int quality = 116;

        // visuals
        int i;
        int linepos;

        public void setInteract(bool x) {
            F1.IsEnabled = x;
            qslider.IsEnabled = x;
            B1.IsEnabled = x;
            Rslider.IsEnabled = x;
            Pslider.IsEnabled = x;

            canvas1.Children.Clear();

            if (x) {
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
        }

        /// <summary>
        /// Initialise window
        /// </summary>
        public MainWindow() {
            // init
            loaded = false;
            InitializeComponent();
            loaded = true;

            // GUI
            setInteract(false);
            F1.IsEnabled = true;
            Pslider.IsEnabled = false;
            changerefreshrate(null, null);
        }
        
        void stop() {
            // stop
            LAVT.StopPlayback();
            B1.Content = "Start";
        }

        void start() {
            // GUI
            B1.Content = "Stop";

            // start
            LAVT.StartPlayback();
        }

        private void SelectFile(object sender, RoutedEventArgs e) {
            if (active) { Toggle(null, null); }

            // Configure
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "Audio files|*.mp3"; // Filter files by extension

            // Show
            dialog.ShowDialog();

            // file
            string filename = dialog.FileName;

            // If file exists
            if (filename.Length > 0) {
                // THREAD GOES HERE

                // Intialise toolkit
                LAVT.dump();
                LAVT.Initialise(filename);

                // GUI
                var button = sender as Button;
                var s = System.IO.Path.GetFileName(filename);
                if (s.Length > 10) { s = s[..10] + "..."; }
                button.Content = s;
                Pslider.Maximum = LAVT.duration;
                C3.Text = "BPM: " + LAVT.BPM;

                // settings
                qualityvalue(null, null);
            } else {
                MessageBox.Show("No file selected.", "No file", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
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
                // Loaded GUI
                if (LAVT.analysed) {
                    if (seconds < LAVT.duration && active) {
                        // Select data
                        int pos = (int)Math.Round(((quality - 1) / 1000.0) * (double)milliseconds);
                        LAVT.data_struct d = LAVT.data[seconds][pos];

                        // use data
                        C1.Text = "Onset: " + d.onset;
                        C2.Text = "Frequency: " + d.frequency;

                        // get visuals
                        Line[] l = canvas1.Children.OfType<Line>().ToArray();

                        // update each line
                        double[] x = d.magnitudes;
                        double max = x.Max();

                        for (int j = 0; j < x.Length; j++) {
                            var val = (x[j] / max) * 100;
                            if (!Double.IsNormal(val)) { val = 0; }
                            l[j / 2].Y2 = val;
                        }

                        // Timing
                        milliseconds += refreshrate;

                        if (milliseconds >= 1000) {
                            seconds += 1;
                            milliseconds = 0;
                        }

                        // GUI
                        Pslider.Value = seconds;
                        T1.Text = seconds + " (Chunk: " + (int)pos + " / " + quality + ") / " + LAVT.duration;
                    } else if (active) {
                        seconds = 0;
                        milliseconds = 0;
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

        private void qualityvalue(object sender, RoutedPropertyChangedEventArgs<double> e) { // ERROR
            if (loaded) {
                // load
                if (active) { Toggle(null, null); }

                setInteract(false);

                // Quality
                quality = (int)qslider.Value;
                Q1.Text = "Quality: " + quality;
                LAVT.setData(quality); // THREAD GOES HERE

                setInteract(true);
            }
        }

        private void changerefreshrate(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (loaded) {
                // GUI
                refreshrate = (int)Rslider.Value;
                R1.Text = "Refresh rate: " + refreshrate;

                // timer
                if (timerGUI != null) { timerGUI.Stop(); timerGUI = null; }
                timerGUI = new System.Timers.Timer(refreshrate);
                timerGUI.Elapsed += UpdateGUI;
                timerGUI.Enabled = true;
                timerGUI.AutoReset = true;
                timerGUI.Start();
            }
        }

        private void Pslider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (loaded) {
                if (seconds != (int)Pslider.Value) {
                    seconds = (int)Pslider.Value;
                    milliseconds = 0;
                    LAVT.seek(seconds);
                    T1.Text = "Seeked to " + seconds + " / " + LAVT.duration;
                }
            }
        }
    }
}