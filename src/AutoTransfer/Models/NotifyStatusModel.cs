namespace AutoTransfer.Models;

public class NotifyStatusModel
{
    public const int NotYetSendNotification = 0;

    public DateTime StartTime { get; set; }

    public int StartTime_Notify { get; set; }
}
