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
            bassEngine = new BassEngine(PropertyChanged);
            spectrumAnalyzer.RegisterSoundPlayer(bassEngine);
        }

        void PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Position":
                    CurLengthLB.Content = bassEngine.Position.ToString(@"hh\:mm\:ss");
                    break;
                case "Length":
                    LengthLB.Content = bassEngine.Length.ToString(@"hh\:mm\:ss");
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
                bassEngine.PropertyChanged += PropertyChanged;
                bassEngine.AddNewPlaylist(openFileDialog.FileNames);
                this.DataContext = bassEngine;
                CompositionList.ItemsSource = bassEngine.Compositions;
                LengthLB.Content = bassEngine.Length.ToString(@"hh\:mm\:ss");
            }
        }



        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            bassEngine.Stop();
        }

        private void slider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            if(bassEngine.IsPlaying)
                bassEngine.Pause();
        }

        private void slider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            if (bassEngine.CanPlay)
            {
                bassEngine.Rewind(slider.Value);
                bassEngine.Play();
            }
        }
        private void AddNewCompositions_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            if (openFileDialog.ShowDialog() == true)
                bassEngine.AddNewCompositions(openFileDialog.FileNames);
        }

        private void DeleteCompositions_Click(object sender, RoutedEventArgs e)
        {
            if (CompositionList.SelectedItems == null)
                return;
            else
            {
                List<Composition> SelectedItems = new List<Composition>();
                foreach (var item in CompositionList.SelectedItems)
                    SelectedItems.Add(item as Composition);
                bassEngine.DeleteCompositions(SelectedItems);
            }
        }
        private void NextComposition_Click(object sender, RoutedEventArgs e)
        {
            bassEngine.PlayNextComposition();
        }
        private void PrevComposition_Click(object sender, RoutedEventArgs e)
        {
            bassEngine.PlayPrevComposition();
        }
    }
}
