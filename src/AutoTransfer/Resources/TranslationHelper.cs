using System.Globalization;
using AutoTransfer.Constants;

namespace AutoTransfer.Resources;

public static class TranslationHelper
{
    public static string Get(string key, string language)
    {
        var value = Translations.ResourceManager.GetString(key, CultureInfo.CreateSpecificCulture(language)) ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(value) || language == LanguageCodeConstants.English)
        {
            return value;
        }

        return Get(key, LanguageCodeConstants.English);
    }
}