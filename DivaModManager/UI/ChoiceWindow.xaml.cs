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
    public partial class ChoiceWindow : Window
    {
        public int? choice = null;
        public ChoiceWindow(List<Choice> choices, string title = null)
        {
            InitializeComponent();
            ChoiceList.ItemsSource = choices;
            if (title != null)
                Title = title;
        }
        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            var item = button.DataContext as Choice;
            choice = item.Index;
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
