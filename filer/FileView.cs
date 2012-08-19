using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.IO;

namespace filer
{
    class FileView : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private String path_;
        public String Path
        {
            get { return path_; }
            set
            {
                path_ = value;
                NotifyPropertyChanged("Path");
                files_ = new ObservableCollection<string>(
                Directory.GetFileSystemEntries(path_).ToArray());
            }
        }

        private ObservableCollection<String> files_ = new ObservableCollection<String>();
        public ReadOnlyObservableCollection<String> Files
        {
            get { return new ReadOnlyObservableCollection<String>(files_); }
        }

        public FileView(String path)
        {
            Path = path;
        }
    }
}
