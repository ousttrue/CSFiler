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
            get
            {
                return bitmap_;

            }
        }

        public void LoadBitmap(Dispatcher dispatcher)
        {
            SHFILEINFO shinfo = new SHFILEINFO();
            var hImgLarge = Win32.SHGetFileInfo(Info.FullName, 0,
                ref shinfo, (uint)Marshal.SizeOf(shinfo),
                Win32.SHGFI_ICON | Win32.SHGFI_LARGEICON);
            BitmapSource source = Imaging.CreateBitmapSourceFromHIcon(shinfo.hIcon, Int32Rect.Empty, null);
            // for thread
            source.Freeze();
            Win32.DestroyIcon(shinfo.hIcon);
            bitmap_=source;

            if (dispatcher == null)
            {
                return;
            }
            Action action = () =>
            {
                NotifyPropertyChanged("Bitmap");
            };
            dispatcher.Invoke(action);

        }
    };


    class FileView : INotifyPropertyChanged
    {
        public Dispatcher EventDispatcher { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        BackgroundWorker backgroundWorker_;
        public FileView()
        {
            backgroundWorker_ = new BackgroundWorker();
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
                    Action<int, Item> onChange = (int i, Item item) =>
                    {
                        files_.RemoveAt(i);
                        files_.Insert(i, item);
                    };
                    var task=new Task(() =>
                    {
                        foreach (var item in workList)
                        {
                            item.LoadBitmap(EventDispatcher);
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

        public FileView(String path)
        {
            Current = new DirectoryInfo(path);
        }
    }
}
