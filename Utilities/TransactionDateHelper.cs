using System.Globalization;

namespace PersonalFinanceApp;

/// <summary>
/// Helper class for managing and formatting transaction dates.
/// Provides utilities for grouping transactions by time units like Day, Week, Month, and Year.
/// </summary>
public class TransactionDateHelper
{
    /// <summary>
    /// Generates a grouping key for a given date based on the specified time unit.
    /// </summary>
    public static string GetGroupKey(DateTime date, string timeUnit)
    {
        return timeUnit switch
        {
            "Day" => date.ToString("yyyy-MM-dd"),
            "Week" => FormatWeekGroupKey(GetWeekOfYear(date)),
            "Month" => date.ToString("MMMM yyyy"),
            "Year" => date.ToString("yyyy"),
            _ => throw new ArgumentException("Invalid time unit", nameof(timeUnit)),
        };
    }

    /// <summary>
    /// Formats a grouping key for weeks, combining ISO year and week number.
    /// </summary>
    public static string FormatWeekGroupKey((int Year, int Week) weekInfo)
    {
        return $"{weekInfo.Year:D4} - Week {weekInfo.Week:D2}";
    }

    /// <summary>
    /// Calculates the ISO year and week number for a given date.
    /// Uses the ISO 8601 standard for determining weeks.
    /// </summary>
    private static (int Year, int Week) GetWeekOfYear(DateTime date)
    {
        int isoYear = ISOWeek.GetYear(date);
        int weekNumber = ISOWeek.GetWeekOfYear(date);

        if (isoYear != date.Year)
        {
            Console.WriteLine($"Date: {date}, Calendar Year: {date.Year}, ISO Year: {isoYear}, Week: {weekNumber}");
        }

        return (isoYear, weekNumber);
    }
}
