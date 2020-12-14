using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CoreLibrary.Helper
{
    public static class DataLayerHelper
    {
        static public string GenDeleteCode()
        {
            return String.Format("({0}{1}{2})", DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
        }
    }
    public static class JsonHelper
    {
        static public ReturnClass JsonToObject<ReturnClass>(string jsonText) where ReturnClass : new()
        {
            try
            {
                return JsonConvert.DeserializeObject<ReturnClass>(jsonText);
            }
            catch
            {
                return new ReturnClass();
            }
        }
        static public string ObjectToJson<ReturnClass>(ReturnClass obj)
        {
            try
            {
                return JsonConvert.SerializeObject(obj);
            }
            catch
            {
                return null;
            }
        }
    }
    public static class DateTimeHelper
    {
        static public int GetCurrentYear()
        {
            return DateTime.Now.Year;
        }
        static public int GetCurrentMonth()
        {
            return DateTime.Now.Month;
        }
        static public int GetCurrentPeriod()
        {
            int currentMonth = DateTime.Now.Month;
            return GetPeriod(currentMonth);
        }
        static public int GetPeriod(int month)
        {
            return ((month - 1) / 3) + 1;
        }
        static public DateTime GetFirstDateOfMonth(int year, int month)
        {
            return new DateTime(year, month, 1);
        }
        static public DateTime GetLastDateOfMonth(int year, int month)
        {
            int nextYear = year;
            int nextMonth = month + 1;
            if (nextMonth > 12)
            {
                nextMonth = 1;
                nextYear++;
            }
            return new DateTime(nextYear, nextMonth, 1).AddSeconds(-1);
        }
        static public DateTime GetStartOfDate(DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
        }
        static public DateTime GetEndOfDate(DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, 23, 59, 59);
        }
    }
    public static class StringHelper
    {
        static public string HashTag(string text)
        {
            if (text == null)
                return "";
            else
                return "#" + text.Trim().ToLower() + "#";
        }
        static public string ToLower(string text)
        {
            if (text == null)
                return "";
            else
                return text.Trim().ToLower();
        }
    }
}
