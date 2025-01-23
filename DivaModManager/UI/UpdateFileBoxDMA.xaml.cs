using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Media;

namespace DivaModManager.UI
{
    /// <summary>
    /// Interaction logic for UpdateFileBoxDMA.xaml
    /// </summary>
    public partial class UpdateFileBoxDMA : Window
    {
        public Uri chosenFileUrl;
        public string chosenFileName;

        class DMAFileDownload
        {
            public String FileName { get; set; }
            public Uri FileUrl { get; set; }
        }

        public UpdateFileBoxDMA(DivaModArchivePost post)
        {
            InitializeComponent();
            List<DMAFileDownload> files = new List<DMAFileDownload>();
            for (int i = 0; i < post.Files.Count; i++)
            {
                files.Add(new DMAFileDownload { FileName = post.FileNames[i], FileUrl = post.Files[i] });
            }
            FileList.ItemsSource = files;
            TitleBox.Text = post.Name;
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            var item = button.DataContext as DMAFileDownload;
            chosenFileUrl = item.FileUrl;
            chosenFileName = item.FileName;
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
