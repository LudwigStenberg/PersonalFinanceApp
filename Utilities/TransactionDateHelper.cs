using System.Globalization;

namespace PersonalFinanceApp;

public class TransactionDateHelper
{

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

    public static string FormatWeekGroupKey((int Year, int Week) weekInfo)
    {
        return $"{weekInfo.Year:D4} - Week {weekInfo.Week:D2}";
    }

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








