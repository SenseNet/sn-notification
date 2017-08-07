﻿using System;
using System.Text;
using SenseNet.Diagnostics;

namespace SenseNet.Notification
{
    internal class Configuration
    {
        private const string SECTIONNAME = "sensenet/notification";

        private const string ENABLEDKEY = "MasterSwitch";
        private const string GROUPBYUSERKEY = "GroupNotificationsByUser";

        // ----------------------------------------

        private const string TIMERINTERVALKEY = "TimerInterval";
        private const double DefaultTimerInterval = 5.0;
        private static double? _timerInterval;

        internal static double TimerInterval => _timerInterval ?? DefaultTimerInterval;

        // ----------------------------------------

        private const string IMMEDIATELYKEY = "Immediately";
        private const string DAILYKEY = "Daily";
        private const string WEEKLYKEY = "Weekly";
        private const string MONTHLYKEY = "Monthly";

        internal static bool ImmediatelyEnabled { get; set; }

        internal static bool DailyEnabled { get; set; }
        private static int _dailyHour;
        internal static int DailyHour
        {
            get { return _dailyHour; }
            set { _dailyHour = value; }
        }
        private static int _dailyMinute;
        internal static int DailyMinute
        {
            get { return _dailyMinute; }
            set { _dailyMinute = value; }
        }

        internal static bool WeeklyEnabled { get; set; }
        private static DayOfWeek _weeklyWeekDay;
        internal static DayOfWeek WeeklyWeekDay
        {
            get { return _weeklyWeekDay; }
            set { _weeklyWeekDay = value; }
        }
        private static int _weeklyHour;
        internal static int WeeklyHour
        {
            get { return _weeklyHour; }
            set { _weeklyHour = value; }
        }
        private static int _weeklyMinute;
        internal static int WeeklyMinute
        {
            get { return _weeklyMinute; }
            set { _weeklyMinute = value; }
        }

        internal static bool MonthlyEnabled { get; set; }
        internal static bool MonthlyLast { get; set; }

        private static int _monthlyWeek;
        internal static int MonthlyWeek
        {
            get { return _monthlyWeek; }
            set { _monthlyWeek = value; }
        }
        private static DayOfWeek _monthlyWeekDay;
        internal static DayOfWeek MonthlyWeekDay
        {
            get { return _monthlyWeekDay; }
            set { _monthlyWeekDay = value; }
        }

        internal static bool MonthlyEvery { get; set; }

        private static int _monthlyDay;
        internal static int MonthlyDay
        {
            get { return _monthlyDay; }
            set { _monthlyDay = value; }
        }
        private static int _monthlyHour;
        internal static int MonthlyHour
        {
            get { return _monthlyHour; }
            set { _monthlyHour = value; }
        }
        private static int _monthlyMinute;
        internal static int MonthlyMinute
        {
            get { return _monthlyMinute; }
            set { _monthlyMinute = value; }
        }

        // ----------------------------------------

        internal static string SenderAddress { get; set; }
        internal static int RetryCount { get; set; }
        internal static int RetryDelay { get; set; }
        internal static int TakeCount { get; set; }
        internal static Encoding MessageEncoding { get; set; }
        
        private const string SENDERADDRESSKEY = "NotificationSenderAddress";
        private const string RETRYCOUNT = "RetryCount";
        private const string RETRYDELAY = "RetryDelay";
        private const string TAKECOUNT = "TakeCount";
        private const string MESSAGEENCODING = "MessageEncoding";

        private const string DefaultSenderAddress = "noreply@example.com";
        private const int DefaultRetryCount = 3;
        private const int DefaultRetryDelay = 2000;
        private const int DefaultTakeCount = 20;
        private static readonly Encoding DefaultMessageEncoding = Encoding.UTF8;

        public static bool Enabled { get; internal set; }
        public static bool GroupNotificationsByUser { get; internal set; }

        // ----------------------------------------

