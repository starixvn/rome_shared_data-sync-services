namespace CashServiceBilling.Extensions;

public static class DateTimeExtensions
{
    [ThreadStatic]
    private static DateTime? _stubDateTime;

    public static DateTime? StubDateTime
    {
        get => _stubDateTime;
        set => _stubDateTime = value;
    }

    public static DateTime Now => StubDateTime ?? DateTime.Now;
}
