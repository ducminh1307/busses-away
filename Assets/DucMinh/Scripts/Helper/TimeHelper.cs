using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using UnityEngine;

namespace DucMinh
{
    public static class TimeHelper
    {
        // private static readonly HttpClient Client = new HttpClient();
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private static float _startSecond;
        private static DateTime? _globalTime;
        private static DateTime _localDateTime;
        private static int _hackSecond;

        private static bool _inited = false;
        
        // private static TimeSpan _serverOffset = TimeSpan.Zero;

        private static void LazyInit()
        {
            if (_inited) return;
            
            _inited = true;
        }
        
        public static void Init(Action callback = null)
        {
            _startSecond = SecondFromStart;
            _localDateTime = LocalDateTime;
            _globalTime = GlobalTime;
            callback?.Invoke();
        }

        private static float SecondFromStart => Time.realtimeSinceStartup;

        public static DateTime LocalDateTime
        {
            get
            {
                var now = DateTime.Now;
#if DEBUG_MODE
                var hackSecond = HackSecond;
                if (HackSecond != 0)
                {
                    now = now.AddSeconds(hackSecond);
                }
#endif
                return now;
            }
        }

        public static DateTime? GlobalTimeFromStart
        {
            get
            {
                if (_globalTime.HasValue)
                {
                    var deltaSecond = SecondFromStart - _startSecond;
#if DEBUG_MODE
                    deltaSecond += HackSecond;
#endif
                    return _globalTime.Value.AddSeconds(deltaSecond);
                }
                return null;
            }
        }

        public static DateTime? GlobalTime
        {
            get
            {
                try
                {
                    var request = WebRequest.Create("https://www.google.com/");
                    request.Timeout = 1000;
                    using var response = request.GetResponse();
                    return DateTime.ParseExact(response.Headers["date"], "ddd, dd MM yyyy HH:mm:ss 'GMT'", CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.AssumeUniversal);
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public static int HackSecond
        {
            get
            {
                var hackSecond = StorageService.GetInt("HackSecond", 0);
                _hackSecond = hackSecond;
                return _hackSecond;
            }
            set
            {
                StorageService.SetInt("HackSecond", value);
                _hackSecond = value;
            }
        }

        public static DateTime CurrentTime
        {
            get
            {
                var globalTime = GlobalTimeFromStart;
                return globalTime ?? LocalDateTime;
            }
        }
        
        public static double SecondsBetween(DateTime start, DateTime end)
        {
            var difference = end - start;
            return difference.TotalSeconds;
        }

        public static double SecondsFromCurrent(DateTime end)
        {
            return SecondsBetween(CurrentTime, end);
        }

        public static bool IsOver(DateTime start, DateTime end)
        {
            return SecondsBetween(end, start) > 0;
        }

        public static bool IsCurrentOVer(DateTime end)
        {
            return IsOver(CurrentTime, end);
        }
        
        public static long ToUnixTimestamp(DateTime dateTime)
        {
            return (long)(dateTime.ToUniversalTime() - UnixEpoch).TotalSeconds;
        }
        
        public static DateTime FromUnixTimestamp(long timestamp)
        {
            return UnixEpoch.AddSeconds(timestamp).ToLocalTime();
        }
        
        public static long ToUnixTimestampMilliseconds(DateTime dateTime)
        {
            return (long)(dateTime.ToUniversalTime() - UnixEpoch).TotalMilliseconds;
        }
        
        public static DateTime FromUnixTimestampMilliseconds(long milliseconds)
        {
            return UnixEpoch.AddMilliseconds(milliseconds).ToLocalTime();
        }
        
        public static string ToFormattedString(DateTime dateTime, string format = "dd/MM/yyyy HH:mm:ss")
        {
            return dateTime.ToString(format);
        }
        
        public static string ToShortDateString(DateTime dateTime) => dateTime.ToString("dd/MM/yyyy");
        public static string ToLongDateString(DateTime dateTime) => dateTime.ToString("dddd, dd MMMM yyyy");
        public static string ToShortTimeString(DateTime dateTime) => dateTime.ToString("HH:mm");
        public static string ToLongTimeString(DateTime dateTime) => dateTime.ToString("HH:mm:ss");
        
        public static double ConvertToSeconds(TimeSpan timeSpan)
        {
            return timeSpan.TotalSeconds;
        }

        public static double ConvertToMinutes(TimeSpan timeSpan)
        {
            return timeSpan.TotalMinutes;
        }

        public static double ConvertToHours(TimeSpan timeSpan)
        {
            return timeSpan.TotalHours;
        }

        public static double ConvertToDays(TimeSpan timeSpan)
        {
            return timeSpan.TotalDays;
        }
        
        public static string SecondsToFormattedString(double seconds)
        {
            var timeSpan = TimeSpan.FromSeconds(seconds);
            if (timeSpan.TotalDays >= 1)
                return $"{(int)timeSpan.TotalDays}d {timeSpan.Hours}h {timeSpan.Minutes}m {timeSpan.Seconds}s";
            if (timeSpan.TotalHours >= 1)
                return $"{timeSpan.Hours}h {timeSpan.Minutes}m {timeSpan.Seconds}s";
            if (timeSpan.TotalMinutes >= 1)
                return $"{timeSpan.Minutes}m {timeSpan.Seconds}s";
            return $"{timeSpan.Seconds}s";
        }
    }
}