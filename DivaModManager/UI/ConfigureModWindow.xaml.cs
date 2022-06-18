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

namespace DivaModManager.UI
{
    /// <summary>
    /// Interaction logic for ConfigureModWindow.xaml
    /// </summary>
    public partial class ConfigureModWindow : Window
    {
        public Mod _mod;
        private string configPath;
        public ConfigureModWindow(Mod mod)
        {
            InitializeComponent();
            if (mod != null)
            {
                _mod = mod;
                configPath = $"{Global.config.Configs[Global.config.CurrentGame].ModsFolder}{Global.s}{mod.name}{Global.s}config.toml";
                var configString = File.ReadAllText(configPath);
                ConfigBox.Text = configString;
                Title = $"Configure {_mod.name}";
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var config = Toml.ToModel(ConfigBox.Text);
            }
            catch (Exception ex)
            {
                Global.logger.WriteLine($"Invalid config: {ex.Message}", LoggerType.Error);
                return;
            }
            File.WriteAllText(configPath, ConfigBox.Text);
            Global.logger.WriteLine($"Successfully saved config!", LoggerType.Info);
        }
    }
}
