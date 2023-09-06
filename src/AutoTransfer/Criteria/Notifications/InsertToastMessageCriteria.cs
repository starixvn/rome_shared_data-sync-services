using AutoTransfer.Constants;
using AutoTransfer.Models.Notifications;
using AutoTransfer.Resources;
using Newtonsoft.Json;
using Fanex.Data;
using Fanex.Data.Repository;

namespace AutoTransfer.Criteria.Notifications;

public class InsertToastMessageCriteria : CriteriaBase
{
    public InsertToastMessageCriteria(FailedNotificationModel notification)
    {
        TypeId = ToastMessageTypeConstants.Error;
        SiteFunctionId = ToastMessageFunctionConstants.AutoTransfer;
        TargetId = notification.TargetId;
        Creator = notification.Creator;

        ContentEN = GetMessage(LanguageCodeConstants.English, notification);
        ContentCH = GetMessage(LanguageCodeConstants.ChineseTraditional, notification);
        ContentCS = GetMessage(LanguageCodeConstants.ChineseSimplified, notification);
        ContentJP = GetMessage(LanguageCodeConstants.Japanese, notification);
        ContentTH = GetMessage(LanguageCodeConstants.Thai, notification);
        ContentKO = GetMessage(LanguageCodeConstants.Korean, notification);
        ContentVN = GetMessage(LanguageCodeConstants.Vietnamese, notification);
    }

    public int TypeId { get; set; }

    public int SiteFunctionId { get; set; }

    public int TargetId { get; set; }

    public int Creator { get; set; }

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

    public int ErrCode { get; set; }

    public string ErrMsg { get; set; }

    public int ToastId { get; set; }

    public override string GetSettingKey() => "Age_ToastMessage_Ins";

    public override bool IsValid() => true;

    private string GetMessage(string langue, FailedNotificationModel notification)
    {
        var content = new ToastContentModel
        {
            Title = TranslationHelper.Get("ToastFailedNotificationTitle", langue),
            Body = string.Format(TranslationHelper.Get("ToastFailedNotification", langue), notification.UserCount, "{0}")
        };

        return JsonConvert.SerializeObject(content, Formatting.None);
    }
}
