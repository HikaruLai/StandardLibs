using System;
using System.Collections.Generic;
using System.Text;

namespace StandardLibs.Utility
{
    /// <summary>
    /// Interface for Date Utility to transfer between DateTime and string
    /// </summary>
    public interface IDateUtility
    {
        /// <summary>
        /// set/get DateTime Pattern,ex."yyyyMMddHHmmssfff"
        /// ref:
        ///   Standard DateTime Format Strings
        ///   http://msdn.microsoft.com/en-us/library/az4se3k1%28v=VS.100%29.aspx#Y2352
        /// </summary>
        string PatternDateTime { set; get; }

        /// <summary>
        /// set/get Date Pattern,ex."yyyyMMdd"
        /// ref:
        ///   Standard DateTime Format Strings
        ///   http://msdn.microsoft.com/en-us/library/az4se3k1%28v=VS.100%29.aspx#Y2352
        /// </summary>
        string PatternDate { set; get; }

        /// <summary>
        ///  Transfer DateTime to string by PatternDateTime 
        /// </summary>
        /// <param name="dateTime">DateTime instance to be transferred</param>
        /// <returns>datetime string</returns> 
        string GetStrByDateTime(DateTime dateTime);

        /// <summary>
        ///   Transfer DateTime.Now to string by PatternDateTime 
        /// </summary>
        /// <returns>datetime string</returns>  
        string GetStrNow();

        /// <summary>
        ///  Transfer DateTime to string by PatternDate
        /// </summary>
        /// <param name="date">DateTime instance to be transferred</param>
        /// <returns>date string</returns>  
        string GetStrByDate(DateTime date);

        /// <summary>
        ///   Transfer DateTime.Now to string by PatternDate
        /// </summary>
        /// <returns>date string</returns>  
        string GetStrToday();

        /// <summary>
        ///   Try to transfer date string to DateTime instance by PatternDate
        /// </summary>
        /// <param name="strDate">Date string to be transferred. Must check first!</param>
        /// <param name="date">out DateTime instance, if strDate check OK!</param>
        /// <returns>bool,strDate check result</returns>  
        bool TryGetDateByStr(string strDate, out DateTime date);

        /// <summary>
        ///   Try to transfer datetime string to DateTime instance by PatternDateTime
        /// </summary>
        /// <param name="strDateTime">DateTime string to be transferred. Must check first!</param>
        /// <param name="dateTime">out DateTime instance, if strDateTime check OK!</param>
        /// <returns>bool,strDateTime check result</returns>
        bool TryGetDateTimeByStr(string strDateTime, out DateTime dateTime);

        /// <summary>
        ///    Get Date string from strDate and diff days, by DateTime calculation 
        /// </summary>
        /// <param name="strDate">date string to be calculated,must check first</param>
        /// <param name="diff">diff days</param>
        /// <param name="strDiffDate">out string, result of DateTime calculation</param>
        /// <returns>strDate check result</returns>
        bool TryGetDiffDateStr(string strDate, int diff, out string strDiffDate);

        /// <summary>
        ///    Get Date string from strDate and diff Months, by DateTime calculation
        /// </summary>
        /// <param name="strDate">date string to be calculated,must check first</param>
        /// <param name="diff">diff Months</param>
        /// <param name="strDiffDate">out string, result of DateTime calculation</param>
        /// <returns>strDate check result</returns>
        bool TryGetDiffMonthStr(string strDate, int diff, out string strDiffDate);

        /// <summary>
        ///    Get Date string from strDate and diff Months, by DateTime calculation
        /// </summary>
        /// <param name="strDate">date string to be calculated,must check first</param>
        /// <param name="diff">diff years</param>
        /// <param name="strDiffDate">out string, result of DateTime calculation</param>
        /// <returns>strDate check result</returns>
        bool TryGetDiffYearStr(string strDate, int diff, out string strDiffDate);

        /// <summary>
        ///    Check the datetime string is valid or not!
        /// </summary>
        /// <param name="strDateTime">datetime string to be checked</param>
        /// <returns>check result</returns>
        bool ValidDateTimeStr(string strDateTime);

        /// <summary>
        ///     Check the date string is valid or not!
        /// </summary>
        /// <param name="strDate">date string to be checked</param>
        /// <returns>check result</returns>
        bool ValidDateStr(string strDate);

        /// <summary>
        ///   Reset date/datetime pattern to default config
        /// </summary>
        void ResetToDefault();

        /// <summary>
        /// Get total milliseconds from start till end
        /// </summary>        
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns>total msecs</returns>
        long GetTotMillisecs(DateTime start, DateTime end);

        /// <summary>
        /// Get total milliseconds from start till now
        /// </summary>
        /// <param name="start"></param>
        /// <returns>total msecs</returns>
        long GetTotMillisecsNow(DateTime start);

        /// <summary>
        /// Time is up according to expired datetime.
        /// </summary>
        /// <param name="expired">expired datetime</param>
        /// <returns>true, time is up!</returns>
        bool TimeIsUp(DateTime expired);

        /// <summary>
        /// Get DateTime from now add specific seconds
        /// </summary>
        /// <param name="secs">seconds to be added</param>
        /// <returns>DateTime from now with specific seconds</returns>
        DateTime GetAddSecondsNow(int secs);

        /// <summary>
        /// Get DateTime format string from UnixTime of 4 bytes( big-endian )
        /// </summary>
        /// <param name="unixTime"></param>
        /// <returns></returns>
        string GetStrByUnixTime(byte[] unixTime, string endian = "LE");

        /// <summary>
        /// Get UnitTime from Date
        /// </summary>
        /// <param name="strDateTime"></param>
        /// <returns></returns>
        byte[] GetUnixTimeByStr(string strDateTime, string endian = "LE");

        /// <summary>
        /// Get Julian string today, yyyyJJJ
        /// </summary>
        /// <returns></returns>
        string GetJulianToday();

        /// <summary>
        /// Get Julian string now, yyyyJJJHHmmss
        /// </summary>
        /// <returns></returns>
        string GetJulianNow();

        /// <summary>
        /// Get Julian string by datetime
        /// </summary>
        /// <param name="dt">datetime</param>
        /// <returns>yyyyJJJHHmmss</returns>
        string GetJulianByDateTime(DateTime dt);

        /// <summary>
        /// Get Datetime by Julian string
        /// </summary>
        /// <param name="julian"></param>
        /// <param name="dt">out datetime</param>
        /// <returns>true: get ok</returns>
        bool TryGetDateTimeByJulian(string julian, out DateTime dt);

        /// <summary>
        /// transfer Julian string( ex.yyyyJJJHHmmssfff) to DateTime string( ex.yyyyMMddHHmmssfff )
        /// </summary>
        /// <param name="julian">julian string</param>
        /// <returns>datetime string</returns>
        string GetDateTimeStrByJulian(string julian);

        string GetExpireDateStrByShort(string yyMM);
    }
}
