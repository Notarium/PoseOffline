using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
using System.Windows.Shapes;
using Microsoft.Win32;
using PoseOfflineLib;

namespace PoseOffline
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
        NetService _service;
        public string StatusString
        {
            get => _statusString;
            set
            {
                _statusString = value;
                OnPropertyChanged("StatusString");
            }
        }
        public string OutputString
        {
            get => _outstr;
            set
            {
                _outstr = value;
                OnPropertyChanged("OutputString");
            }
        }
        private string _outstr = "---";
        private string _statusString = "Initial";

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private async void MenuDebug_Click(object sender, RoutedEventArgs e)
        {
            Result rslt = await _service.Analyse();
            MessageBox.Show("PageCount:" + rslt.PageCount);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _service = new NetService();
            StatusString = "Ready";
            _service.OnProgressChanged += UpdateMessage;
        }
        private void UpdateMessage(string message)
        {
            StatusString = message;
        }
        private async void Analyse_Click(object sender, RoutedEventArgs e)
        {
            Result rslt = await _service.Analyse();
            MessageBox.Show($"errcount:{rslt.ErrorEntry}");
            if (rslt.ErrorMessages.Count() > 0)
            {
                foreach (var msg in rslt.ErrorMessages)
                {
                    MessageBox.Show(msg);
                }
            }
        }

        private void DebugQueryCheck_Click(object sender, RoutedEventArgs e)
        {
            IEnumerable<Entry> r = _service.QueryAll();
            MessageBox.Show(r.Count().ToString());
        }

        private void CleanAll_Click(object sender, RoutedEventArgs e)
        {
            _service.CleanAll(217654389);
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private async void DebugUpdate_Click(object sender, RoutedEventArgs e)
        {
            IEnumerable<Entry> r = _service.QueryAll();
            StatusString = "Running";
            await Task.Run(() =>
            {
                foreach (Entry entry in r)
                {
                    int epos = entry.PreviewImgUri.LastIndexOf('/');
                    string dir = entry.PreviewImgUri.Substring(0, epos + 1);
                    entry.SrcUriDir = dir;
                    _service.Update(entry);
                }
            });
            StatusString = "Ready";
        }

        private async void DownloadPic_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _service.UploadFiles();
                MessageBox.Show("Done");
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
            StatusString = "Ready";
        }

        private void Backup_Click(object sender, RoutedEventArgs e)
        {
            OutputString = _service.Backup();
        }

        private void Restore_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.DefaultExt = ".json";
            dlg.Filter = "Json Files (*.json)|*.json";
            bool? dlgRslt = dlg.ShowDialog();
            if(dlgRslt.GetValueOrDefault())
            {
                var str = File.ReadAllText(dlg.FileName);
                _service.RestoreFromString(str);
                MessageBox.Show("restore complete");
            }
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            _service.CallStop();
        }

        private void UpdateIndex_Click(object sender, RoutedEventArgs e)
        {
            _service._dBService.UpdateIndexIndex();
            MessageBox.Show("done");
        }
    }
}
