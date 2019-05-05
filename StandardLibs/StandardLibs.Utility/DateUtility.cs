using System;
using System.Globalization;
using Microsoft.Extensions.Logging;

namespace StandardLibs.Utility
{
    public class DateUtility : IDateUtility
    {
        public static readonly String DU_DATETIME = "yyyyMMddHHmmssfff";
        public static readonly String DU_DATETIME_SHORT = "yyyyMMddHHmmss";
        public static readonly String DU_DATE = "yyyyMMdd";
        private static readonly CultureInfo CI = CultureInfo.InvariantCulture;
        private static readonly DateTime UNIX_START = new DateTime(1970, 1, 1, 0, 0, 0); //DateTimeKind.Local );
        private static readonly String DU_CENTURY_HEAD = "20";

        private ILogger logger { get; set; }
        public string PatternDateTime { set; get; }
        public string PatternDate { set; get; }

        public DateUtility(string patternDateTime = null)
        {
            this.PatternDate = DU_DATE;
            this.PatternDateTime = patternDateTime ?? DU_DATETIME_SHORT;
        }

        public string GetStrByDateTime(DateTime dt)
        {
            return dt.ToString(this.PatternDateTime, CI);
        }

        public string GetStrNow()
        {
            return this.GetStrByDateTime(DateTime.Now);
        }

        public string GetStrByDate(DateTime d)
        {
            return d.ToString(this.PatternDate, CI);
        }

        public string GetStrToday()
        {
            return this.GetStrByDate(DateTime.Now);
        }
    
        public bool TryGetDateByStr(string strDate, out DateTime d)
        {
            return DateTime.TryParseExact(
                strDate, this.PatternDate, CI, DateTimeStyles.None, out d
            );
        }

        public bool TryGetDateTimeByStr(string strDateTime, out DateTime dt)
        {
            return DateTime.TryParseExact(
                strDateTime, this.PatternDateTime, CI, DateTimeStyles.None, out dt
            );
        }

        public bool TryGetDiffDateStr(string strDate, int diff, out string strDiffDate)
        {
            strDiffDate = null;
            if (this.TryGetDateByStr(strDate, out DateTime d))
            {
                d = d.AddDays(diff);
                strDiffDate = this.GetStrByDate(d);
                return true;
            };
            return false;
        }

        public bool TryGetDiffMonthStr(string strDate, int diff, out string strDiffDate)
        {
            strDiffDate = null;
            if (this.TryGetDateByStr(strDate, out DateTime d))
            {
                d = d.AddMonths(diff);
                strDiffDate = this.GetStrByDate(d);
                return true;
            }
            return false;
        }

        public bool TryGetDiffYearStr(string strDate, int diff, out string strDiffDate)
        {
            strDiffDate = null;
            if (this.TryGetDateByStr(strDate, out DateTime d))
            {
                d = d.AddYears(diff);
                strDiffDate = this.GetStrByDate(d);
                return true;
            }
            return false;
        }
              
        public bool ValidDateStr(string strDate)
        {
            return (this.TryGetDateByStr(strDate, out DateTime d));
        }

        public bool ValidDateTimeStr(string strDateTime)
        {
            return (this.TryGetDateTimeByStr(strDateTime, out DateTime dt));
        }

        public void ResetToDefault()
        {
            this.PatternDate = DateUtility.DU_DATE;
            this.PatternDateTime = DateUtility.DU_DATETIME;
        }

        public long GetTotMillisecs(DateTime start, DateTime end)
        {
            return (long)((end - start).TotalMilliseconds);
        }

        public long GetTotMillisecsNow(DateTime start)
        {
            return this.GetTotMillisecs(start, DateTime.Now);
        }

        public bool TimeIsUp(DateTime expired)
        {
            return this.GetTotMillisecsNow(expired) > 0;
        }

        public DateTime GetAddSecondsNow(int secs)
        {
            return DateTime.Now.AddSeconds(secs);
        }

