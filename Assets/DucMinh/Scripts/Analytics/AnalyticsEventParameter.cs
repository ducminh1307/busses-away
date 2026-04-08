using System.Collections.Generic;
using System.Text;

namespace DucMinh
{
    public enum AnalyticsEventParameterType
    {
        String,
        Long,
        Double,
        Dictionary,
        DictionaryList
    }

    public readonly struct AnalyticsEventParameter
    {
        private readonly object _referenceValue;

        public string Key { get; }
        public AnalyticsEventParameterType Type { get; }
        public long LongValue { get; }
        public double DoubleValue { get; }

        public AnalyticsEventParameter(string key, string value)
        {
            Key = key;
            Type = AnalyticsEventParameterType.String;
            LongValue = default;
            DoubleValue = default;
            _referenceValue = value ?? string.Empty;
        }

        public AnalyticsEventParameter(string key, long value)
        {
            Key = key;
            Type = AnalyticsEventParameterType.Long;
            LongValue = value;
            DoubleValue = default;
            _referenceValue = null;
        }

        public AnalyticsEventParameter(string key, double value)
        {
            Key = key;
            Type = AnalyticsEventParameterType.Double;
            LongValue = default;
            DoubleValue = value;
            _referenceValue = null;
        }

        public AnalyticsEventParameter(string key, IDictionary<string, object> value)
        {
            Key = key;
            Type = AnalyticsEventParameterType.Dictionary;
            LongValue = default;
            DoubleValue = default;
            _referenceValue = value;
        }

        public AnalyticsEventParameter(string key, IEnumerable<IDictionary<string, object>> value)
        {
            Key = key;
            Type = AnalyticsEventParameterType.DictionaryList;
            LongValue = default;
            DoubleValue = default;
            _referenceValue = value;
        }

        public string StringValue => _referenceValue as string ?? string.Empty;
        public IDictionary<string, object> DictionaryValue => _referenceValue as IDictionary<string, object>;
        public IEnumerable<IDictionary<string, object>> DictionaryListValue => _referenceValue as IEnumerable<IDictionary<string, object>>;

        public static AnalyticsEventParameter FromObject(string key, object value)
        {
            return value switch
            {
                string stringValue => new AnalyticsEventParameter(key, stringValue),
                long longValue => new AnalyticsEventParameter(key, longValue),
                int intValue => new AnalyticsEventParameter(key, (long)intValue),
                double doubleValue => new AnalyticsEventParameter(key, doubleValue),
                float floatValue => new AnalyticsEventParameter(key, (double)floatValue),
                IDictionary<string, object> dictionaryValue => new AnalyticsEventParameter(key, dictionaryValue),
                IEnumerable<IDictionary<string, object>> dictionaryListValue => new AnalyticsEventParameter(key, dictionaryListValue),
                null => new AnalyticsEventParameter(key, string.Empty),
                _ => new AnalyticsEventParameter(key, value.ToString() ?? string.Empty)
            };
        }

        public string GetDisplayValue()
        {
            return Type switch
            {
                AnalyticsEventParameterType.String => StringValue,
                AnalyticsEventParameterType.Long => LongValue.ToString(),
                AnalyticsEventParameterType.Double => DoubleValue.ToString(),
                AnalyticsEventParameterType.Dictionary => FormatDictionary(DictionaryValue),
                AnalyticsEventParameterType.DictionaryList => FormatDictionaryList(DictionaryListValue),
                _ => string.Empty
            };
        }

        private static string FormatDictionary(IDictionary<string, object> dictionary)
        {
            if (dictionary == null || dictionary.Count == 0) return "{}";

            var sb = new StringBuilder();
            sb.Append('{');
            int index = 0;
            foreach (var pair in dictionary)
            {
                sb.Append($"{pair.Key}: {pair.Value}");
                if (index < dictionary.Count - 1)
                {
                    sb.Append(", ");
                }

                index++;
            }

            sb.Append('}');
            return sb.ToString();
        }

        private static string FormatDictionaryList(IEnumerable<IDictionary<string, object>> dictionaries)
        {
            if (dictionaries == null) return "[]";

            var sb = new StringBuilder();
            sb.Append('[');

            bool hasAny = false;
            foreach (var dictionary in dictionaries)
            {
                if (hasAny)
                {
                    sb.Append(", ");
                }

                sb.Append(FormatDictionary(dictionary));
                hasAny = true;
            }

            sb.Append(']');
            return sb.ToString();
        }
    }
}
