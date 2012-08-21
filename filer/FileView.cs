using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.IO;

namespace filer
{
    class Item
    {
        public bool IsDirectory
        {
            set;
            get;
        }

        public FileSystemInfo Info
        {
            set;
            get;
        }

        public String Name
        {
            get { return Info.Name; }
        }

        public String Length
        {
            get
            {
                var file = Info as FileInfo;
                if (file == null)
                {
                    return "";
                }
                else
                {
                    return file.Length.ToString();
                }
            }
        }
    };

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
                files_.Clear();
                try
                {
                    foreach (var e in current_.GetFileSystemInfos())
                    {
                        files_.Add(new Item
                        {
                            IsDirectory = (e is DirectoryInfo),
                            Info = e
                        });
                    }
                }
                catch (UnauthorizedAccessException e)
                {
                    // do nothing
                }
                catch (DirectoryNotFoundException e)
                {
                    // do nothing
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

        private ObservableCollection<Item> files_ = new ObservableCollection<Item>();
        public ReadOnlyObservableCollection<Item> Files
        {
            get { return new ReadOnlyObservableCollection<Item>(files_); }
        }

        public FileView(String path)
        {
            Current = new DirectoryInfo(path);
        }
    }
}
