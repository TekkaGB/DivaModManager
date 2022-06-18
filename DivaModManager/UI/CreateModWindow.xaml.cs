using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Reflection;
using System.Windows.Input;
using System.Windows.Controls.Primitives;
using System.Collections.ObjectModel;
using Tomlyn;
using Tomlyn.Model;
using System.Diagnostics;

namespace DivaModManager.UI
{
    /// <summary>
    /// Interaction logic for CreateModWindow.xaml
    /// </summary>
    public partial class CreateModWindow : Window
    {
        public TomlTable config;
        public CreateModWindow()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var path = String.Join(String.Empty, NameBox.Text.Split(Path.GetInvalidFileNameChars()));
            if (Directory.Exists($"{Global.config.Configs[Global.config.CurrentGame].ModsFolder}{Global.s}{path}"))
            {
                Global.logger.WriteLine($"{path} already exists in your mod folder, please choose another name", LoggerType.Warning);
                return;
            }
            config = new();
            config.Add("enabled", true);
            config.Add("include", new string[1] { "." });
            if (!String.IsNullOrWhiteSpace(NameBox.Text))
                config.Add("name", NameBox.Text);
            if (!String.IsNullOrWhiteSpace(AuthorBox.Text))
                config.Add("author", AuthorBox.Text);
            if (!String.IsNullOrWhiteSpace(VersionBox.Text))
                config.Add("version", VersionBox.Text);
            if (!String.IsNullOrWhiteSpace(DateBox.Text))
                config.Add("date", DateBox.Text);
            if (!String.IsNullOrWhiteSpace(DescriptionBox.Text))
                config.Add("description", DescriptionBox.Text);
            Directory.CreateDirectory($"{Global.config.Configs[Global.config.CurrentGame].ModsFolder}{Global.s}{path}");
            var configFile = Toml.FromModel(config);
            File.WriteAllText($"{Global.config.Configs[Global.config.CurrentGame].ModsFolder}{Global.s}{path}{Global.s}config.toml", configFile);
            Global.logger.WriteLine($"Created {NameBox.Text}!", LoggerType.Info);
            try
            {
                Process process = Process.Start("explorer.exe", $"{Global.config.Configs[Global.config.CurrentGame].ModsFolder}{Global.s}{path}");
                Global.logger.WriteLine($@"Opened {Global.config.Configs[Global.config.CurrentGame].ModsFolder}{Global.s}{path}.", LoggerType.Info);
            }
            catch (Exception ex)
            {
                Global.logger.WriteLine($@"Couldn't open {Global.config.Configs[Global.config.CurrentGame].ModsFolder}{Global.s}{path}. ({ex.Message})", LoggerType.Error);
            }
            Close();
        }

        private void NameBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(NameBox.Text))
                SaveButton.IsEnabled = true;
            else
                SaveButton.IsEnabled = false;
        }
    }
}
