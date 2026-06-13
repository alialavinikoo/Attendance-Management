using System;
using System.Globalization;

namespace AttendanceManagement.Utilities
{
    public static class IranTimeHelper
    {
        private static readonly TimeZoneInfo IranTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time");

        private static readonly PersianCalendar PersianCal = new PersianCalendar();

        
        public static DateTime GetCurrentIranTime()
        {
            DateTime utcNow = DateTime.UtcNow;
            return TimeZoneInfo.ConvertTimeFromUtc(utcNow, IranTimeZone);
        }

        public static string ToPersianDateString(DateTime gregorianDate)
        {
            int year = PersianCal.GetYear(gregorianDate);
            int month = PersianCal.GetMonth(gregorianDate);
            int day = PersianCal.GetDayOfMonth(gregorianDate);

            return $"{year:0000}/{month:00}/{day:00}";
        }

        public static DateTime PersianToGregorian(int persianYear, int persianMonth, int persianDay)
        {
            return PersianCal.ToDateTime(persianYear, persianMonth, persianDay, 0, 0, 0, 0);
        }

        public static int GetDaysInMonth(int year, int month) 
        {
            return (month <= 6) ? 31 : (month < 12) ? 30 : (IranTimeHelper.IsLeapYear(year) ? 30 : 29);
        }

        public static int GetCurrentPersianYear()
        {
            return PersianCal.GetYear(DateTime.Today);
        }

        public static bool IsLeapYear(int persianYear)
        {
            return PersianCal.IsLeapYear(persianYear);
        }
    }
}