using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.IO;
using Un4seen.Bass;
using Un4seen.Bass.Misc;
using Un4seen.Bass.AddOn.Wma;
using TagLib;

namespace AudioPlayer
{
    public class Composition
    {
        TagLib.File FileInfo;
        BitmapImage image;
        string artists;
        string title;
        string album;
        string name;

        public event PropertyChangedEventHandler PropertyChanged;
        public string Artists
        {
            get
            {
                return artists;
            }
            set
            {
                artists = value;
                NotifyPropertyChanged("Artist");
            }
        }

        public string Title
        {
            get
            {
                return title;
            }
            set
            {
                title = value;
                NotifyPropertyChanged("Title");
            }
        }

        public string Album
        {
            get
            {
                return album;
            }
            set
            {
                album = value;
                NotifyPropertyChanged("Album");
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                NotifyPropertyChanged("Name");
            }
        }

        public BitmapImage Image
        {
            get
            {
                return image;
            }
            set
            {
                image = value;
                this.NotifyPropertyChanged("Image");
            }
        }
        private void NotifyPropertyChanged(string info)
        {
            if (this.PropertyChanged == null)
                return;
            this.PropertyChanged((object)this, new PropertyChangedEventArgs(info));
        }
        public Composition(string path, PropertyChangedEventHandler PropertyChanged)
        {
            this.PropertyChanged += PropertyChanged;
            FileInfo = TagLib.File.Create(path);
            if (FileInfo.Tag.Artists.Length == 0)
                Artists = "Unknown artist";
            else
            {
                foreach (string str in FileInfo.Tag.Artists)
                {
                    Artists += str;
                    Artists += "; ";
                }
                Artists = Artists.Substring(0, Artists.Length - 2);
            }
            if (FileInfo.Tag.Title == null)
                Title = "Unknown title";
            else
                Title = FileInfo.Tag.Title;
            if (FileInfo.Tag.Album == null)
                Album = "Unknown album";
            else
                Album = FileInfo.Tag.Album;
            Name = FileInfo.Name;
            Image = new BitmapImage();
            Image.BeginInit();
            if (FileInfo.Tag.Pictures.Length != 0)
                Image.StreamSource = new MemoryStream(FileInfo.Tag.Pictures[0].Data.Data);
            else
                Image.UriSource = new Uri("Content\\note-blue.png", UriKind.RelativeOrAbsolute);
            Image.EndInit();
        }
        public Composition()
        {

        }
    }
}
