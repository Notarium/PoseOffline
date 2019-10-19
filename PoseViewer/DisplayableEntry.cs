using PoseOfflineLib;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Media.Imaging;

namespace PoseViewer
{
    public class DisplayableEntry : Entry, INotifyPropertyChanged
    {
        public BitmapImage PreviewImage
        {
            get => _prev;
            private set
            {
                _prev = value;
                OnPropertyChanged("PreviewImage");
            }
        }

        private BitmapImage _prev;

        public List<BitmapImage> FullImages
        {
            get => _full;
            private set
            {
                _full = value;
                OnPropertyChanged("FullImage");
            }
        }

        public List<BitmapImage> _full;

        public DisplayableEntry(Entry entry)
        {
            ID = entry.ID;
            SrcUriDir = entry.SrcUriDir;
            Tags = entry.Tags;
            Counts = entry.Counts;
            PreviewImgUri = entry.PreviewImgUri;
            _prev = null;
            _full = new List<BitmapImage>();
        }

        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void LoadPreview(Stream stream)
        {
            var img = new BitmapImage();
            img.BeginInit();
            //img.CacheOption = BitmapCacheOption.OnLoad;
            img.StreamSource = stream;
            img.DownloadCompleted += (sen, ev) =>
            {
                img.StreamSource.Dispose();
            };
            img.EndInit();
            PreviewImage = img;
        }

        public void LoadFullImages(Stream[] streams)
        {
            FullImages.Clear();
            foreach (Stream s in streams)
            {
                var img = new BitmapImage();
                img.BeginInit();
                img.DownloadCompleted += (sen, ev) =>
                {
                    img.StreamSource.Dispose();
                };
                img.StreamSource = s;
                img.EndInit();
                FullImages.Add(img);
            }
        }
    }
}