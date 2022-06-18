using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
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
    public partial class AltLinkWindow : Window
    {
        public AltLinkWindow(List<GameBananaAlternateFileSource> files, string packageName, string game, string url, bool update = false)
        {
            InitializeComponent();
            FileList.ItemsSource = files;
            TitleBox.Text = packageName;
            Description.Text = update ? $"Links from the Alternate File Sources section were found. You can " +
                $"select one to manually download.\nTo update, delete the previous files from and extract the downloaded archive into:"
                : $"Links from the Alternate File Sources section were found. You can " +
                $"select one to manually download.\nTo install, extract the downloaded archive into:";
            PathText.Text = update ? $"{Global.assemblyLocation}{Global.s}Mods{Global.s}{game}{Global.s}{packageName}" 
                : $"{Global.assemblyLocation}{Global.s}Mods{Global.s}{game}";
            FetchDescription.Text = update ? $"To fetch GameBanana metadata for the manual update, Right click {packageName} > " +
                $"Fetch Metadata, and use the link:"
                : $"To fetch GameBanana metadata from the manual download, Right click row > " +
                $"Fetch Metadata, and use the link:";
            UrlText.Text = url;
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            var item = button.DataContext as GameBananaAlternateFileSource;
            var ps = new ProcessStartInfo(item.Url.AbsoluteUri)
            {
                UseShellExecute = true,
                Verb = "open"
            };
            Process.Start(ps);
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
