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

        // timer
        DispatcherTimer dispatcherTimer;

        // lines
        List<Line[]> lines;

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

                // configure slider
                slider1.Maximum = pcm.Length - getSampleSize();
                slider1.Value = 0;
                slider1.IsMoveToPointEnabled = true;
                getReader().Position = 0;

                // dispatch timer
                dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
                dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
                dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 0, 1);
                dispatcherTimer.Start();
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

        /// <summary>
        /// When the slider changes value
        /// </summary>
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            // move slider
            if (slider1.Value != getReader().Position) {
                getReader().Position = ((int)slider1.Value * 2);
            }

            // get time
            label1.Content = getReader().Position + " / " + getReader().Length;

            // get position
            int pos = (int)(slider1.Value / getSampleSize());

            
        }

        private void dispatcherTimer_Tick(object? sender, EventArgs e) {
            slider1.Value = (int)(getReader().Position / 2);

            // position in array
            int pos = (int)(slider1.Value / getSampleSize());

            // draw waveform
            canvas1.Children.Clear();

            for (int i = 0; i < windows[pos].Length; i++) {
                int x = i;
                int y = (int)((windows[pos][i] / 2) / 100);

                Line line = new Line();
                line.Visibility = System.Windows.Visibility.Visible;
                line.StrokeThickness = 1;
                line.Stroke = System.Windows.Media.Brushes.Black;
                line.X1 = x + 10;
                line.X2 = x + 10;
                line.Y1 = 10;
                line.Y2 = 10 + y;
                canvas1.Children.Add(line);
            }

            // onset
            if (pos > 0) {
                check1.IsChecked = (magnitudes[pos].Sum() > magnitudes[pos - 1].Sum());
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

        private void RadioButton_Checked(object sender, RoutedEventArgs e) {

        }

        private void check1_Checked(object sender, RoutedEventArgs e) {

        }
    }
}