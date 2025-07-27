using NAudio.Wave;
using System;
using System.Windows;
using System.Windows.Threading;

namespace Audio_visual_app {
    using static LAVT;

    public partial class MainWindow : Window {
        // file name
        string filename;

        // audio is loading
        bool audioLoading;

        // arrays
        short[] pcm;

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

                // set slider
                slider1.Maximum = getReadLength() - 8;
                slider1.Minimum = 0;
                slider1.Value = 0;
                slider1.IsMoveToPointEnabled = true;

                // disable UI
                slider1.IsEnabled = false;
                B1.IsEnabled = false;
                B2.IsEnabled = false;
                B3.IsEnabled = false;
                check1.IsEnabled = false;
                check1.Content = "Loading...";
                check2.IsEnabled = false;
                check3.IsEnabled = false;

                // audio
                audioLoading = true;

                // get samples
                using (AudioFileReader reader = new AudioFileReader(filename)) {
                    byte[] buffer = new byte[reader.Length];
                    int read = reader.Read(buffer, 0, buffer.Length);

                    short[] sampleBuffer = new short[read / 2];
                    Buffer.BlockCopy(buffer, 0, sampleBuffer, 0, read);

                    pcm = sampleBuffer;

                    slider1.Maximum = pcm.Length - getSampleSize();
                }

                enable();
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

        private void enable() {
            // enable/disable
            audioLoading = false;
            B1.IsEnabled = true;
            B2.IsEnabled = true;
            B3.IsEnabled = true;

            // loaded check box
            check1.IsChecked = true;
            check1.Content = "Loaded.";

            // slider modify
            slider1.IsEnabled = true;
            setReadPosition(0);
        }
        
        /// <summary>
        /// When the slider changes value
        /// </summary>
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            // floor slider
            slider1.Value = Math.Floor(slider1.Value);

            // move slider
            if (slider1.Value != getReadPosition() && !audioLoading) {
                setReadPosition((int)slider1.Value);
            }

            // Playback label
            label1.Content = slider1.Value + " / " + slider1.Maximum;

            // Value
            check2.Content = pcm[(int)slider1.Value];

            // read 32 pcm
            double[] sample = new double[getSampleSize()];

            for (int i = 0; i < getSampleSize(); i++) {
                sample[i] = pcm[(int)slider1.Value + i];
            }

            check3.Content = GetMagnitudes(PerformFFT(sample)).Max();
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