        static Configuration()
        {
            Parse();
        }
        private static void Parse()
        {
            var collection = System.Configuration.ConfigurationManager.GetSection(SECTIONNAME) as System.Collections.Specialized.NameValueCollection;
            if (collection == null)
            {
                ParseEnabled(null);
                ParseGroupByUser(null);

                ParseTimerInterval(null);

                ImmediatelyEnabled = true;
                DailyEnabled = true;
                WeeklyEnabled = true;
                MonthlyEnabled = true;
                ParseDaily(null);
                ParseWeekly(null);
                ParseMonthly(null);

                ParseSenderAddress(null);
                ParseRetryCount(null);
                ParseRetryDelay(null);
                ParseTakeCount(null);
                ParseEncoding(null);
            }
            else
            {
                ParseEnabled(collection.Get(ENABLEDKEY));
                ParseGroupByUser(collection.Get(GROUPBYUSERKEY));

                ParseTimerInterval(collection.Get(TIMERINTERVALKEY));

                ParseImmediately(collection.Get(IMMEDIATELYKEY));
                ParseDaily(collection.Get(DAILYKEY));
                ParseWeekly(collection.Get(WEEKLYKEY));
                ParseMonthly(collection.Get(MONTHLYKEY));

                ParseSenderAddress(collection.Get(SENDERADDRESSKEY));
                ParseRetryCount(collection.Get(RETRYCOUNT));
                ParseRetryDelay(collection.Get(RETRYDELAY));
                ParseTakeCount(collection.Get(TAKECOUNT));
                ParseEncoding(collection.Get(MESSAGEENCODING));
            }
        }

        private static void ParseEnabled(string setting)
        {
            if (setting != null)
                Enabled = setting.ToUpper() == "ON";
            else
                Enabled = true;
        }

        private static void ParseGroupByUser(string setting)
        {
            bool gbu;

            GroupNotificationsByUser = !bool.TryParse(setting ?? "true", out gbu) || gbu;
        }

        private static void ParseTimerInterval(string setting)
        {
            if (setting == null)
            {
                _timerInterval = DefaultTimerInterval;
                return;
            }
            double @double;
            _timerInterval =
                double.TryParse(setting, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out @double)
                ?
                @double
                :
                DefaultTimerInterval;
         }

