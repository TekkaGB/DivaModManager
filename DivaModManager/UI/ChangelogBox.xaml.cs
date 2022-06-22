using Microsoft.Win32;
using System;
using System.IO;
using System.Media;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Octokit;
using System.Reflection;

namespace DivaModManager.UI
{
    /// <summary>
    /// Interaction logic for ChangelogBox.xaml
    /// </summary>
    public partial class ChangelogBox : Window
    {
        public bool YesNo = false;
        public bool Skip = false;

        public ChangelogBox(GameBananaItemUpdate update, string packageName, string text, Uri preview, bool skip = false)
        {
            InitializeComponent();
            if (preview != null)
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = preview;
                bitmap.EndInit();
                PreviewImage.Source = bitmap;
                PreviewImage.Visibility = Visibility.Visible;
            }
            else
            {
                var bitmap = new BitmapImage(new Uri("pack://application:,,,/DivaModManager;component/Assets/preview.png"));
                PreviewImage.Source = bitmap;
                PreviewImage.Visibility = Visibility.Visible;
            }
            ChangesGrid.ItemsSource = update.Changes;
            Title = $"{packageName} Changelog";
            VersionLabel.Content = $"Update: {update.Title} {update.Version}";
            Text.Text = text;
            // Format/Remove html tags
            update.Text = update.Text.Replace("<br>", "\n").Replace("&nbsp;", " ");
            UpdateText.Text = Regex.Replace(update.Text, "<.*?>", string.Empty);
            if (UpdateText.Text.Length == 0)
                UpdateText.Visibility = Visibility.Collapsed;
            if (skip)
                SkipButton.Visibility = Visibility.Visible;
            else
            {
                Grid.SetColumnSpan(YesButton, 2);
                Grid.SetColumnSpan(NoButton, 2);
            }
            PlayNotificationSound();
        }
        public ChangelogBox(Release release, string packageName, string text, Uri preview, bool skip = false)
        {
            InitializeComponent();
            if (preview != null)
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = preview;
                bitmap.EndInit();
                PreviewImage.Source = bitmap;
                PreviewImage.Visibility = Visibility.Visible;
            }
            else
            {
                var bitmap = new BitmapImage(new Uri("pack://application:,,,/DivaModManager;component/Assets/dml.png"));
                PreviewImage.Source = bitmap;
                PreviewImage.Visibility = Visibility.Visible;
            }
            Title = $"{packageName} Changelog";
            VersionLabel.Content = $"Update: {release.Name}";
            Text.Text = text;
            // Format/Remove html tags
            UpdateText.Text = release.Body;
            if (UpdateText.Text.Length == 0)
                UpdateText.Visibility = Visibility.Collapsed;
            if (skip)
                SkipButton.Visibility = Visibility.Visible;
            else
            {
                Grid.SetColumnSpan(YesButton, 2);
                Grid.SetColumnSpan(NoButton, 2);
            }
            PlayNotificationSound();
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            YesNo = true;
            Close();
        }
        private void Skip_Button_Click(object sender, RoutedEventArgs e)
        {
            Skip = true;
            Close();
        }

        public void PlayNotificationSound()
        {
            bool found = false;
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"AppEvents\Schemes\Apps\.Default\Notification.Default\.Current"))
                {
                    if (key != null)
                    {
                        Object o = key.GetValue(null); // pass null to get (Default)
                        if (o != null)
                        {
                            SoundPlayer theSound = new SoundPlayer((String)o);
                            theSound.Play();
                            found = true;
                        }
                    }
                }
            }
            catch
            { }
            if (!found)
                SystemSounds.Beep.Play(); // consolation prize
        }
    }
}
