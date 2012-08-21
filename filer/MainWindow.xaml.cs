using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace filer
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private FileView fileView_ = new FileView("C:\\");

        public MainWindow()
        {
            InitializeComponent();

            DataContext=fileView_;
        }

        private void listBoxItem_DoubleClick(object sender, MouseButtonEventArgs args)
        {
            var listItem = sender as ListBoxItem;
            var item = listItem.Content as Item;
            var directory = item.Info as DirectoryInfo;
            if (directory != null)
            {
                fileView_.Current=directory;
                return;
            }
            var file = item.Info as FileInfo;
            if(file != null)
            {
                // todo: fileの実行
                return;
            }
        }

        private void goParent_Click(object sender, RoutedEventArgs args)
        {
            if (fileView_.Current.Parent != null)
            {
                fileView_.Current = fileView_.Current.Parent;
            }
        }

    }
}
