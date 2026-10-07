using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Leaderboard page chrome: title banner, season line, and purple rank rows.
/// </summary>
public class LeaderboardChrome : MonoBehaviour
{
    private void Awake()
    {
        Refresh();
    }

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        ApplyLayout();
        ApplyTitle();
    }

    public void ShowSeason(string text)
    {
        SetLabel(FindTmp("SeasonLabel"), text);
    }

    private void ApplyLayout()
    {
        var dim = EnsureImage("LeaderboardDim", null);
        if (dim != null)
        {
            Stretch(dim.rectTransform);
            dim.sprite = null;
            dim.color = new Color(0.03f, 0.01f, 0.1f, 0.42f);
            dim.raycastTarget = false;
            dim.transform.SetSiblingIndex(1);
        }

        PageHeader.Apply(transform, "LeaderboardTitle", "CloseLeaderboardButton", GameStrings.LeaderboardTitle, "LeaderboardTrophy");
        PlaceTop(FindRt("SeasonLabel"), new Vector2(0.5f, 1f), new Vector2(0f, -108f), new Vector2(520f, 30f), new Vector2(0.5f, 1f));

        var scroll = FindRt("LeaderboardScrollView");
        Place(scroll, new Vector2(0f, -36f), new Vector2(980f, 500f));
        StyleScroll(scroll);

        FindRt("SeasonLabel")?.SetAsLastSibling();
        FindRt("CloseLeaderboardButton")?.SetAsLastSibling();
    }

    private void ApplyTitle()
    {
        SetLabel(FindTmp("LeaderboardTitle"), GameStrings.LeaderboardTitle);
        var season = FindTmp("SeasonLabel");
        if (season != null && string.IsNullOrWhiteSpace(PersianUi.ReadLogical(season)))
            SetLabel(season, string.Empty);
        StyleMuted(season);
    }

    private static void StyleScroll(RectTransform scroll)
    {
        if (scroll == null)
            return;

        var sr = scroll.GetComponent<ScrollRect>();
        if (sr != null)
        {
            sr.horizontal = false;
            sr.vertical = true;
            sr.movementType = ScrollRect.MovementType.Elastic;
            sr.scrollSensitivity = 28f;
        }

        var content = sr != null ? sr.content : null;
        if (content == null)
            return;

        content.localScale = Vector3.one;
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.offsetMin = new Vector2(0f, content.offsetMin.y);
        content.offsetMax = new Vector2(0f, content.offsetMax.y);
        content.sizeDelta = new Vector2(0f, content.sizeDelta.y);

        var layout = content.GetComponent<VerticalLayoutGroup>();
        if (layout == null)
            layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(12, 12, 6, 18);
        layout.spacing = 12f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        var fitter = content.GetComponent<ContentSizeFitter>();
        if (fitter == null)
            fitter = content.gameObject.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    public static void SetLabel(TMP_Text tmp, string value)
    {
        if (tmp == null)
            return;
        tmp.raycastTarget = false;
        tmp.enableAutoSizing = true;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.overflowMode = TextOverflowModes.Overflow;
        WritePersian(tmp, value ?? string.Empty);
    }

    public static void WritePersian(TMP_Text tmp, string value)
    {
        if (tmp == null)
            return;
        var logical = value ?? string.Empty;
        if (tmp is RTLTMPro.RTLTextMeshPro rtl)
        {
            rtl.ForceFix = true;
            rtl.Farsi = true;
            rtl.FixTags = true;
            if (rtl.OriginalText == logical)
                rtl.UpdateText();
            else
                rtl.text = logical;
            return;
        }

        tmp.isRightToLeftText = false;
        tmp.text = PersianUi.Shape(logical);
    }

    public static void WriteNumber(TMP_Text tmp, string value)
    {
        if (tmp == null)
            return;
        tmp.isRightToLeftText = false;
        tmp.overflowMode = TextOverflowModes.Overflow;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        if (tmp is RTLTMPro.RTLTextMeshPro rtl)
        {
            rtl.ForceFix = false;
            rtl.Farsi = false;
            rtl.PreserveNumbers = true;
            rtl.text = value ?? string.Empty;
            rtl.isRightToLeftText = false;
            return;
        }

        tmp.text = value ?? string.Empty;
    }

    private static void StyleMuted(TMP_Text tmp)
    {
        if (tmp == null)
            return;
        tmp.color = new Color(1f, 0.86f, 0.45f, 0.95f);
        tmp.enableAutoSizing = true;
        tmp.fontSizeMax = 22f;
        tmp.fontSizeMin = 14f;
        tmp.alignment = TextAlignmentOptions.Center;
    }

    private Image EnsureImage(string objectName, Sprite sprite)
    {
        var rt = FindRt(objectName);
        GameObject go;
        if (rt == null)
        {
            go = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.layer = gameObject.layer;
            go.transform.SetParent(transform, false);
            rt = go.GetComponent<RectTransform>();
        }
        else
        {
            go = rt.gameObject;
        }

        go.SetActive(true);
        var image = go.GetComponent<Image>();
        if (image == null)
            image = go.AddComponent<Image>();
        if (sprite != null)
            image.sprite = sprite;
        return image;
    }

    private TMP_Text FindTmp(string objectName)
    {
        var rt = FindRt(objectName);
        return rt != null ? rt.GetComponent<TMP_Text>() : null;
    }

    private RectTransform FindRt(string objectName)
    {
        foreach (var t in GetComponentsInChildren<Transform>(true))
        {
            if (t != null && t.name == objectName)
                return t as RectTransform;
        }

        return null;
    }

    public static Sprite FindSprite(string name)
    {
        return PageHeader.FindSprite(name);
    }

    private static void Stretch(RectTransform rt)
    {
        if (rt == null)
            return;
        rt.localScale = Vector3.one;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static void Place(RectTransform rt, Vector2 pos, Vector2 size)
    {
        if (rt == null)
            return;
        rt.localScale = Vector3.one;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    private static void PlaceTop(RectTransform rt, Vector2 anchor, Vector2 pos, Vector2 size, Vector2 pivot)
    {
        if (rt == null)
            return;
        rt.localScale = Vector3.one;
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }
}
