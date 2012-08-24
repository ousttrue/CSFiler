using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Diagnostics;


namespace filer
{


    class Item : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
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

        private BitmapSource bitmap_;
        public BitmapSource Bitmap
        {
            set
            {
                bitmap_ = value;
                NotifyPropertyChanged("Bitmap");
            }
            get
            {
                return bitmap_;

            }
        }

    };


    class FileView : INotifyPropertyChanged
    {
        public Dispatcher dispatcher_;

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public FileView(String path, Dispatcher dispatcher)
        {
            dispatcher_ = dispatcher;
            Current = new DirectoryInfo(path);
        }

        private BitmapSource LoadBitmapSource(FileSystemInfo info)
        {
            SHFILEINFO shinfo = new SHFILEINFO();
            var hImgLarge = Win32.SHGetFileInfo(info.FullName, 0,
                ref shinfo, (uint)Marshal.SizeOf(shinfo),
                Win32.SHGFI_ICON | Win32.SHGFI_LARGEICON);
            BitmapSource source = Imaging.CreateBitmapSourceFromHIcon(shinfo.hIcon, Int32Rect.Empty, null);
            Win32.DestroyIcon(shinfo.hIcon);
            return source;
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
                NotifyPropertyChanged("Status");
                files_.Clear();
                var workList=new List<Item>();
                try
                {
                    foreach (var e in current_.GetFileSystemInfos())
                    {
                        var item=new Item
                        {
                            Info = e
                        };
                        files_.Add(item);
                        workList.Add(item);
                    }

                    // 別スレッドでビットマップ更新を呼び出す
                    var task = new Task(() =>
                    {
                        foreach (var item in workList)
                        {
                            var source = LoadBitmapSource(item.Info);
                            source.Freeze();
                            Action action = () =>
                            {
                                item.Bitmap = source;
                            };
                            dispatcher_.Invoke(action);
                        }
                    });
                    task.Start();
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

        public String Status
        {
            get
            {
                return String.Format("{0} entries", files_.Count);
            }
        }
    }
}
