using System.Globalization;

namespace PersonalFinanceApp;

public class TransactionDateHelper
{



    public static string FormatGroupKey(string groupKey, string timeUnit)
    {
        if (timeUnit == "Week")
        {
            // Split the groupKey into year and week
            var parts = groupKey.Split('-');
            if (parts.Length == 2 && int.TryParse(parts[1], out int weekNumber))
            {
                return $"{parts[0]} - Week {weekNumber}";
            }
        }
        // For other time units, return the groupKey as is
        return groupKey;
    }

    public static string FormatWeekGroupKey((int Year, int Week) weekInfo)
    {
        return $"{weekInfo.Year:D4} - Week {weekInfo.Week:D2}";
    }







    // Need?
    // Används för att hämta och printa rätt datumformat         
    public static string GetDateKey(Transaction transaction, string timeUnit)
    {
        if (timeUnit == "Day")
        {
            return transaction.Date.ToString("yyyy-MM-dd");
        }
        else if (timeUnit == "Week")
        {
            int weekOfYear = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(transaction.Date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            return $"{transaction.Date.Year} - Week {weekOfYear}";
        }
        else if (timeUnit == "Month")
        {
            return transaction.Date.ToString("yyyy - MMMM");
        }
        else if (timeUnit == "Year")
        {
            return $"{transaction.Date.Year}";
        }

        return null;
    }

    // Remove?
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

    // Remove?
    public static string GetGroupKey(DateTime date, string timeUnit)
    {
        return timeUnit switch
        {
            "Day" => date.ToString("yyyy-MM-dd"),
            "Week" => TransactionDateHelper.FormatWeekGroupKey(GetWeekOfYear(date)),
            "Month" => date.ToString("yyyy MMMM"),
            "Year" => date.ToString("yyyy"),
            _ => throw new ArgumentException("Invalid time unit", nameof(timeUnit)),
        };
    }




}








