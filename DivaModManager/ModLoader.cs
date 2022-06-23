using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Tomlyn;

namespace DivaModManager
{
    public static class ModLoader
    {
        public static void Build()
        {
            var configPath = $"{Path.GetDirectoryName(Global.config.Configs[Global.config.CurrentGame].ModsFolder)}{Global.s}config.toml";
            if (!File.Exists(configPath))
            {
                Global.logger.WriteLine($"Unable to find {configPath}", LoggerType.Error);
                return;
            }
            var configString = File.ReadAllText(configPath);
            var config = Toml.ToModel(configString);
            var priorityList = new List<string>();
            foreach (var mod in Global.config.Configs[Global.config.CurrentGame].Loadouts[Global.config.Configs[Global.config.CurrentGame].CurrentLoadout].Where(x => x.enabled).ToList())
                priorityList.Add(mod.name);
            config["priority"] = priorityList.ToArray();
            File.WriteAllText(configPath, Toml.FromModel(config));
        }
    }
}
