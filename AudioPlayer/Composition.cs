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
        TagLib.File fileInfo;
        BitmapImage image;

        public event PropertyChangedEventHandler PropertyChanged;
        public TagLib.File FileInfo
        {
            get
            {
                return fileInfo;
            }
            set
            {
                fileInfo = value;
                this.NotifyPropertyChanged("FileInfo");
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
            Image = new BitmapImage();
            Image.BeginInit();
            Image.StreamSource = new MemoryStream(FileInfo.Tag.Pictures[0].Data.Data);
            Image.EndInit();
        }
        public Composition()
        {

        }
    }
}
