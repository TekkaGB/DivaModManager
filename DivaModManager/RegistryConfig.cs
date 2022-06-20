using Microsoft.Win32;
using System.IO;
using System.Reflection;

namespace DivaModManager
{
    public static class RegistryConfig
    {
        public static bool InstallGBHandler()
        {
            string AppPath = $"{Global.assemblyLocation}{Global.s}DivaModManager.exe";
            string protocolName = $"divamodmanager";
            try
            {
                var reg = Registry.CurrentUser.CreateSubKey(@"Software\Classes\DivaModManager");
                reg.SetValue("", $"URL:{protocolName}");
                reg.SetValue("URL Protocol", "");
                reg = reg.CreateSubKey(@"shell\open\command");
                reg.SetValue("", $"\"{AppPath}\" -download \"%1\"");
                reg.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
