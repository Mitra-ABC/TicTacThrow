/// <summary>
/// Client-only fallbacks. Server text is shown as-is — do not map backend English here.
/// </summary>
public static class UserError
{
    public const string Generic = "مشکلی پیش آمد. دوباره تلاش کنید.";
    public const string ConnectionLost = "اتصال قطع شد. دوباره تلاش کنید.";
    public const string ConnectionFailed = "اتصال به سرور برقرار نشد.";
    public const string NotEnoughHearts = "قلب کافی ندارید.";

    public static string ToUserText(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return Generic;

        var trimmed = raw.Trim();
        if (ContainsPersian(trimmed))
            return trimmed;

        if (LooksTechnical(trimmed))
            return Generic;

        var key = trimmed.ToLowerInvariant().Replace('_', ' ').TrimEnd('.', '!', ' ');
        if (key == "connection failed" || key == "connection lost")
            return key.Contains("lost") ? ConnectionLost : ConnectionFailed;

        return Generic;
    }

    public static bool IsNotEnoughHearts(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return false;
        var key = raw.Trim().ToLowerInvariant();
        return key.Contains("not_enough_hearts")
            || key.Contains("not enough hearts")
            || key.Contains("قلب کافی");
    }

    private static bool LooksTechnical(string text)
    {
        var lower = text.ToLowerInvariant();
        return lower.Contains("json parse")
            || lower.Contains("ssl")
            || lower.Contains("certificate")
            || lower.Contains("http/")
            || lower.Contains("exception")
            || lower.Contains("stacktrace")
            || lower.StartsWith("40")
            || lower.StartsWith("50");
    }

    private static bool ContainsPersian(string text)
    {
        foreach (var c in text)
        {
            if (c >= 0x0600 && c <= 0x06FF)
                return true;
        }
        return false;
    }
}
