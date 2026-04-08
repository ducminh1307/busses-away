using System;

namespace DucMinh
{
    public static class DataFactory
    {
        public static IDataController<int> CreateIntDataController(string key, string name, int defaultValue = 0)
        {
            return new IntDataController(key, name, defaultValue);
        }
        
        public static IDataController<float> CreateFloatDataController(string key, string name, float defaultValue = 0)
        {
            return new FloatDataController(key, name, defaultValue);
        }
        
        public static IDataController<string> CreateStringDataController(string key, string name, string defaultValue = null)
        {
            return new StringDataController(key, name, defaultValue);
        }

        public static IDataController<bool> CreateBoolDataController(string key, string name, bool defaultValue = false)
        {
            return new BoolDataController(key, name, defaultValue);
        }

        public static IDataController<DateTime?> CreateDateTimeDataController(string key, string name, DateTime defaultValue = default)
        {
            return new DateTimeController(key, name, defaultValue);
        }

        public static IDataController<T> CreateEnumDataController<T>(string key, string name, T defaultValue = default, int maxColumn = 1) where T : Enum
        {
            return new EnumDataController<T>(key, name, defaultValue, maxColumn);
        }
        
        public static IDataController<int[]> CreateIntArrayDataController(string key, string name, int[] defaultValue = null)
        {
            return new IntArrayDataController(key, name, defaultValue);
        }
    }
}