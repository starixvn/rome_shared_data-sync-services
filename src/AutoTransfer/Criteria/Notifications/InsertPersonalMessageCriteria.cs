using AutoTransfer.Constants;
using AutoTransfer.Models.Notifications;
using AutoTransfer.Resources;
using Fanex.Data;
using Fanex.Data.Repository;

namespace AutoTransfer.Criteria.Notifications;

public class InsertPersonalMessageCriteria : CriteriaBase
{
    public InsertPersonalMessageCriteria(FailedNotificationModel notification)
    {
        ContentEN = GetMessage(LanguageCodeConstants.English, notification);
        ContentCH = GetMessage(LanguageCodeConstants.ChineseTraditional, notification);
        ContentCS = GetMessage(LanguageCodeConstants.ChineseSimplified, notification);
        ContentJP = GetMessage(LanguageCodeConstants.Japanese, notification);
        ContentTH = GetMessage(LanguageCodeConstants.Thai, notification);
        ContentKO = GetMessage(LanguageCodeConstants.Korean, notification);
        ContentVN = GetMessage(LanguageCodeConstants.Vietnamese, notification);
        MessageType = MessageTypeConstants.Personal;
        CreatorName = string.Empty;
        IsBatch = true;
    }

    [SpParam(SqlParamName = "content_en")]
    public string ContentEN { get; set; }

    [SpParam(SqlParamName = "content_ch")]
    public string ContentCH { get; set; }

    [SpParam(SqlParamName = "content_cs")]
    public string ContentCS { get; set; }

    [SpParam(SqlParamName = "content_jp")]
    public string ContentJP { get; set; }

    [SpParam(SqlParamName = "content_it")]
    public string ContentIT => string.Empty;

    [SpParam(SqlParamName = "content_th")]
    public string ContentTH { get; set; }

    [SpParam(SqlParamName = "content_ko")]
    public string ContentKO { get; set; }

    [SpParam(SqlParamName = "content_es")]
    public string ContentES => string.Empty;

    [SpParam(SqlParamName = "content_vn")]
    public string ContentVN { get; set; }

    [SpParam(SqlParamName = "msgtype")]
    public int MessageType { get; set; }

    public string CreatorName { get; }

    public bool IsBatch { get; }

    public override string GetSettingKey() => "Age_Msg_SetMessage";

    public override bool IsValid() => true;

    private string GetMessage(string langue, FailedNotificationModel notification)
    {
        var text = TranslationHelper.Get("FailedNotification", langue);

        return string.Format(text, notification.UserCount, notification.Usernames);
    }
}
