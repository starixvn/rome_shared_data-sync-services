using System.Text.Json.Serialization;

namespace CashServiceBilling.Models;

public class LogEntry
{
    [JsonPropertyName("UserID")]
    public int UserId { get; init; }

    [JsonPropertyName("CustID")]
    public int CustId { get; init; }

    [JsonPropertyName("ActionID")]
    public int ActionId { get; init; }

    public string TrackingValueMask { get; init; }

    public string TrackingValueMask2 { get; init; }

    public string TrackingValue { get; init; }

    public string TrackingValue2 { get; init; }

    public short CustDBGroupId { get; init; }
}
