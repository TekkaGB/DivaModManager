using System.ComponentModel;
using System.Threading;
using System.Windows;

namespace DivaModManager.UI
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
