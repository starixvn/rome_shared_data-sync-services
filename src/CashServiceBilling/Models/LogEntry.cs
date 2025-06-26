namespace CashServiceBilling.Models;

public class LogEntry
{
    public int UserId { get; init; }
    public int CustId { get; init; }
    public int ActionId { get; init; }
    public string TrackingValueMask { get; init; }
    public string TrackingValueMask2 { get; init; }
    public string TrackingValue { get; init; }
    public string TrackingValue2 { get; init; }
    public short CustDBGroupId { get; init; }
}
