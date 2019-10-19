using PoseOfflineLib;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;


namespace PoseViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public string Selected
        {
            get => _selected;
            set
            {
                _selected = value;
                OnPropertyChanged("Selected");
            }
        }

        private string _scrotext;

        public string ScrollValue
        {
            get => _scrotext;
            set
            {
                _scrotext = value;
                OnPropertyChanged("ScrollValue");
            }
        }

        private bool _fullload;

        public bool FullyLoaded
        {
            get => _fullload;
            set
            {
                _fullload = value;
                OnPropertyChanged("FullyLoaded");
            }
        }

        private string _selected;
        public ObservableCollection<DisplayableEntry> QueriedCollection { get; set; } = new ObservableCollection<DisplayableEntry>();

        public event PropertyChangedEventHandler PropertyChanged;

        private DataService _service;

        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var sel = ((TreeViewItem)TagSelection.SelectedItem);
            if (sel.Tag.ToString() != "item")
            {
                return;
            }
            TagSelection.IsEnabled = false;
            QueriedCollection.Clear();
            if (sel.Tag.ToString() == "item")
            {
                IEnumerable<DisplayableEntry> qrslt = null;
                qrslt = _service.Query((string)sel.Header);
                _service.ExtendQuery(qrslt);
                qrslt.ToList().ForEach(qe => QueriedCollection.Add(qe));
            }
            //else if (sel.Tag.ToString() == "collection")
            //{
            //    foreach (TreeViewItem tvi in sel.Items)
            //    {
            //        IEnumerable<DisplayableEntry> qrslt = null;
            //        qrslt = _service.Query((string)tvi.Header);
            //        _service.ExtendQuery(qrslt);
            //        qrslt.ToList().ForEach(qe => QueriedCollection.Add(qe));
            //    }
            //}
            TagSelection.IsEnabled = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _service = new DataService();
            ScrollValue = "-";
            CounterVisibility = Visibility.Collapsed;
            Counter = 9;
            _timer = new Timer();
            _timer.Stop();
            _timer.Elapsed += _timer_Elapsed;
            _gapTimer = new Timer();
            _gapTimer.Interval = 1000;
            _gapTimer.Stop();
            _gapTimer.Elapsed += _gapTimer_Elapsed;
            SetQuickDrawState(false);
        }
        private void _gapTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Counter -= 1;
            if(Counter == 0)
            {
                _gapTimer.Stop();
                CounterVisibility = Visibility.Collapsed;
            }
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!IsQuickDrawing)
            {
                _timer.Stop();
                return;
            }
            Dispatcher.Invoke(LoadRandomImage);
        }

        private BitmapImage _randImg;
        public BitmapImage RandomImage
        {
            get => _randImg;
            set
            {
                _randImg = value;
                OnPropertyChanged("RandomImage");
            }
        }

        private void CountDown()
        {
            CounterVisibility = Visibility.Visible;
            Counter = 3;
            _gapTimer.Start();
        }

        private void LoadRandomImage()
        {
            CountDown();
            var stream = _service._db.Random();
            var img = new BitmapImage();
            img.BeginInit();
            img.StreamSource = stream;
            img.DownloadCompleted += (sender, ev) =>
            {
                stream.Dispose();
            };
            img.EndInit();
            RandomImage = img;
        }

        private void ResultView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DisplayableEntry selected = ResultView.SelectedItem as DisplayableEntry;
            if (selected != null)
            {
                var stream = EntryHelper.PoseImgIDs(selected).Select(str => _service._db.FileDownloadStream(str)).ToArray();
                selected.LoadFullImages(stream);
                ViewWindow vw = new ViewWindow(selected);
                vw.ShowDialog();
            }
        }

        private void ResultView_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollValue = $"{e.VerticalOffset.ToString(".00")}/{(e.ExtentHeight - e.ViewportHeight).ToString(".00")}";
        }

        public bool _isQd;

        public bool IsQuickDrawing
        {
            get => _isQd;
        }

        public bool ButtonEnabled
        {
            get => !_isQd;
        }

        private Visibility _counterVisibility;

        public Visibility CounterVisibility
        {
            get => _counterVisibility;
            set
            {
                _counterVisibility = value;
                OnPropertyChanged("CounterVisibility");
            }
        }

        private int _counter;

        public int Counter
        {
            get => _counter;
            set
            {
                _counter = value;
                OnPropertyChanged("Counter");
            }
        }

        private void SetQuickDrawState(bool state)
        {
            _isQd = state;
            OnPropertyChanged("IsQuickDrawing");
            OnPropertyChanged("ButtonEnabled");
        }
        private Timer _timer;
        private Timer _gapTimer;
        
        private void QuickDraw_Click(object sender, RoutedEventArgs e)
        {
            int Sec = int.Parse(((Button)sender).Content.ToString());
            SetQuickDrawState(true);
            _timer.Interval = (Sec+3) * 1000;
            _timer.Start();
            LoadRandomImage();
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                MessageBox.Show("想跑路了？");
            }
            SetQuickDrawState(false);
        }
    }
}