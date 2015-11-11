using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using Un4seen.Bass;
using WPFSoundVisualizationLib;
using TagLib;

namespace AudioPlayer
{
    //Все равно этот код никто не прочитает, поэтому я даже не старался писать его красиво. Главное, что он работает. Ну или почти работает...
    public class BassEngine :ISpectrumPlayer, INotifyPropertyChanged
    {
        private readonly int fftDataSize = 2048;
        private readonly int maxFFT = -2147483645;
        public int activeStreamHandle;
        private int sampleFrequency = 44100;
        private bool canPlay;
        private bool canPause;
        private bool isPlaying;
        private bool canStop;
        public DispatcherTimer Timer { get; private set; }
        public ObservableCollection<Composition> Compositions;
        private Composition currentComposition;
        public int currentCompositionNumber;
        public bool EnableRepeating { get; set; }
        private TimeSpan length;
        private TimeSpan position;
        private int volume;
        public TimeSpan Length
        {
            get
            {
                return length;
            }
            private set
            {
                length = value;
                NotifyPropertyChanged("Length");
            }
        }
        public int Volume
        {
            get
            {
                return volume;
            }
            set
            {
                volume = value;
                SetVolume(Volume);
                NotifyPropertyChanged("Volume");
            }
        }

        public TimeSpan Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
                NotifyPropertyChanged("Position");
            }
        }

        public Composition CurrentComposition
        {
            get
            {
                return currentComposition;
            }
            private set
            {
                currentComposition = value;
                NotifyPropertyChanged("CurrentComposition");
            }
        }
        public int CurrentCompositionNumber
        {
            get
            {
                return currentCompositionNumber;
            }
            private set
            {
                if (value < Compositions.Count)
                {
                    currentCompositionNumber = value;
                    CurrentComposition = Compositions[CurrentCompositionNumber];
                    OpenFile(CurrentComposition.Name);
                    this.NotifyPropertyChanged("CurrentCompositionNumber");
                }
            }
        }

        public int ActiveStreamHandle
        {
            get
            {
                return this.activeStreamHandle;
            }
            private set
            {
                activeStreamHandle = value;
                this.NotifyPropertyChanged("ActiveStreamHandle");
            }
        }

        public bool CanPlay
        {
            get
            {
                return this.canPlay;
            }
            private set
            {
                canPlay = value;
                this.NotifyPropertyChanged("CanPlay");
            }
        }

        public bool CanPause
        {
            get
            {
                return this.canPause;
            }
            private set
            {
                canPause = value;
                this.NotifyPropertyChanged("CanPause");
            }
        }

        public bool CanStop
        {
            get
            {
                return this.canStop;
            }
            private set
            {
                canStop = value;
                this.NotifyPropertyChanged("CanStop");
            }
        }

        public bool IsPlaying
        {
            get
            {
                return this.isPlaying;
            }
            private set
            {
                isPlaying = value;
                this.NotifyPropertyChanged("IsPlaying");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public TagLib.File GetFileData (string path)
        {
            return TagLib.File.Create(path);
        }
        public BassEngine(PropertyChangedEventHandler PropertyChanged)
        {
            Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
            Compositions = new ObservableCollection<Composition>();
            Compositions.CollectionChanged += Compositions_CollectionChanged;
            Volume = 50;
        }

        public void AddNewPlaylist(string[] FileNames)
        {
            Timer = new DispatcherTimer();
            Timer.Tick += Timer_Tick;
            Timer.Interval = TimeSpan.FromMilliseconds(100);
            Compositions.Clear();
            foreach (string FileName in FileNames)
                if (System.IO.File.Exists(FileName))
                    Compositions.Add(new Composition(FileName, PropertyChanged));
            CurrentCompositionNumber = 0;
            CurrentComposition = Compositions[CurrentCompositionNumber];
            OpenFile(CurrentComposition.Name);
        }

        public void Compositions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {

        }

        void SetVolume(int Volume)
        {
            float vol = (float)Volume / 100;
            Bass.BASS_ChannelSetAttribute(ActiveStreamHandle, BASSAttribute.BASS_ATTRIB_VOL, vol);
        }

        void Timer_Tick(object sender, EventArgs e)
        {
            Position = TimeSpan.FromSeconds(Bass.BASS_ChannelBytes2Seconds(ActiveStreamHandle, Bass.BASS_ChannelGetPosition(ActiveStreamHandle)));
            if (Position.TotalSeconds == Length.TotalSeconds)
                PlayNextComposition();
        }

        public string[] GetFileNames()
        {
            List<string> FileNames = new List<string>();
            foreach (Composition composition in Compositions)
                FileNames.Add(composition.Name);
            return FileNames.ToArray();
        }

        public int GetFFTFrequencyIndex(int frequency)
        {
            return Utils.FFTFrequency2Index(frequency, this.fftDataSize, this.sampleFrequency);
        }

        public bool GetFFTData(float[] fftDataBuffer)
        {
            return Bass.BASS_ChannelGetData(this.ActiveStreamHandle, fftDataBuffer, this.maxFFT) > 0;
        }

        private void NotifyPropertyChanged(string info)
        {
            if (this.PropertyChanged == null)
                return;
            this.PropertyChanged((object)this, new PropertyChangedEventArgs(info));
        }

        public void Stop()
        {
            if (this.ActiveStreamHandle != 0)
            {
                Bass.BASS_ChannelStop(this.ActiveStreamHandle);
                Bass.BASS_ChannelSetPosition(this.ActiveStreamHandle, 0);
                Timer.Stop();
                Position = new TimeSpan(0);
            }
            this.IsPlaying = false;
            this.CanStop = false;
            this.CanPlay = true;
            this.CanPause = false;
        }

        public void Pause()
        {
            if (!this.IsPlaying || !this.CanPause)
                return;
            Timer.Stop();
            Bass.BASS_ChannelPause(this.ActiveStreamHandle);
            this.IsPlaying = false;
            this.CanPlay = true;
            this.CanPause = false;
        }

        public void Play()
        {
            if (CanPlay)
            {
                this.PlayCurrentStream();
                this.IsPlaying = true;
                this.CanPause = true;
                this.CanPlay = false;
                this.CanStop = true;
            }
        }

        public bool OpenFile(string path)
        {
            Stop();
            if (ActiveStreamHandle != 0)
                Bass.BASS_StreamFree(ActiveStreamHandle);
            if (System.IO.File.Exists(path))
            {
                ActiveStreamHandle = Bass.BASS_StreamCreateFile(path, 0, 0, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_PRESCAN);
                if (ActiveStreamHandle != 0)
                {
                    CanPlay = true;
                    Length = TimeSpan.FromSeconds(Bass.BASS_ChannelBytes2Seconds(ActiveStreamHandle, Bass.BASS_ChannelGetLength(ActiveStreamHandle)));
                    return true;
                }
                else
                {
                    ActiveStreamHandle = 0;
                    CanPlay = false;
                }
            }
            return false;
        }

        private void PlayCurrentStream()
        {
            if (this.ActiveStreamHandle == 0)
                return;
            Bass.BASS_ChannelPlay(this.ActiveStreamHandle, false);
            SetVolume(Volume);
            Timer.Start();
        }
        public void Rewind(double seconds)
        {
            Bass.BASS_ChannelSetPosition(ActiveStreamHandle, seconds);
        }
        public void AddNewCompositions(string[] FileNames)
        {
            foreach (string FileName in FileNames)
                if (System.IO.File.Exists(FileName))
                    Compositions.Add(new Composition(FileName, PropertyChanged));
        }
        public void DeleteCompositions (List<Composition> SelectedItems)
        {
            foreach (var Composition in SelectedItems)
                Compositions.Remove(Composition);
            if (Compositions.Count == 0)
            {
                if (ActiveStreamHandle != 0)
                    Bass.BASS_StreamFree(ActiveStreamHandle);
                Length = TimeSpan.FromMilliseconds(0);
                this.CanPlay = false;
            }
            else
            {
                CurrentCompositionNumber = 0;
                OpenFile(Compositions[CurrentCompositionNumber].Name);
                this.CanPlay = true;
            }
            this.IsPlaying = false;
            this.CanPause = false;
            Position = TimeSpan.FromMilliseconds(0);
        }
        public void PlayNextComposition()
        {
            if (CurrentCompositionNumber == Compositions.Count - 1)
            {
                if (EnableRepeating)
                {
                    CurrentCompositionNumber = 0;
                    Play();
                }
                else
                    Stop();
            }
            else
            {
                CurrentCompositionNumber++;
                Play();
            }
        }
        public void PlayPrevComposition()
        {
            if (CurrentCompositionNumber == 0)
            {
                Stop();
                return;
            }
            else
            {
                CurrentCompositionNumber--;
                Play();
            }
        }
    }
}
