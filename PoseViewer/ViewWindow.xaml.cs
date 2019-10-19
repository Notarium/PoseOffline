using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace PoseViewer
{
    /// <summary>
    /// ViewWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ViewWindow : Window, INotifyPropertyChanged
    {
        public ViewWindow(DisplayableEntry entry)
        {
            _entry = entry;
            InitializeComponent();
        }

        private int idx = 0;
        private DisplayableEntry _entry;

        public BitmapImage CurrentImage
        {
            get => _entry.FullImages[idx];
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                idx++;
            }
            else
            {
                idx--;
            }

            if (idx > 35)
            {
                idx -= 36;
            }

            if (idx < 0)
            {
                idx += 36;
            }
            OnPropertyChanged("CurrentImage");
        }
    }
}