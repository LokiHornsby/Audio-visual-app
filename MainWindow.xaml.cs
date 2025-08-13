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
        int seconds = 0;
        int milliseconds = 0;
        int wait = 100;
        System.Timers.Timer timerGUI;

        // quality
        int quality = 100;

        // visuals
        int i;
        int linepos;

        public void setInteract(bool x) {
            F1.IsEnabled = x;
            qslider.IsEnabled = x;
            Pslider.IsEnabled = x;
            B1.IsEnabled = x;

            /*canvas1.Children.Clear();

            band1.IsEnabled = x;
            band2.IsEnabled = x;
            band3.IsEnabled = x;
            band4.IsEnabled = x;
            band5.IsEnabled = x;

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
            }*/
        }

        /// <summary>
        /// Initialise window
        /// </summary>
        public MainWindow() {
            // init
            loaded = false;
            InitializeComponent();
            loaded = true;

            // setup GUI
            setInteract(false);

            F1.IsEnabled = true;
            
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
            /*LAVT.bands[0].Gain = (int)band1.Value;
            LAVT.bands[1].Gain = (int)band2.Value;
            LAVT.bands[2].Gain = (int)band3.Value;
            LAVT.bands[3].Gain = (int)band4.Value;
            LAVT.bands[4].Gain = (int)band5.Value;
            LAVT.equalizer.Update();*/

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
                        LAVT.data_struct d = LAVT.data[seconds][(milliseconds / 1000) * quality]; // ERROR

                        // use data
                        //check1.IsChecked = d.onset;
                        //check1.Content = "Onset: " + check1.IsChecked;
                        //check2.Content = "Frequency: " + d.frequency;

                        // get visuals
                        /*Line[] l = canvas1.Children.OfType<Line>().ToArray();

                        // update each line according to pcm data
                        for (int j = 0; j < LAVT.samplesize; j++) {
                            l[j].Y2 = d.sample[j];
                        }*/

                        // Timing
                        milliseconds += wait;

                        if (milliseconds >= 1000 - wait) {
                            seconds += 1;
                            milliseconds = 0;
                        }

                        // GUI
                        Pslider.Value = seconds;
                        T1.Text = seconds + " (" + milliseconds + "ms / 1 second" + ") / " + LAVT.duration;
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

        private void qualityvalue(object sender, RoutedPropertyChangedEventArgs<double> e) { // ERROR
            if (loaded) {
                // load
                if (active) { Toggle(null, null); }

                // GUI
                quality = (int)qslider.Value;
                Q1.Text = "Quality: " + quality;

                // THREAD GOES HERE

                // quality
                quality = quality;
                LAVT.setData(quality);

                // timer
                wait = 1000 / quality;
                if (timerGUI != null) { timerGUI.Stop(); timerGUI = null; }
                timerGUI = new System.Timers.Timer(wait);
                timerGUI.Elapsed += UpdateGUI;
                timerGUI.Enabled = true;
                timerGUI.AutoReset = true;
                timerGUI.Start();

                setInteract(true);
            }
        }
    }
}