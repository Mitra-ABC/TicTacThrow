using System.Collections.Generic;
using System.Text;
using RTLTMPro;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Lalezar + RTLTMPro shaping so Persian letters join and read right-to-left.
/// Does not move or resize UI — only font and glyph order.
/// </summary>
public static class PersianUi
{
    private static TMP_FontAsset cachedFont;
    private static readonly FastStringBuilder ShapeBuffer = new FastStringBuilder(RTLSupport.DefaultBufferSize);

    private static readonly Dictionary<string, string> Labels = new Dictionary<string, string>
    {
        { "user name", GameStrings.UsernamePlaceholder },
        { "username", GameStrings.UsernamePlaceholder },
        { "password", GameStrings.PasswordPlaceholder },
        { "nickname", GameStrings.NicknamePlaceholder },
        { "login", GameStrings.LoginButton },
        { "register", GameStrings.RegisterButton },
        { "logout", GameStrings.LogoutButton },
        { "welcome", GameStrings.WelcomeGuest },
        { "welcome to dodoooz", GameStrings.WelcomeTitle },
        { "play online", GameStrings.PlayOnlineButton },
        { "friendly match", GameStrings.FriendlyMatchButton },
        { "with friends", GameStrings.WithFriends },
        { "leader board", GameStrings.LeaderboardTitle },
        { "leaderboard", GameStrings.LeaderboardTitle },
        { "my stats", GameStrings.MyStatsTitle },
        { "store", GameStrings.StoreTitle },
        { "boosters", GameStrings.BoostersTitle },
        { "lobby", GameStrings.LobbyTitle },
        { "close", GameStrings.CloseButton },
        { "cancel", GameStrings.CancelButton },
        { "back", GameStrings.BackButton },
        { "back to lobby", GameStrings.BackToLobbyButton },
        { "play again", GameStrings.PlayAgainButton },
        { "create room", GameStrings.CreateRoomButton },
        { "join room", GameStrings.JoinRoomButton },
        { "join", GameStrings.JoinButton },
        { "enter room id", GameStrings.RoomIdPlaceholder },
        { "share this room id", "این شناسه را برای دوستت بفرست" },
        { "waiting for opponent", GameStrings.WaitingForOpponent },
        { "looking for opponent", GameStrings.SearchingForOpponent },
        { "no heart", GameStrings.NoHeartsTitle },
        { "buy heart", GameStrings.BuyHeartButton },
        { "remaining time", "زمان باقی‌مانده" },
        { "time remaining", "زمان باقی‌مانده" },
        { "loading", GameStrings.Loading },
        { "buy", GameStrings.BuyButton },
        { "coins 0", string.Format(GameStrings.CoinsFormat, 0) },
        { "hearts 0/5", string.Format(GameStrings.HeartsFormat, 0, 5) },
        { "season -", string.Format(GameStrings.SeasonFormat, "-") },
        { "rank -", string.Format(GameStrings.RankFormat, "-") },
        { "rating -", string.Format(GameStrings.RatingFormat, "-") },
        { "wins 0", string.Format(GameStrings.WinsFormat, 0) },
        { "losses 0", string.Format(GameStrings.LossesFormat, 0) },
        { "draws 0", string.Format(GameStrings.DrawsFormat, 0) },
        { "games played 0", string.Format(GameStrings.GamesPlayedFormat, 0) },
        { "duration 60 minutes", string.Format(GameStrings.BoosterDurationFormat, 60) },
        { "price 50 coins", string.Format(GameStrings.BoosterPriceFormat, 50) },
        { "+25 bonus", string.Format(GameStrings.CoinBonusFormat, 25) },
        { "100 coins", "۱۰۰ سکه" },
        { "small pack", "بسته کوچک" },
        { "double reward", "پاداش دوبل" },
        { "double coins on match win", "سکه برد دوبل می‌شود" },
        { "enter text", "متن را وارد کنید" },
        { "new text", "" },
        { "playername", GameStrings.UnknownNickname },
    };

    public static void Apply()
    {
        var font = ResolveFont();

        foreach (var tmp in Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (tmp == null)
                continue;
            if (tmp.GetComponentInParent<BoardCell>(true) != null)
                continue;
            if (IsTypingField(tmp))
            {
                Style(tmp, font);
                continue;
            }

            Style(tmp, font);
            var logical = ReadLogical(tmp);
            SetText(tmp, Translate(logical) ?? logical);
        }

        foreach (var text in Object.FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (text == null || string.IsNullOrWhiteSpace(text.text))
                continue;
            UpgradeLegacyText(text, font);
        }
    }

    public static string ReadLogical(TMP_Text tmp)
    {
        if (tmp is RTLTextMeshPro rtl && !string.IsNullOrEmpty(rtl.OriginalText))
            return rtl.OriginalText;
        return tmp != null ? tmp.text : string.Empty;
    }