        public string GetStrByUnixTime(byte[] unixTime, string endian = "LE")
        {
            // check if big-endian, reverse it!
            if ("BE".Equals(endian))
            {
                byte tmpByte = 0;
                for (int i = 0; i < unixTime.Length / 2; i++)
                {
                    tmpByte = unixTime[i];
                    unixTime[i] = unixTime[unixTime.Length - 1 - i];
                    unixTime[unixTime.Length - 1 - i] = tmpByte;
                }
            }
            uint seconds = System.BitConverter.ToUInt32(unixTime, 0);
            return this.GetStrByDateTime(UNIX_START.AddSeconds(seconds));
        }

        public byte[] GetUnixTimeByStr(string strDateTime, string endian = "LE")
        {
            if (this.TryGetDateTimeByStr(strDateTime, out DateTime dt))
            {
                uint seconds = (uint)(dt - UNIX_START).TotalSeconds;
                byte[] unixTime = System.BitConverter.GetBytes(seconds);
                if ("BE".Equals(endian))
                {
                    byte tmpByte = 0;
                    for (int i = 0; i < unixTime.Length / 2; i++)
                    {
                        tmpByte = unixTime[i];
                        unixTime[i] = unixTime[unixTime.Length - 1 - i];
                        unixTime[unixTime.Length - 1 - i] = tmpByte;
                    }
                }
                return unixTime;
            }
            return null;
        }

        public string GetJulianToday()
        {
            DateTime dt = DateTime.Now;
            return string.Format("{0:D4}{1:D3}", dt.Year, dt.DayOfYear);
        }

        public string GetJulianNow()
        {
            return this.GetJulianByDateTime(DateTime.Now);
        }

        public string GetJulianByDateTime(DateTime dt)
        {
            //DateTime firstJan = new DateTime(dt.Year, 1, 1);
            //int daysSinceFirstJan = (dt - firstJan).Days + 1;
            string nowStr = this.GetStrByDateTime(dt);
            return string.Format("{0:D4}{1:D3}{2}", dt.Year, dt.DayOfYear, nowStr.Substring(8));
        }

        public bool TryGetDateTimeByJulian(string julian, out DateTime dt)
        {
            try
            {
                DateTime tmpDt;
                DateTime firstJan = new DateTime(Convert.ToInt32(julian.Substring(0, 4), 10), 1, 1);
                tmpDt = firstJan.AddDays(Convert.ToInt32(julian.Substring(4, 3), 10) - 1);
                string dateStr = this.GetStrByDate(tmpDt); // yyyyMMdd                
                return this.TryGetDateTimeByStr(dateStr + julian.Substring(7), out dt);
            }
            catch (Exception ex)
            {
                logger.LogError($"TryGetDateTimeByJulian Exception: [{0}], {1}, {2}", julian, ex.Message, ex.StackTrace);
                dt = default(DateTime);
                return false;
            }
        }

        public string GetDateTimeStrByJulian(string julian)
        {
            try
            {
                DateTime firstJan = new DateTime(Convert.ToInt32(julian.Substring(0, 4), 10), 1, 1);
                DateTime tmpDt = firstJan.AddDays(Convert.ToInt32(julian.Substring(4, 3), 10) - 1);
                string dateStr = this.GetStrByDate(tmpDt); // yyyyMMdd                
                return dateStr + julian.Substring(7); ;
            }
            catch (Exception ex)
            {
                logger.LogError($"TryGetDateTimeStrByJulian Exception: [{0}], {1}, {2}", julian, ex.Message, ex.StackTrace);
                return default(string);
            }
        }

        public string GetExpireDateStrByShort(string yyMM)
        {
            string bom = DU_CENTURY_HEAD + yyMM + "01";   // first day of the month
            if (this.TryGetDiffMonthStr(bom, 1, out string nextDate))
            {
                if (this.TryGetDiffDateStr(nextDate, -1, out string expireDate))
                {
                    return expireDate;
                }
            }
            return DU_CENTURY_HEAD + yyMM + "31";  // the most date of the month
        }
    }
}
