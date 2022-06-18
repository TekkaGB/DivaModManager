using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DivaModManager
{
    public static class StringConverters
    {
        public static string FormatFileName(string filename)
        {
            return Path.GetFileName(filename);
        }
        // Load all suffixes in an array  
        static readonly string[] suffixes =
        { " Bytes", " KB", " MB", " GB", " TB", " PB" };
        public static string FormatSize(long bytes)
        {
            int counter = 0;
            decimal number = (decimal)bytes;
            while (Math.Round(number / 1000) >= 1)
            {
                number = number / 1000;
                counter++;
            }
            return bytes != 0 ? string.Format("{0:n1}{1}", number, suffixes[counter])
                : string.Format("{0:n0}{1}", number, suffixes[counter]);
        }
        public static string FormatNumber(int number)
        {
            if (number > 1000000)
                return Math.Round((double)number / 1000000, 1).ToString() + "M";
            else if (number > 1000)
                return Math.Round((double)number / 1000, 1).ToString() + "K";
            else
                return number.ToString();
        }
        public static string FormatTimeSpan(TimeSpan timeSpan)
        {
            if (timeSpan.TotalMinutes < 60)
            {
                return Math.Floor(timeSpan.TotalMinutes).ToString() + "min";
            }
            else if (timeSpan.TotalHours < 24)
            {
                return Math.Floor(timeSpan.TotalHours).ToString() + "hr";
            }
            else if (timeSpan.TotalDays < 7)
            {
                return Math.Floor(timeSpan.TotalDays).ToString() + "d";
            }
            else if (timeSpan.TotalDays < 30.4)
            {
                return Math.Floor(timeSpan.TotalDays / 7).ToString() + "wk";
            }
            else if (timeSpan.TotalDays < 365.25)
            {
                return Math.Floor(timeSpan.TotalDays / 30.4).ToString() + "mo";
            }
            else
            {
                return Math.Floor(timeSpan.TotalDays % 365.25).ToString() + "yr";
            }
        }
        public static string FormatTimeAgo(TimeSpan timeSpan)
        {
            if (timeSpan.TotalMinutes < 60)
            {
                var minutes = Math.Floor(timeSpan.TotalMinutes);
                return minutes > 1 ? $"{minutes} minutes ago" : $"{minutes} minute ago";
            }
            else if (timeSpan.TotalHours < 24)
            {
                var hours = Math.Floor(timeSpan.TotalHours);
                return hours > 1 ? $"{hours} hours ago" : $"{hours} hour ago";
            }
            else if (timeSpan.TotalDays < 7)
            {
                var days = Math.Floor(timeSpan.TotalDays);
                return days > 1 ? $"{days} days ago" : $"{days} day ago";
            }
            else if (timeSpan.TotalDays < 30.4)
            {
                var weeks = Math.Floor(timeSpan.TotalDays / 7);
                return weeks > 1 ? $"{weeks} weeks ago" : $"{weeks} week ago";
            }
            else if (timeSpan.TotalDays < 365.25)
            {
                var months = Math.Floor(timeSpan.TotalDays / 30.4);
                return months > 1 ? $"{months} months ago" : $"{months} month ago";
            }
            else
            {
                var years = Math.Floor(timeSpan.TotalDays / 365.25);
                return years > 1 ? $"{years} years ago" : $"{years} year ago";
            }
        }
        public static string FormatSingular(string rootCat, string cat)
        {
            if (rootCat == null)
                return cat.TrimEnd('s');
            rootCat = rootCat.Replace("User Interface", "UI");

            if (cat == "Skin Packs")
                return cat.Substring(0, cat.Length - 1);

            if (rootCat[rootCat.Length - 1] == 's')
            {
                if (cat == rootCat)
                {
                    rootCat = rootCat.Replace("xes", "xs").Replace("xs/", "xes/");
                    return rootCat.Substring(0, rootCat.Length - 1);
                }
                else
                    return $"{cat} {rootCat.Substring(0, rootCat.Length - 1)}";
            }
            else
            {
                if (cat == rootCat)
                    return rootCat;
                else
                    return $"{cat} {rootCat}";
            }
        }
    }
}
