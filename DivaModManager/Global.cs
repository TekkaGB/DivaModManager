using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DivaModManager
{
    public static class Global
    {
        public static Config config;
        public static Logger logger;
        public static char s = Path.DirectorySeparatorChar;
        public static string assemblyLocation = AppDomain.CurrentDomain.BaseDirectory;
        public static List<string> games;
        public static ObservableCollection<Mod> ModList;
        // After applying the filter, for backup purposes
        public static ObservableCollection<Mod> ModList_All;
        public static ObservableCollection<String> LoadoutItems;
        public static void UpdateConfig()
        {
            config.Configs[config.CurrentGame].Loadouts[config.Configs[config.CurrentGame].CurrentLoadout] = ModList;
            string configString = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            var isReady = false;
            while (!isReady)
            {
                try
                {
                    File.WriteAllText($@"{assemblyLocation}{s}Config.json", configString);
                    isReady = true;
                }
                catch (Exception e)
                {
                    // Check if the exception is related to an IO error.
                    if (e.GetType() != typeof(IOException))
                    {
                        Global.logger.WriteLine($"Couldn't write to Config.json ({e.Message})", LoggerType.Error);
                        break;
                    }
                }
            }
        }
    }
}
