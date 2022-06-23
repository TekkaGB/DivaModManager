using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Tomlyn;
using System.Threading.Tasks;

namespace DivaModManager
{
    public static class ModLoader
    {
        public static async void Build()
        {
            var configPath = $"{Path.GetDirectoryName(Global.config.Configs[Global.config.CurrentGame].ModsFolder)}{Global.s}config.toml";
            if (!File.Exists(configPath))
            {
                Global.logger.WriteLine($"Unable to find {configPath}", LoggerType.Error);
                return;
            }
            if (await IsFileReady(configPath))
            {
                var configString = File.ReadAllText(configPath);
                var config = Toml.ToModel(configString);
                var priorityList = new List<string>();
                foreach (var mod in Global.config.Configs[Global.config.CurrentGame].Loadouts[Global.config.Configs[Global.config.CurrentGame].CurrentLoadout].Where(x => x.enabled).ToList())
                    priorityList.Add(mod.name);
                config["priority"] = priorityList.ToArray();
                File.WriteAllText(configPath, Toml.FromModel(config));
            }
        }
        // Failsafe for loadout being edited too quickly
        private static async Task<bool> IsFileReady(string filename)
        {
            var isReady = false;
            await Task.Run(() =>
            {
                while (!isReady)
                {
                    // If the file can be opened for exclusive access it means that the file
                    // is no longer locked by another process.
                    try
                    {
                        using (FileStream inputStream =
                            File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.None))
                            isReady = inputStream.Length > 0;
                    }
                    catch (Exception e)
                    {
                        // Check if the exception is related to an IO error.
                        if (e.GetType() == typeof(IOException))
                        {
                            isReady = false;
                        }
                        else
                        {
                            Global.logger.WriteLine($"Couldn't access {filename} ({e.Message})", LoggerType.Error);
                            break;
                        }
                    }
                }
            });
            return isReady;
        }
    }
}
