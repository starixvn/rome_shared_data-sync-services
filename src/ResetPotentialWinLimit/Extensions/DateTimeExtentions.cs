namespace ResetPotentialWinLimit.Extensions;

public static class DateTimeExtentions
{
    [ThreadStatic]
    private static DateTime? _stubDateTime;

    public static DateTime? StubDateTime
    {
        get => _stubDateTime;
        set => _stubDateTime = value;
    }

    public static DateTime Now => StubDateTime ?? DateTime.Now;

    public static bool IsSameDate(this DateTime dateTime, DateTime otherDateTime)
    {
        return dateTime.Year == otherDateTime.Year && dateTime.Month == otherDateTime.Month && dateTime.Day == otherDateTime.Day;
    }
}
