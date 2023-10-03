namespace AutoTransfer.Models.Notifications;

public class FailedNotificationModel
{
    public int TargetId { get; set; }
    public string TargetName { get; set; }
    public string UserCount { get; set; }
    public string Usernames { get; set; }
    public int Creator { get; set; }
}
