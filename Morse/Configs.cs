using System.Security.AccessControl;
using Microsoft.Win32;

namespace Morse
{
    public static class Configs
    {
        public const string Rev = "Rev";
        public const string Sed = "Sed";
        private const string RegistryPath = "SOFTWARE\\Morse";
        private const string ModeRegName = "LastMode";
        private const string GridRowRegName = "GridRowCount";
        private const string GridColRegName = "GridColCount";

        private static int _gridRowCount = -1;
        private static int _gridColCount = -1;

        public static string GetModeConfig()
        {
            return ReadRegValue(ModeRegName, Sed);
        }

        public static void SetModeConfig(string mode)
        {
            SetRegValue(ModeRegName, mode);
        }

        public static int GridRowCount
        {
            get
            {
                if (_gridRowCount < 0)
                {
                    _gridRowCount = int.Parse(ReadRegValue(GridRowRegName, "40")); //64
                }

                return _gridRowCount;
            }
        }

        public static int GridColCount
        {
            get
            {
                if (_gridColCount < 0)
                {
                    _gridColCount = int.Parse(ReadRegValue(GridColRegName, "25")); //45
                }

                return _gridColCount;
            }
        }
        
        public static int BlockCount => (GridColCount * GridRowCount);

        private static string ReadRegValue(string valueName, string defaultValue)
        {
            using (var key = Registry.CurrentUser.OpenSubKey(RegistryPath))
            {
                if (key == null)
                {
                    SetRegValue(valueName, defaultValue);
                    return defaultValue;
                }

                var value = key.GetValue(valueName);
                if (value != null) return value.ToString();

                SetRegValue(valueName, defaultValue);
                return defaultValue;
            }
        }

        private static void SetRegValue(string valueName, string value)
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryPath, true))
            {
                if (key == null)
                {
                    var newKey = Registry.CurrentUser.CreateSubKey(RegistryPath);
                    newKey.SetValue(valueName, value);
                }
                else
                {
                    key.SetValue(valueName, value);
                }
            }
        }
    }
}