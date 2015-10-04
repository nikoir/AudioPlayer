using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using System.Windows.Shapes;
using System.ComponentModel;
using Un4seen.Bass;
using Un4seen.Bass.Misc;
using Un4seen.Bass.AddOn.Wma;
using Microsoft.Win32;
using WPFSoundVisualizationLib;

namespace AudioPlayer
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        BassEngine bassEngine;
        public MainWindow()
        {
            InitializeComponent();
            VolumeSlider.Value = 50;
            bassEngine = new BassEngine();
            spectrumAnalyzer.RegisterSoundPlayer(bassEngine);
        }

        void PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Position":
                    CurLengthLB.Content = bassEngine.GetCurrentFile().Position.ToString(@"hh\:mm\:ss");
                    //slider.Value = bassEngine.GetCurrentFile().Position.TotalSeconds;
                    break;
                case "Length":
                    LengthLB.Content = bassEngine.GetCurrentFile().Length.ToString(@"hh\:mm\:ss");
                    break;
            }
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if (bassEngine.CanPause)
                bassEngine.Pause();
            else
                if (bassEngine.CanPlay)
                    bassEngine.Play();
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            if (openFileDialog.ShowDialog() == true)
            {
                bassEngine.AddFiles(openFileDialog.FileNames, PropertyChanged);
                this.DataContext = bassEngine;
                CompositionList.ItemsSource = bassEngine.Compositions;
                LengthLB.Content = bassEngine.GetCurrentFile().Length.ToString(@"hh\:mm\:ss");
                bassEngine.SetVolume(Convert.ToInt32(VolumeSlider.Value));
            }
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            bassEngine.Stop();
        }

        private void slider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            bassEngine.Pause();
        }

        private void slider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            bassEngine.Rewind(slider.Value);
            bassEngine.Play();
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            VolumeLB.Content = Convert.ToInt32(e.NewValue);
            if (bassEngine != null)
                bassEngine.SetVolume(Convert.ToInt32(e.NewValue));
        }
    }
}
