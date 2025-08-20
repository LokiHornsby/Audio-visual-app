using NAudio.Gui;
using NAudio.Wave;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Audio_visual_app {
    public static class Data {
        static LAVT.data_struct GetData(MainWindow m) {
            return LAVT.data[m.seconds][m.pos];
        }
    }

    public partial class MainWindow : Window {
        // load
        bool loaded;

        // active
        bool active = false;

        // timing
        int refreshrate = 10;
        public int seconds = 0;
        public int pos;
        int milliseconds = 0;
        System.Timers.Timer timerGUI;

        // quality
        int quality = 20;

        // sensitivity
        int sensitivity = 0;

        void startup() {
            // init
            loaded = false;
            InitializeComponent();
            loaded = true;

            // GUI
            setInteract(false);
            F1.IsEnabled = true;
        }

        public MainWindow() {
            startup();
        }

        public void setInteract(bool x) {
            F1.IsEnabled = x;
            qslider.IsEnabled = x;
            Rslider.IsEnabled = x;
            Sslider.IsEnabled = x;
            CH1.IsEnabled = x;
            Tslider.IsEnabled = x;
            B1.IsEnabled = x;
            B2.IsEnabled = x;

            canvas1.Children.Clear();
        }

        string? getFile(string filter) {
            // Configure
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = filter; // Filter files by extension

            // Show
            Nullable<bool> result = dialog.ShowDialog();

            if (result.Value) {
                return dialog.FileName;
            } else {
                return null;
            }
        }

        private void SelectFile(object sender, RoutedEventArgs e) {
            if (active) { Toggle(null, null); }

            string s = getFile("Audio files|*.mp3");

            // If file exists
            if (s != null) {
                // Intialise toolkit
                LAVT.dump();
                LAVT.Initialise(s);

                // GUI
                var button = sender as Button;
                s = System.IO.Path.GetFileName(s);
                if (s.Length > 10) { s = s[..10] + "..."; }
                button.Content = s;
                Tslider.Maximum = LAVT.duration;
                C3.Text = "BPM: " + LAVT.BPM;

                canvas1.VerticalAlignment = VerticalAlignment.Bottom;
                canvas1.HorizontalAlignment = HorizontalAlignment.Left;

                applychanges(null, null);
            }
        }

        private void changequality(object sender, RoutedPropertyChangedEventArgs<double> e) { // ERROR
            if (loaded) {
                // load
                if (active) { Toggle(null, null); }

                // Quality
                quality = (int)qslider.Value;
                Q1.Text = "Quality: " + quality;
                CH1.IsEnabled = true;
            }
        }

        private void changerefreshrate(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (loaded) {
                if (active) { Toggle(null, null); }

                // GUI
                refreshrate = (int)Rslider.Value;
                R1.Text = "Refresh rate: " + refreshrate;
                CH1.IsEnabled = true;
            }
        }

        private void changesensitivity(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (loaded) {
                if (active) { Toggle(null, null); }

                sensitivity = (int)Sslider.Value;
                S1.Text = "Sensitivity: " + sensitivity;
                CH1.IsEnabled = true;
            }
        }

        private void applychanges(object sender, RoutedEventArgs e) {
            if (loaded) {
                if (active) { Toggle(null, null); }

                // interact
                setInteract(false);

                // timer
                if (timerGUI != null) { timerGUI.Stop(); timerGUI = null; }
                timerGUI = new System.Timers.Timer(refreshrate);
                timerGUI.Elapsed += UpdateGUI;
                timerGUI.Enabled = true;
                timerGUI.AutoReset = true;
                timerGUI.Start();

                // data
                CH1.Dispatcher.InvokeAsync(() => {
                    LAVT.setData(quality, sensitivity);

                    setInteract(true);
                    CH1.IsEnabled = false;

                    for (int i = 0; i < LAVT.data[0][0].magnitudes.Length; i++) {
                        Line line = new Line();
                        line.StrokeThickness = 1;
                        line.Stroke = System.Windows.Media.Brushes.Black;
                        line.X1 = canvas1.ActualWidth / 2 + i;
                        line.X2 = canvas1.ActualWidth / 2 + i;
                        line.Y1 = 0;
                        line.Y2 = 15;
                        canvas1.Children.Add(line);
                    }
                }, DispatcherPriority.SystemIdle);
            }
        }

        private void changetime(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (loaded) {
                if (seconds != (int)Tslider.Value) {
                    seconds = (int)Tslider.Value;
                    milliseconds = 0;
                    LAVT.seek(seconds);
                    T1.Text = "Seeked to " + seconds + " / " + LAVT.duration;
                }
            }
        }

        private void Toggle(object sender, RoutedEventArgs e) {
            if (!active) {
                // start
                B1.Content = "Pause";
                LAVT.StartPlayback();
            } else {
                // pause
                LAVT.PausePlayback();
                B1.Content = "Start";
            }

            active = !active;
        }

        private void stop(object sender, RoutedEventArgs e) {
            if (loaded) {
                if (active) { Toggle(null, null); }
                seconds = 0;
                milliseconds = 0;
                LAVT.StopPlayback();
                Tslider.Value = 0;
                T1.Text = "Stopped";
            }
        }

        private void UpdateGUI(object? sender, EventArgs e) {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                // Loaded GUI
                if (LAVT.analysed) {
                    if (seconds < LAVT.duration && active) {
                        // Select data
                        pos = (int)Math.Round(((quality - 1) / 1000.0) * (double)milliseconds);
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
                            if (d.onset) {
                                l[j / 2].Stroke = System.Windows.Media.Brushes.Green;
                            } else {
                                l[j / 2].Stroke = System.Windows.Media.Brushes.Black;
                            }
                        }

                        // Timing
                        milliseconds += refreshrate;

                        if (milliseconds >= 1000) {
                            seconds += 1;
                            milliseconds = 0;
                        }

                        // GUI
                        T1.Text = seconds + " (Chunk: " + (int)pos + " / " + quality + ") / " + LAVT.duration;
                    } else if (active) {
                        seconds = 0;
                        milliseconds = 0;
                        Toggle(null, null);
                    }
                }

            Tslider.Value = seconds;
            }));
        }
    }
}