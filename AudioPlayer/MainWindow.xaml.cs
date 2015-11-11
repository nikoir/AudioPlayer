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
            bassEngine.PropertyChanged += PropertyChanged;
            this.DataContext = bassEngine;
            LengthLB.Content = "00:00:00";
            CurLengthLB.Content = "00:00:00";
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
            openFileDialog.Filter = "Music files|*.mp3;*.mp2;*.mp1;*.ogg;*.wav;*.aiff";
            if (openFileDialog.ShowDialog() == true)
            {
                bassEngine.AddNewPlaylist(openFileDialog.FileNames);
                CompositionList.ItemsSource = bassEngine.Compositions;
                LengthLB.Content = bassEngine.Length.ToString(@"hh\:mm\:ss");
            }
        }

        private void OpenPlaylist_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Playlists|*m3u";
            if (openFileDialog.ShowDialog() == true)
            {
                string[] FileNames = M3UFile.Parse(openFileDialog.FileName);
                if (FileNames != null)
                {
                    bassEngine.AddNewPlaylist(FileNames);
                    this.DataContext = bassEngine;
                    CompositionList.ItemsSource = bassEngine.Compositions;
                    LengthLB.Content = bassEngine.Length.ToString(@"hh\:mm\:ss");
                }
            }
        }

        private void SavePlaylist_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog() == true)
                M3UFile.Save(bassEngine.GetFileNames(), saveFileDialog.FileName);
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
