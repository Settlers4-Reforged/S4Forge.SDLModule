using S4_GFXBridge.S4Hooks;
using S4_UIEngine.GameTypes;
using S4_UIEngine.Settings;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

internal class GameSettings : IGameSettings {
    public string GetLanguage() {
        try {
            if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\TheSettlers4\Config\GameSettings.cfg")) {
                IniFile s4settings1 = new IniFile(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\TheSettlers4\Config\GameSettings.cfg");
                switch (s4settings1.Read("Language", "GAMESETTINGS")) {
                    default:
                    case ("0"):
                        return "en";
                    case ("1"):
                        return "de";
                }
            }
        } catch { }
        return "en";
    }

    public Tribe GetCurrentTribe() {
        uint player = S4ModAPI.API.GetLocalPlayer();

        return (Tribe)S4ModAPI.API.GetPlayerTribe(player);
    }

    /// <summary>
    /// IniFile is used to read and/or write an ini file.
    /// </summary>
    internal class IniFile {
        private readonly string path = "";
        private readonly string assemblyFileName = Assembly.GetExecutingAssembly().GetName().Name ?? "";
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern long WritePrivateProfileString(string section, string key, string value, string filePath);
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string section, string key, string @default, StringBuilder retVal, int size, string filePath);
        public IniFile(string? iniPath = null) {
            path = new FileInfo(iniPath ?? assemblyFileName + ".ini").FullName ?? "";
        }
        public string Read(string key, string? section = null) {
            var retVal = new StringBuilder(255);
            _ = GetPrivateProfileString(section ?? assemblyFileName, key, "", retVal, 255, path);
            return retVal.ToString();
        }
        public void Write(string key, string value, string? section = null) {
            WritePrivateProfileString(section ?? assemblyFileName, key, value, path);
        }
    }
}