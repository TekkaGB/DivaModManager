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
        public static void Build(List<Mod> mods)
        {
            var configPath = $"{Path.GetDirectoryName(Global.config.Configs[Global.config.CurrentGame].ModsFolder)}{Global.s}config.toml";
            var configString = File.ReadAllText(configPath);
            var config = Toml.ToModel(configString);
            var priorityList = new List<string>();
            foreach (var mod in mods)
                priorityList.Add(mod.name);
            config["priority"] = priorityList.ToArray();
            File.WriteAllText(configPath, Toml.FromModel(config));
            Global.logger.WriteLine("Finished building!", LoggerType.Info);
        }
    }
}
