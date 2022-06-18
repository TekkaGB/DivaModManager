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
        public static string assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static List<string> games;
        public static ObservableCollection<Mod> ModList;
        public static void UpdateConfig()
        {
            config.Configs[config.CurrentGame].ModList = ModList;
            string configString = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            try
            {
                File.WriteAllText($@"{assemblyLocation}{s}Config.json", configString);
            }
            catch (Exception e)
            {
                logger.WriteLine($"Couldn't write Config.json ({e.Message})", LoggerType.Error);
            }
        }
    }
}