    public static void SetText(TMP_Text tmp, string value)
    {
        if (tmp == null)
            return;
        Style(tmp);
        var logical = value ?? string.Empty;
        if (tmp is RTLTextMeshPro rtl)
        {
            rtl.Farsi = true;
            rtl.FixTags = true;
            rtl.text = logical;
        }
        else
        {
            tmp.text = string.IsNullOrEmpty(logical) ? string.Empty : Shape(logical);
        }
    }

    public static string Shape(string value)
    {
        if (string.IsNullOrEmpty(value) || !ContainsRtl(value) || LooksAlreadyShaped(value))
            return value;

        ShapeBuffer.Clear();
        RTLSupport.FixRTL(value, ShapeBuffer, farsi: true, fixTextTags: true, preserveNumbers: false);
        return ShapeBuffer.ToString();
    }

    public static void Style(TMP_Text tmp, bool rtl = false)
    {
        if (tmp == null)
            return;
        Style(tmp, ResolveFont());
    }

    private static void Style(TMP_Text tmp, TMP_FontAsset font)
    {
        if (tmp == null)
            return;
        if (font != null)
            tmp.font = font;
        if (tmp is not RTLTextMeshPro)
            tmp.isRightToLeftText = false;
    }

    private static void UpgradeLegacyText(Text text, TMP_FontAsset font)
    {
        if (text == null)
            return;

        var translated = Translate(text.text) ?? text.text;
        if (string.IsNullOrWhiteSpace(translated))
            return;

        var parent = text.transform as RectTransform;
        if (parent == null)
        {
            text.text = Shape(translated);
            return;
        }

        const string childName = "PersianLabel";
        var child = parent.Find(childName);
        TextMeshProUGUI tmp = child != null ? child.GetComponent<TextMeshProUGUI>() : null;
        if (tmp == null)
        {
            var go = child != null
                ? child.gameObject
                : new GameObject(childName, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            tmp = go.GetComponent<TextMeshProUGUI>();
            if (tmp == null)
                tmp = go.AddComponent<RTLTextMeshPro>();
        }

        if (tmp == null)
        {
            text.text = Shape(translated);
            return;
        }

        text.enabled = false;
        tmp.color = text.color;
        tmp.fontSize = Mathf.Max(18f, text.fontSize);
        tmp.alignment = TextAlignmentOptions.Midline;
        tmp.raycastTarget = false;
        SetText(tmp, translated);
    }

    public static string Translate(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;
        var key = Normalize(raw);
        if (string.IsNullOrEmpty(key))
            return null;
        return Labels.TryGetValue(key, out var fa) ? fa : null;
    }

    private static bool IsTypingField(TMP_Text tmp)
    {
        var input = tmp.GetComponentInParent<TMP_InputField>(true);
        return input != null && input.textComponent == tmp;
    }

    private static bool LooksAlreadyShaped(string value)
    {
        var shaped = 0;
        var letters = 0;
        foreach (var c in value)
        {
            if (c >= 0xFB50 && c <= 0xFEFC)
                shaped++;
            else if (c >= 0x0600 && c <= 0x06FF)
                letters++;
        }
        return shaped > 0 && shaped >= letters;
    }

    private static bool ContainsRtl(string value)
    {
        foreach (var c in value)
        {
            if (c >= 0x0590 && c <= 0x08FF)
                return true;
            if (c >= 0xFB1D && c <= 0xFEFC)
                return true;
        }
        return false;
    }

    private static string Normalize(string value)
    {
        var sb = new StringBuilder(value.Length);
        var pendingSpace = false;
        foreach (var c in value.Trim())
        {
            if (char.IsWhiteSpace(c) || c == '\n' || c == '\r')
            {
                pendingSpace = sb.Length > 0;
                continue;
            }
            if (c == ':' || c == '?' || c == '!' || c == '.' || c == '،')
                continue;
            if (pendingSpace)
            {
                sb.Append(' ');
                pendingSpace = false;
            }
            sb.Append(char.ToLowerInvariant(c));
        }
        return sb.ToString();
    }

    private static TMP_FontAsset ResolveFont()
    {
        if (cachedFont != null)
            return cachedFont;

        foreach (var font in Resources.FindObjectsOfTypeAll<TMP_FontAsset>())
        {
            if (font != null && font.name.IndexOf("Lalezar", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                cachedFont = font;
                return cachedFont;
            }
        }

        foreach (var tmp in Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (tmp != null && tmp.font != null
                && tmp.font.name.IndexOf("Lalezar", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                cachedFont = tmp.font;
                return cachedFont;
            }
        }

        return TMP_Settings.defaultFontAsset;
    }
}
