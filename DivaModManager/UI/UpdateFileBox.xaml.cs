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
    /// Interaction logic for UpdateFileBox.xaml
    /// </summary>
    public partial class UpdateFileBox : Window
    {
        public string chosenFileUrl;
        public string chosenFileName;

        public UpdateFileBox(List<GameBananaItemFile> files, string packageName)
        {
            InitializeComponent();
            FileList.ItemsSource = files;
            TitleBox.Text = packageName;
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            var item = button.DataContext as GameBananaItemFile;
            chosenFileUrl = item.DownloadUrl;
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
