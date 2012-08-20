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
        public DirectoryInfo Current
        {
            get { return current_; }
            set
            {
                current_ = value;
                NotifyPropertyChanged("Current");
                NotifyPropertyChanged("Path");
                /*
                files_ = new ObservableCollection<FileSystemInfo>(
                current_.GetFileSystemInfos().ToArray());
                 */
                files_.Clear();
                foreach (var e in current_.GetFileSystemInfos())
                {
                    files_.Add(e);
                }
            }
        }

        public string Path
        {
            get { return current_.FullName; }
            set
            {
                Current = new DirectoryInfo(value);
            }
        }

        private ObservableCollection<FileSystemInfo> files_ = new ObservableCollection<FileSystemInfo>();
        public ReadOnlyObservableCollection<FileSystemInfo> Files
        {
            get { return new ReadOnlyObservableCollection<FileSystemInfo>(files_); }
        }

        public FileView(String path)
        {
            Current = new DirectoryInfo(path);
        }
    }
}
