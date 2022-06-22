using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Reflection;
using System.Windows.Input;

namespace DivaModManager.UI
{
    /// <summary>
    /// Interaction logic for EditWindow.xaml
    /// </summary>
    public partial class EditWindow : Window
    {
        public string _name;
        public bool _folder;
        public string newName;
        public string loadout = null;
        public EditWindow(string name, bool folder)
        {
            InitializeComponent();
            _folder = folder;
            if (!String.IsNullOrEmpty(name))
            {
                _name = name;
                NameBox.Text = name;
                Title = $"Edit {name}";
            }
            else
                Title = "Create New Loadout";
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (_folder)
                EditFolderName();
            else
                CreateLoadoutName();
        }
        private void CreateLoadoutName()
        {
            if (String.IsNullOrWhiteSpace(NameBox.Text))
            {
                Global.logger.WriteLine($"Invalid loadout name", LoggerType.Error);
                return;
            }
            if (!Global.config.Configs[Global.config.CurrentGame].Loadouts.ContainsKey(NameBox.Text))
            {
                loadout = NameBox.Text;
                Close();
            }
            else
                Global.logger.WriteLine($"{NameBox.Text} already exists", LoggerType.Error);
        }
        private void EditFolderName()
        {
            if (!NameBox.Text.Equals(_name, StringComparison.InvariantCultureIgnoreCase))
            {
                var oldDirectory = $"{Global.config.Configs[Global.config.CurrentGame].ModsFolder}{Global.s}{Global.s}{_name}";
                var newDirectory = $"{Global.config.Configs[Global.config.CurrentGame].ModsFolder}{Global.s}{Global.s}{NameBox.Text}";
                if (!Directory.Exists(newDirectory))
                {
                    try
                    {
                        Directory.Move(oldDirectory, newDirectory);
                        // Rename in every single loadout
                        foreach (var key in Global.config.Configs[Global.config.CurrentGame].Loadouts.Keys)
                        {
                            var index = Global.config.Configs[Global.config.CurrentGame].Loadouts[key].ToList().FindIndex(x => x.name == _name);
                            Global.config.Configs[Global.config.CurrentGame].Loadouts[key][index].name = NameBox.Text;
                        }
                        Global.ModList = Global.config.Configs[Global.config.CurrentGame].Loadouts[Global.config.Configs[Global.config.CurrentGame].CurrentLoadout];
                        Close();
                    }
                    catch (Exception ex)
                    {
                        Global.logger.WriteLine($"Couldn't rename {oldDirectory} to {newDirectory} ({ex.Message})", LoggerType.Error);
                    }
                }
                else
                    Global.logger.WriteLine($"{newDirectory} already exists", LoggerType.Error);
            }
        }

        private void NameBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                if (_folder)
                    EditFolderName();
                else
                    CreateLoadoutName();
            }
        }
    }
}
