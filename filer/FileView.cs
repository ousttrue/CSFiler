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

        private DirectoryInfo current_;
        public String Path
        {
            get { return current_.FullName; }
            set
            {
                current_ = new DirectoryInfo(value);
                NotifyPropertyChanged("Path");
                files_ = new ObservableCollection<FileSystemInfo>(
                current_.GetFileSystemInfos().ToArray());
            }
        }

        private ObservableCollection<FileSystemInfo> files_ = new ObservableCollection<FileSystemInfo>();
        public ReadOnlyObservableCollection<FileSystemInfo> Files
        {
            get { return new ReadOnlyObservableCollection<FileSystemInfo>(files_); }
        }

        public FileView(String path)
        {
            Path = path;
        }
    }
}
