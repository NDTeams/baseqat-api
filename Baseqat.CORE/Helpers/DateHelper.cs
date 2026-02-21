using System;
using System.Globalization;

namespace Baseqat.CORE.Helpers
{
    public static class DateHelper
    {
        private static readonly UmAlQuraCalendar _hijriCalendar = new UmAlQuraCalendar();
        private static readonly string[] _arabicMonths = new[]
        {
            "محرم", "صفر", "ربيع الأول", "ربيع الثاني",
            "جمادى الأولى", "جمادى الآخرة", "رجب", "شعبان",
            "رمضان", "شوال", "ذو القعدة", "ذو الحجة"
        };

        private static readonly string[] _arabicDays = new[]
        {
            "الأحد", "الاثنين", "الثلاثاء", "الأربعاء",
            "الخميس", "الجمعة", "السبت"
        };

        /// <summary>
        /// تحويل التاريخ الميلادي إلى هجري
        /// </summary>
        /// <param name="gregorianDate">التاريخ الميلادي</param>
        /// <returns>التاريخ الهجري كنص</returns>
        public static string ToHijri(DateTime gregorianDate)
        {
            try
            {
                int hijriYear = _hijriCalendar.GetYear(gregorianDate);
                int hijriMonth = _hijriCalendar.GetMonth(gregorianDate);
                int hijriDay = _hijriCalendar.GetDayOfMonth(gregorianDate);

                return $"{hijriDay}/{hijriMonth}/{hijriYear}";
            }
            catch
            {
                return gregorianDate.ToString("yyyy/MM/dd");
            }
        }

        /// <summary>
        /// تحويل التاريخ الميلادي إلى هجري مع الوقت
        /// </summary>
        public static string ToHijriWithTime(DateTime gregorianDate)
        {
            try
            {
                int hijriYear = _hijriCalendar.GetYear(gregorianDate);
                int hijriMonth = _hijriCalendar.GetMonth(gregorianDate);
                int hijriDay = _hijriCalendar.GetDayOfMonth(gregorianDate);
                string time = gregorianDate.ToString("hh:mm tt");

                return $"{hijriDay}/{hijriMonth}/{hijriYear} {time}";
            }
            catch
            {
                return gregorianDate.ToString("yyyy/MM/dd hh:mm tt");
            }
        }

        /// <summary>
        /// تحويل التاريخ الميلادي إلى هجري مع اسم الشهر بالعربي
        /// </summary>
        public static string ToHijriArabicMonth(DateTime gregorianDate)
        {
            try
            {
                int hijriYear = _hijriCalendar.GetYear(gregorianDate);
                int hijriMonth = _hijriCalendar.GetMonth(gregorianDate);
                int hijriDay = _hijriCalendar.GetDayOfMonth(gregorianDate);
                string monthName = _arabicMonths[hijriMonth - 1];

                return $"{hijriDay} {monthName} {hijriYear}هـ";
            }
            catch
            {
                return gregorianDate.ToString("yyyy/MM/dd");
            }
        }

        /// <summary>
        /// تحويل التاريخ الميلادي إلى هجري مع اليوم والشهر بالعربي
        /// </summary>
        public static string ToHijriFullArabic(DateTime gregorianDate)
        {
            try
            {
                int hijriYear = _hijriCalendar.GetYear(gregorianDate);
                int hijriMonth = _hijriCalendar.GetMonth(gregorianDate);
                int hijriDay = _hijriCalendar.GetDayOfMonth(gregorianDate);
                string monthName = _arabicMonths[hijriMonth - 1];
                string dayName = _arabicDays[(int)gregorianDate.DayOfWeek];

                return $"{dayName}، {hijriDay} {monthName} {hijriYear}هـ";
            }
            catch
            {
                return gregorianDate.ToString("yyyy/MM/dd");
            }
        }

        /// <summary>
        /// تحويل التاريخ الميلادي إلى هجري (nullable)
        /// </summary>
        public static string? ToHijri(DateTime? gregorianDate)
        {
            if (!gregorianDate.HasValue)
                return null;

            return ToHijri(gregorianDate.Value);
        }

        /// <summary>
        /// تحويل التاريخ الميلادي إلى هجري مع الوقت (nullable)
        /// </summary>
        public static string? ToHijriWithTime(DateTime? gregorianDate)
        {
            if (!gregorianDate.HasValue)
                return null;

            return ToHijriWithTime(gregorianDate.Value);
        }

        /// <summary>
        /// تحويل التاريخ الميلادي إلى هجري مع اسم الشهر بالعربي (nullable)
        /// </summary>
        public static string? ToHijriArabicMonth(DateTime? gregorianDate)
        {
            if (!gregorianDate.HasValue)
                return null;

            return ToHijriArabicMonth(gregorianDate.Value);
        }

        /// <summary>
        /// الحصول على التاريخ الهجري الحالي
        /// </summary>
        public static string GetCurrentHijriDate()
        {
            return ToHijri(DateTime.Now);
        }

        /// <summary>
        /// الحصول على التاريخ الهجري الحالي مع الوقت
        /// </summary>
        public static string GetCurrentHijriDateWithTime()
        {
            return ToHijriWithTime(DateTime.Now);
        }
    }
}