        internal static void ParseImmediately(string setting)
        {
            ImmediatelyEnabled = IsNever(setting);
        }
        internal static void ParseDaily(string setting)
        {
            DailyEnabled = IsNever(setting);
            if (!DailyEnabled)
                return;

            if (setting == null)
            {
                DailyHour = 1;
                DailyMinute = 0;
                return;
            }
            ParseTime(setting, "Daily", out _dailyHour, out _dailyMinute);
        }
        internal static void ParseWeekly(string setting)
        {
            WeeklyEnabled = IsNever(setting);
            if (!WeeklyEnabled)
                return;

            if (setting == null)
            {
                WeeklyWeekDay = DayOfWeek.Monday;
                WeeklyHour = 1;
                WeeklyMinute = 0;
                return;
            }
            var sa = setting.Split(' ');
            if (sa.Length != 2)
                throw ConfigurationExceptionHelper("Weekly", "Invalid format. Expected: 'Weekday Hour:Minute' (e.g. 'Sunday 23:00').");

            ParseWeekday(sa[0], "Weekly", out _weeklyWeekDay);
            ParseTime(sa[1], "Weekly", out _weeklyHour, out _weeklyMinute);
        }
        internal static void ParseMonthly(string setting)
        {
            MonthlyEnabled = IsNever(setting);
            if (!MonthlyEnabled)
                return;

            if (setting == null)
            {
                MonthlyEvery = true;
                MonthlyLast = false;
                MonthlyDay = 1;
                MonthlyHour = 1;
                MonthlyMinute = 0;
                return;
            }
            var sa = setting.Split(' ');
            if (sa.Length != 3)
                throw ConfigurationExceptionHelper("Monthly", @"Invalid format. Expected: 'Number. Weekday Hour:Minute' (e.g. '1. Sunday 23:00'), 
or 'Last Weekday Hour:Minute' (e.g. 'Last Sunday 23:00'), or 'Every Day. Hour:Minute' (e.g. 'Every 10. 23:10')");

            var firstWord = sa[0].ToLower();
            if (firstWord == "every")
            {
                // Every 10, 23:10
                MonthlyEvery = true;
                MonthlyLast = false;
                if (sa[1][sa[1].Length - 1] != ',')
                    throw ConfigurationExceptionHelper("Monthly", @"Missing ',' after the day number. Valid format is: 'Every Day. Hour:Minute' (e.g. 'Every 10, 23:10')");
                if (!int.TryParse(sa[1].TrimEnd(','), out _monthlyDay))
                    throw ConfigurationExceptionHelper("Monthly", @"Invalid day number. Expected: 'Every Day, Hour:Minute' (e.g. 'Every 10, 23:10')");
                if (MonthlyDay < 1 || MonthlyDay > 31)
                    throw ConfigurationExceptionHelper("Monthly", "Day is out of range: 1-31.");
            }
            else if (firstWord == "last")
            {
                // Last Sunday 23:10
                MonthlyEvery = false;
                MonthlyLast = true;
                ParseWeekday(sa[1], "Monthly", out _monthlyWeekDay);
            }
            else
            {
                // 3th Sunday 23:10
                MonthlyEvery = false;
                MonthlyLast = false;

                switch (sa[0])
	            {
                    case "1st": _monthlyWeek = 1; break;
                    case "2nd": _monthlyWeek = 2; break;
                    case "3rd": _monthlyWeek = 3; break;
                    case "4th": _monthlyWeek = 4; break;
                    default: throw ConfigurationExceptionHelper("Monthly", "Week number is out of range. Available values: 1st, 2nd, 3rd, 4th");
                }

                ParseWeekday(sa[1], "Monthly", out _monthlyWeekDay);
            }
            ParseTime(sa[2], "Monthly", out _monthlyHour, out _monthlyMinute);
        }
        private static bool IsNever(string setting)
        {
            if (setting == null)
                return true;
            return setting.ToLower() != "never";
        }

        private static void ParseTime(string setting, string key, out int hour, out int minute)
        {
            var sa = setting.Split(':');
            if (!int.TryParse(sa[0], out hour))
                throw ConfigurationExceptionHelper(key, "Invalid hour.");
            if (hour < 0 || hour > 23)
                throw ConfigurationExceptionHelper(key, "Hour is out of range: 0-23.");
            if (!int.TryParse(sa[1], out minute))
                throw ConfigurationExceptionHelper(key, "Invalid minute.");
            if (minute < 0 || minute > 59)
                throw ConfigurationExceptionHelper(key, "Minute is out of range: 0-59.");
        }
        private static void ParseWeekday(string setting, string key, out DayOfWeek weekday)
        {
            if (!Enum.TryParse(setting, true, out weekday))
                throw ConfigurationExceptionHelper("Weekly", "Invalid weekday. Expected one: Sunday, Monday, Tuesday, Wednesday, Thursday, Friday or Saturday");
        }

        private static void ParseSenderAddress(string setting)
        {
            SenderAddress = string.IsNullOrEmpty(setting) ? DefaultSenderAddress : setting;
        }
        private static void ParseRetryCount(string setting)
        {
            int value;
            if (string.IsNullOrEmpty(setting) || !int.TryParse(setting, out value))
                RetryCount = DefaultRetryCount;
            else
                RetryCount = value;
        }
        private static void ParseRetryDelay(string setting)
        {
            int value;
            if (string.IsNullOrEmpty(setting) || !int.TryParse(setting, out value))
                RetryDelay = DefaultRetryDelay;
            else
                RetryDelay = value;
        }
        private static void ParseTakeCount(string setting)
        {
            int value;
            if (string.IsNullOrEmpty(setting) || !int.TryParse(setting, out value))
                TakeCount = DefaultTakeCount;
            else
                TakeCount = value;
        }
        private static void ParseEncoding(string setting)
        {
            if (string.IsNullOrEmpty(setting))
            {
                MessageEncoding = DefaultMessageEncoding;
                return;
            }

            try
            {
                MessageEncoding = Encoding.GetEncoding(setting);
            }
            catch (ArgumentException)
            {
                SnLog.WriteError(
                    $"The configured value: {setting} is not a valid code page name. Default encoding is set: {DefaultMessageEncoding.BodyName}.");
                MessageEncoding = DefaultMessageEncoding;
            }
        }

        private static Exception ConfigurationExceptionHelper(string key, string msg)
        {
            return new System.Configuration.ConfigurationErrorsException($"{msg} Key: {SECTIONNAME}: @{key}");
        }
    }
}
