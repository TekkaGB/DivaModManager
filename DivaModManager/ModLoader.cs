using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Tomlyn;
using System.Threading.Tasks;
using Tomlyn.Model;

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
            var configString = String.Empty;
            while (String.IsNullOrEmpty(configString))
            {
                try
                {
                    configString = File.ReadAllText(configPath);
                }
                catch (Exception e)
                {
                    // Check if the exception is related to an IO error.
                    if (e.GetType() != typeof(IOException))
                    {
                        Global.logger.WriteLine($"Couldn't access {configPath} ({e.Message})", LoggerType.Error);
                        break;
                    }
                }
            }
            if (!Toml.TryToModel(configString, out TomlTable config, out var diagnostics))
            {
                // Create a new config if it failed to parse
                config = new();
                config.Add("enabled", true);
                config.Add("console", false);
                config.Add("mods", "mods");
            }
            var priorityList = new List<string>();
            foreach (var mod in Global.config.Configs[Global.config.CurrentGame].Loadouts[Global.config.Configs[Global.config.CurrentGame].CurrentLoadout].Where(x => x.enabled).ToList())
                priorityList.Add(mod.name);
            config["priority"] = priorityList.ToArray();
            var isReady = false;
            while (!isReady)
            {
                try
                {
                    File.WriteAllText(configPath, Toml.FromModel(config));
                    isReady = true;
                }
                catch (Exception e)
                {
                    // Check if the exception is related to an IO error.
                    if (e.GetType() != typeof(IOException))
                    {
                        Global.logger.WriteLine($"Couldn't access {configPath} ({e.Message})", LoggerType.Error);
                        break;
                    }
                }
            }
        }
    }
}
