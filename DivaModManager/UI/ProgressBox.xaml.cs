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
using System.Threading;
using System.ComponentModel;

namespace DivaModManager
{
    /// <summary>
    /// Interaction logic for ProgressBox.xaml
    /// </summary>
    public partial class ProgressBox : Window
    {
        private CancellationTokenSource cancellationTokenSource;
        public bool finished = false;
        public ProgressBox(CancellationTokenSource cancellationTokenSource)
        {
            InitializeComponent();
            this.cancellationTokenSource = cancellationTokenSource;
        }

        private void ProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            cancellationTokenSource.Cancel();
        }
    }
}
