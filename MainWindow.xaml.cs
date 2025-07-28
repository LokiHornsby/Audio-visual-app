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

        // arrays
        short[] pcm;
        double[][] samples;
        double[][] windows;
        System.Numerics.Complex[][] ffts;

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

                // get samples
                //using (AudioFileReader reader = new AudioFileReader(filename)) {

                // read
                byte[] buffer = new byte[getReader().Length];
                int read = getReader().Read(buffer, 0, buffer.Length);

                //check1.Content = read.ToString() + " " + read2.ToString();

                // sample
                pcm = new short[read / 2];
                Buffer.BlockCopy(buffer, 0, pcm, 0, read);

                // samples
                samples = new double[(int)(pcm.Length / getSampleSize())][];

                for (int j = 0; j < pcm.Length / getSampleSize(); j++) {
                    // define a sample
                    samples[j] = new double[getSampleSize()];

                    // fill it with data
                    for (int i = 0; i < samples[j].Length; i++) {
                        samples[j][i] = pcm[(getSampleSize() * j) + i];
                    }
                }

                // windows
                windows = new double[samples.Length][];

                for (int i = 0; i < samples.Length; i++) {
                    windows[i] = getWindow(samples[i]);
                }

                // ffts
                ffts = new System.Numerics.Complex[windows.Length][];

                for (int i = 0; i < windows.Length; i++) {
                    ffts[i] = PerformFFT(windows[i]);
                }

                // configure slider
                slider1.Maximum = pcm.Length - getSampleSize();
                slider1.Value = 0;
                slider1.IsMoveToPointEnabled = true;
                setReadPosition(0);

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

        private void dispatcherTimer_Tick(object? sender, EventArgs e) {
            slider1.Value = (int)(getReadPosition() / 2);
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
            if (slider1.Value != getReadPosition()) {
                getReader().Position = ((int)slider1.Value * 2);
            }

            int pos = (int)(slider1.Value / getSampleSize());

            check1.Content = samples[pos][0];
            check2.Content = windows[pos][0];
            check3.Content = ffts[pos][0].Real;

            canvas1.Children.Clear();

            for (int i = 0; i < windows[pos].Length; i++) {
                Line line = new Line();
                line.Visibility = System.Windows.Visibility.Visible;
                line.StrokeThickness = 1;
                line.Stroke = System.Windows.Media.Brushes.Black;
                line.X1 = i;
                line.X2 = i;
                line.Y1 = 0;
                line.Y2 = (windows[pos][i] / 2);
                canvas1.Children.Add(line);
            }


            /*
            // Playback label
            label1.Content = slider1.Value + " / " + slider1.Maximum;

            // read 32 pcm
            double[] sample = new double[getSampleSize()];

            for (int i = 0; i < getSampleSize(); i++) {
                sample[i] = pcm[(int)slider1.Value + i];
            }

            // Value
            check2.Content = sample.Sum();

            canvas1.Children.Clear();

            for (int i = 0; i < windows[(int)slider1.Value].Length; i++) {
                Line line = new Line();
                line.Visibility = System.Windows.Visibility.Visible;
                line.StrokeThickness = 1;
                line.Stroke = System.Windows.Media.Brushes.Black;
                line.X1 = i;
                line.X2 = i;
                line.Y1 = 0;
                line.Y2 = (windows[(int)slider1.Value][i] / 2);
                canvas1.Children.Add(line);
            }

            check3.Content = GetMagnitudes(PerformFFT(getWindow(sample))).Max() > 0;
            */
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