
namespace AutoTransfer.Constants;

public static class LanguageCodeConstants
{
    public const string English = "en-US";
    public const string ChineseTraditional = "zh-Hant"; // zh-Hant-TW
    public const string ChineseSimplified = "zh-Hans"; // zh-Hans-CN
    public const string Thai = "th-TH";
    public const string Vietnamese = "vi-VN";
    public const string Japanese = "ja-JP";
    public const string Korean = "ko-KR";

    public static readonly IDictionary<string, string> LanguageMapper = new Dictionary<string, string>
    {
        [ChineseTraditional] = "zh-TW",
        [ChineseSimplified] = "zh-CN"
    };
}
