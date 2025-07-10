namespace SplitWiseService.Helpers;

public static class DateHelper
{
    public static DateTime FirstDayOfMonth(DateTime date)
    {
        return new DateTime(date.Year, date.Month, 1);
    }

    public static DateTime LastDayOfMonth(DateTime date)
    {
        return new DateTime(date.Year, date.Month, 1).AddMonths(1).AddDays(-1);
    }
}
