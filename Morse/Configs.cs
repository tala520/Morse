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
        private const string DataRowRegName = "DataRowCount";
        private const string DataColRegName = "DataColCount";
        private const string SendIntervalRegName = "SendInterval";
        private const string ReceiveIntervalRegName = "ReceiveInterval";
        private const string AutoClickCycleRegName = "AutoClickCycle";

        private static int _dataRowCount = -1;
        private static int _dataColCount = -1;
        private static int _sendInterval = -1;
        private static int _receiveInterval = -1;
        private static int _autoClickCycle = -1;
        
        public static string GetModeConfig()
        {
            return ReadRegValue(ModeRegName, Sed);
        }

        public static void SetModeConfig(string mode)
        {
            SetRegValue(ModeRegName, mode);
        }

        public static int DataRowCount
        {
            get
            {
                if (_dataRowCount < 0)
                {
                    _dataRowCount = int.Parse(ReadRegValue(DataRowRegName, "32"));
                }

                return _dataRowCount;
            }
        }

        public static int DataColCount
        {
            get
            {
                if (_dataColCount < 0)
                {
                    _dataColCount = int.Parse(ReadRegValue(DataColRegName, "64"));
                }

                return _dataColCount;
            }
        }
        
        public static int DataBlockByteSize => (DataColCount * DataRowCount);
        
        public static int SendInterval
        {
            get
            {
                if (_sendInterval < 0)
                {
                    _sendInterval = int.Parse(ReadRegValue(SendIntervalRegName, "1000"));
                }

                return _sendInterval;
            }
        }
        
        public static int ReceiveInterval
        {
            get
            {
                if (_receiveInterval < 0)
                {
                    _receiveInterval = int.Parse(ReadRegValue(ReceiveIntervalRegName, "500"));
                }

                return _receiveInterval;
            }
        }
        
        public static int AutoClickCycle
        {
            get
            {
                if (_autoClickCycle < 0)
                {
                    _autoClickCycle = int.Parse(ReadRegValue(AutoClickCycleRegName, "10"));
                }

                return _autoClickCycle;
            }
        }

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