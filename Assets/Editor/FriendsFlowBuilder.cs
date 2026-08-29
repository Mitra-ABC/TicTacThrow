using RTLTMPro;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Builds friends / join-room / waiting screens from the 18.xx mockups.
/// Does not destroy GameManager-referenced objects.
/// </summary>
public static class FriendsFlowBuilder
{
    private const string FontPath = "Assets/Fonts/Vazir Black SDF.asset";

    [MenuItem("Tools/DuoDooz/Build Friends Flow")]
    public static void Build()
    {
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        BuildFriends(font);
        BuildJoin(font);
        BuildWaiting(font);

        var scene = EditorSceneManager.GetActiveScene();
        if (scene.IsValid())
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        Debug.Log("DuoDooz: Friends flow screens built.");
    }

    private static void BuildFriends(TMP_FontAsset font)
    {
        var panel = FindNamed("FriendlyGamePanel");
        if (panel == null)
        {
            Debug.LogError("FriendlyGamePanel not found.");
            return;
        }

        var root = panel.GetComponent<RectTransform>();
        StretchPanel(root);
        Hide(root, "CreateRoomImage");
        Hide(root, "JoinRoomModeButtonImage");
        Hide(root, "BackFromFriendlyGameButtonImage");
        Hide(root, "BackFromFriendlyGameButtonLabel");
        Hide(root, "CreateRoom");
        Hide(root, "CreateRoomLabel");

        var bg = EnsureImage(root, "FriendsBg", ImportSprite("Assets/UI/Friends/FriendsBg.png"), 0);
        Stretch(bg.rectTransform);
        bg.raycastTarget = false;

        StyleIconButton(FindDeep(root, "BackFromFriendlyGameButton"), ImportSprite("Assets/UI/Friends/FriendsBack.png"),
            new Vector2(0f, 1f), new Vector2(18f, -14f), new Vector2(72f, 72f), new Vector2(0f, 1f));

        var title = FindDeep(root, "FriendlyPanelTitle") as RectTransform;
        if (title != null)
        {
            title.gameObject.SetActive(true);
            PlaceTop(title, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(100f, -22f), new Vector2(360f, 56f), new Vector2(0f, 1f));
            StyleLabel(title.GetComponent<TMP_Text>(), font, 40f, Color.white, TextAlignmentOptions.MidlineRight);
            PersianUi.SetText(title.GetComponent<TMP_Text>(), GameStrings.PlayWithFriendsButton);
        }

        StyleCard(FindDeep(root, "CreateRoomButton"), ImportSprite("Assets/UI/Friends/FriendsCreateCard.png"),
            new Vector2(-230f, -24f), new Vector2(360f, 440f), GameStrings.CreateRoomButton, GameStrings.CreateRoomHint, font);
        StyleCard(FindDeep(root, "JoinRoomModeButton"), ImportSprite("Assets/UI/Friends/FriendsJoinCard.png"),
            new Vector2(230f, -24f), new Vector2(360f, 440f), GameStrings.JoinRoomButton, GameStrings.JoinRoomHint, font);

        EditorUtility.SetDirty(panel);
    }

    private static void BuildJoin(TMP_FontAsset font)
    {
        var panel = FindNamed("JoinRoomPanel");
        if (panel == null)
        {
            Debug.LogError("JoinRoomPanel not found.");
            return;
        }

        var root = panel.GetComponent<RectTransform>();
        StretchPanel(root);
        Hide(root, "JoinRoomInputImage");
        Hide(root, "SubmitJoinButtonImage");
        Hide(root, "BackFromJoinButtonImage");
        Hide(root, "BackFromJoinButtonLabel");
        Hide(root, "BackFromJoinButtonLabelLabel");
        Hide(root, "SubmitJoinButtonLabelLabel");

        var bg = EnsureImage(root, "JoinBg", ImportSprite("Assets/UI/Join/JoinBg.png"), 0);
        Stretch(bg.rectTransform);
        bg.raycastTarget = false;

        var titleSprite = ImportSprite("Assets/UI/Join/JoinTitle.png");
        var digitSprite = ImportSprite("Assets/UI/Join/JoinDigit.png");
        var submitSprite = ImportSprite("Assets/UI/Join/JoinSubmit.png");
        var dividerSprite = ImportSprite("Assets/UI/Join/JoinDivider.png");
        var shieldSprite = ImportSprite("Assets/UI/Join/JoinShield.png");

        StyleIconButton(FindDeep(root, "BackFromJoinButton"), ImportSprite("Assets/UI/Friends/FriendsBack.png"),
            new Vector2(0f, 1f), new Vector2(18f, -14f), new Vector2(72f, 72f), new Vector2(0f, 1f));

        var title = EnsureImage(root, "JoinTitle", titleSprite, 2);
        Place(title.rectTransform, new Vector2(0f, 210f), new Vector2(520f, 120f));
        title.preserveAspect = true;
        title.raycastTarget = false;

        var hint = EnsureLabel(root, "JoinInstruction", GameStrings.JoinRoomInstruction, 26f, 3, font);
        Place(hint.rectTransform, new Vector2(0f, 118f), new Vector2(480f, 36f));

        var divL = EnsureImage(root, "JoinDividerLeft", dividerSprite, 4);
        Place(divL.rectTransform, new Vector2(-280f, 118f), new Vector2(160f, 18f));
        divL.preserveAspect = true;
        divL.raycastTarget = false;
        var divR = EnsureImage(root, "JoinDividerRight", dividerSprite, 5);
        Place(divR.rectTransform, new Vector2(280f, 118f), new Vector2(160f, 18f));
        divR.preserveAspect = true;
        divR.raycastTarget = false;

        var row = EnsureRect(root, "JoinDigitRow", 6);
        Place(row, new Vector2(0f, 18f), new Vector2(720f, 96f));
        var digitLabels = new TMP_Text[6];
        for (var i = 0; i < 6; i++)
        {
            var box = EnsureImage(row, "JoinDigit" + i, digitSprite, i);
            var x = -300f + i * 120f;
            Place(box.rectTransform, new Vector2(x, 0f), new Vector2(96f, 96f));
            box.preserveAspect = true;
            box.raycastTarget = false;
            var digit = EnsureLabel(box.rectTransform, "JoinDigitLabel", string.Empty, 42f, 0, font);
            StretchInsets(digit.rectTransform, new Vector2(8f, 8f), new Vector2(-8f, -8f));
            digitLabels[i] = digit;
        }

        var input = FindDeep(root, "JoinRoomInput") as RectTransform;
        if (input != null)
        {
            input.SetParent(row, false);
            input.gameObject.SetActive(true);
            Stretch(input);
            var field = input.GetComponent<TMP_InputField>();
            if (field != null)
            {
                field.contentType = TMP_InputField.ContentType.Standard;
                field.characterValidation = TMP_InputField.CharacterValidation.None;
                field.keyboardType = TouchScreenKeyboardType.NumberPad;
                field.characterLimit = GameStrings.RoomCodeLength;
                if (field.textComponent != null)
                    field.textComponent.color = new Color(1f, 1f, 1f, 0f);
                if (field.placeholder is TMP_Text ph)
                    PersianUi.SetText(ph, string.Empty);
            }

            var img = input.GetComponent<Image>();
            if (img != null)
            {
                img.color = new Color(1f, 1f, 1f, 0.01f);
                img.raycastTarget = true;
            }
        }

        StyleSpriteButton(FindDeep(root, "SubmitJoinButton"), submitSprite,
            new Vector2(0f, -140f), new Vector2(480f, 100f), GameStrings.JoinRoomButton, font, 40f);

        var footer = EnsureRect(root, "JoinFooter", 10);
        Place(footer, new Vector2(0f, -230f), new Vector2(520f, 36f));
        var shield = EnsureImage(footer, "JoinShield", shieldSprite, 0);
        PlaceLocal(shield.rectTransform, new Vector2(1f, 0.5f), new Vector2(-18f, 0f), new Vector2(28f, 28f));
        shield.preserveAspect = true;
        shield.raycastTarget = false;
        var foot = EnsureLabel(footer, "JoinFooterLabel", GameStrings.JoinRoomNumbersOnly, 20f, 1, font);
        StretchInsets(foot.rectTransform, new Vector2(8f, 0f), new Vector2(-48f, 0f));

        var chrome = panel.GetComponent<JoinRoomChrome>();
        if (chrome == null)
            chrome = panel.AddComponent<JoinRoomChrome>();
        var so = new SerializedObject(chrome);
        so.FindProperty("input").objectReferenceValue = input != null ? input.GetComponent<TMP_InputField>() : null;
        var digitsProp = so.FindProperty("digits");
        digitsProp.arraySize = digitLabels.Length;
        for (var i = 0; i < digitLabels.Length; i++)
            digitsProp.GetArrayElementAtIndex(i).objectReferenceValue = digitLabels[i];
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(panel);
    }

    private static void BuildWaiting(TMP_FontAsset font)
    {
        var panel = FindNamed("WaitingPanel");
        if (panel == null)
        {
            Debug.LogError("WaitingPanel not found.");
            return;
        }

        var root = panel.GetComponent<RectTransform>();
        StretchPanel(root);
        Hide(root, "CancelWaitingButtonImage");
        Hide(root, "CancelWaitingButtonLabel");

        var bg = EnsureImage(root, "WaitBg", ImportSprite("Assets/UI/Waiting/WaitBg.png"), 0);
        Stretch(bg.rectTransform);
        bg.raycastTarget = false;

        var cancel = FindDeep(root, "CancelWaitingButton");
        var waitBack = EnsureImage(root, "WaitBackButton", ImportSprite("Assets/UI/Friends/FriendsBack.png"), 1);
        PlaceTop(waitBack.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -14f), new Vector2(72f, 72f), new Vector2(0f, 1f));
        waitBack.preserveAspect = true;
        var waitBackBtn = EnsureButton(waitBack);
        waitBackBtn.onClick.RemoveAllListeners();
        foreach (var gm in Object.FindObjectsByType<GameManager>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (gm == null)
                continue;
            UnityEditor.Events.UnityEventTools.AddPersistentListener(waitBackBtn.onClick, gm.OnBackToLobby);
            break;
        }

        var created = EnsureLabel(root, "WaitCreatedTitle", GameStrings.RoomCreatedTitle, 44f, 2, font);
        Place(created.rectTransform, new Vector2(0f, 200f), new Vector2(640f, 56f));

        var status = FindDeep(root, "WaitingStatusLabel") as RectTransform;
        if (status != null)
        {
            status.gameObject.SetActive(true);
            Place(status, new Vector2(0f, 148f), new Vector2(700f, 36f));
            StyleLabel(status.GetComponent<TMP_Text>(), font, 24f, Color.white, TextAlignmentOptions.Center);
            PersianUi.SetText(status.GetComponent<TMP_Text>(), GameStrings.WaitingForFriend);
        }

        var card = EnsureImage(root, "WaitCodeCard", ImportSprite("Assets/UI/Waiting/WaitCodeCard.png"), 4);
        Place(card.rectTransform, new Vector2(0f, 20f), new Vector2(620f, 200f));
        card.preserveAspect = true;
        card.raycastTarget = false;

        var badge = EnsureImage(card.rectTransform, "WaitBadge", ImportSprite("Assets/UI/Waiting/WaitBadge.png"), 0);
        Place(badge.rectTransform, new Vector2(0f, 88f), new Vector2(56f, 40f));
        badge.preserveAspect = true;
        badge.raycastTarget = false;

        var codeCaption = EnsureLabel(card.rectTransform, "WaitCodeCaption", GameStrings.YourRoomCode, 22f, 1, font);
        Place(codeCaption.rectTransform, new Vector2(0f, 36f), new Vector2(400f, 28f));

        var code = FindDeep(root, "ShareRoomIdLabel") as RectTransform;
        if (code != null)
        {
            code.SetParent(card.rectTransform, false);
            code.gameObject.SetActive(true);
            Place(code, new Vector2(0f, -16f), new Vector2(520f, 72f));
            StyleLabel(code.GetComponent<TMP_Text>(), font, 56f, Color.white, TextAlignmentOptions.Center);
        }

        var copy = EnsureImage(root, "WaitCopyButton", ImportSprite("Assets/UI/Waiting/WaitCopyBtn.png"), 8);
        Place(copy.rectTransform, new Vector2(-170f, -180f), new Vector2(280f, 78f));
        copy.preserveAspect = true;
        var copyBtn = EnsureButton(copy);
        var copyIcon = EnsureImage(copy.rectTransform, "WaitCopyIcon", ImportSprite("Assets/UI/Waiting/WaitCopyIcon.png"), 0);
        PlaceLocal(copyIcon.rectTransform, new Vector2(1f, 0.5f), new Vector2(-36f, 0f), new Vector2(36f, 36f));
        copyIcon.preserveAspect = true;
        copyIcon.raycastTarget = false;
        var copyLabel = EnsureLabel(copy.rectTransform, "WaitCopyLabel", GameStrings.CopyButton, 28f, 1, font);
        StretchInsets(copyLabel.rectTransform, new Vector2(24f, 8f), new Vector2(-64f, -8f));

        var share = EnsureImage(root, "WaitShareButton", ImportSprite("Assets/UI/Waiting/WaitShareBtn.png"), 9);
        Place(share.rectTransform, new Vector2(170f, -180f), new Vector2(280f, 78f));
        share.preserveAspect = true;
        var shareBtn = EnsureButton(share);
        var shareIcon = EnsureImage(share.rectTransform, "WaitShareIcon", ImportSprite("Assets/UI/Waiting/WaitShareIcon.png"), 0);
        PlaceLocal(shareIcon.rectTransform, new Vector2(1f, 0.5f), new Vector2(-36f, 0f), new Vector2(36f, 36f));
        shareIcon.preserveAspect = true;
        shareIcon.raycastTarget = false;
        var shareLabel = EnsureLabel(share.rectTransform, "WaitShareLabel", GameStrings.ShareButton, 28f, 1, font);
        StretchInsets(shareLabel.rectTransform, new Vector2(24f, 8f), new Vector2(-64f, -8f));

        StyleSpriteButton(cancel, ImportSprite("Assets/UI/Waiting/WaitCancelBtn.png"),
            new Vector2(0f, -280f), new Vector2(280f, 72f), GameStrings.CancelRoomButton, font, 28f);
        if (cancel != null)
        {
            var x = EnsureImage(cancel, "WaitCancelIcon", ImportSprite("Assets/UI/Waiting/WaitCancelIcon.png"), 0);
            PlaceLocal(x.rectTransform, new Vector2(0f, 0.5f), new Vector2(36f, 0f), new Vector2(28f, 28f));
            x.preserveAspect = true;
            x.raycastTarget = false;
        }

        var chrome = panel.GetComponent<WaitingChrome>();
        if (chrome == null)
            chrome = panel.AddComponent<WaitingChrome>();
        var so = new SerializedObject(chrome);
        so.FindProperty("copyButton").objectReferenceValue = copyBtn;
        so.FindProperty("shareButton").objectReferenceValue = shareBtn;
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(panel);
    }

    private static void StyleCard(Transform button, Sprite sprite, Vector2 pos, Vector2 size, string title, string hint, TMP_FontAsset font)
    {
        if (button == null)
            return;
        button.gameObject.SetActive(true);
        var rt = button as RectTransform;
        if (rt != null)
            Place(rt, pos, size);
        HideChildImages(button);
        Hide(button, "CreateRoom");
        Hide(button, "CreateRoomLabel");
        Hide(button, "PersianLabel");
        foreach (var text in button.GetComponentsInChildren<Text>(true))
        {
            if (text == null)
                continue;
            text.enabled = false;
            text.text = string.Empty;
            if (text.GetComponent<TMP_Text>() == null && text.transform != button)
                text.gameObject.SetActive(false);
        }

        var image = button.GetComponent<Image>();
        if (image != null)
        {
            image.sprite = sprite;
            image.type = Image.Type.Simple;
            image.preserveAspect = true;
            image.color = Color.white;
            image.raycastTarget = true;
        }

        var tmp = EnsureLabel(button, button.name + "Label", title, 36f, button.childCount, font);
        var labelRt = tmp.rectTransform;
        labelRt.anchorMin = new Vector2(0.08f, 0.08f);
        labelRt.anchorMax = new Vector2(0.92f, 0.26f);
        labelRt.offsetMin = Vector2.zero;
        labelRt.offsetMax = Vector2.zero;
        tmp.fontSizeMax = 36f;
        tmp.fontSizeMin = 16f;

        var sub = EnsureLabel(button, button.name + "Hint", hint, 18f, button.childCount, font);
        var subRt = sub.rectTransform;
        subRt.anchorMin = new Vector2(0.1f, 0.02f);
        subRt.anchorMax = new Vector2(0.9f, 0.12f);
        subRt.offsetMin = Vector2.zero;
        subRt.offsetMax = Vector2.zero;
        sub.fontSizeMax = 18f;
        sub.color = new Color(1f, 1f, 1f, 0.85f);
    }

    private static void StyleSpriteButton(Transform button, Sprite sprite, Vector2 pos, Vector2 size, string label, TMP_FontAsset font, float fontMax)
    {
        if (button == null)
            return;
        button.gameObject.SetActive(true);
        var rt = button as RectTransform;
        if (rt != null)
            Place(rt, pos, size);
        HideChildImages(button);

        var image = button.GetComponent<Image>();
        if (image != null)
        {
            image.sprite = sprite;
            image.type = Image.Type.Simple;
            image.preserveAspect = true;
            image.color = Color.white;
            image.raycastTarget = true;
        }

        var tmp = button.GetComponentInChildren<TMP_Text>(true);
        if (tmp == null)
            tmp = EnsureLabel(button, button.name + "Label", label, fontMax, 1, font);
        tmp.gameObject.SetActive(true);
        StretchInsets(tmp.rectTransform, new Vector2(48f, 8f), new Vector2(-48f, -8f));
        tmp.fontSizeMax = fontMax;
        tmp.fontSizeMin = 14f;
        tmp.enableAutoSizing = true;
        tmp.fontStyle = FontStyles.Normal;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;
        if (font != null)
            tmp.font = font;
        PersianUi.SetText(tmp, label);
    }

    private static void StyleIconButton(Transform button, Sprite sprite, Vector2 anchor, Vector2 pos, Vector2 size, Vector2 pivot)
    {
        if (button == null)
            return;
        button.gameObject.SetActive(true);
        var rt = button as RectTransform;
        if (rt != null)
            PlaceTop(rt, anchor, anchor, pos, size, pivot);

        var image = button.GetComponent<Image>();
        if (image != null)
        {
            image.sprite = sprite;
            image.type = Image.Type.Simple;
            image.preserveAspect = true;
            image.color = Color.white;
            image.raycastTarget = true;
        }

        foreach (var tmp in button.GetComponentsInChildren<TMP_Text>(true))
        {
            if (tmp != null)
                tmp.gameObject.SetActive(false);
        }

        foreach (var childImg in button.GetComponentsInChildren<Image>(true))
        {
            if (childImg == null || childImg.transform == button)
                continue;
            childImg.gameObject.SetActive(false);
        }
    }

    private static void StyleLabel(TMP_Text tmp, TMP_FontAsset font, float size, Color color, TextAlignmentOptions align)
    {
        if (tmp == null)
            return;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMax = size;
        tmp.fontSizeMin = Mathf.Max(10f, size * 0.45f);
        tmp.fontStyle = FontStyles.Normal;
        tmp.alignment = align;
        tmp.color = color;
        tmp.raycastTarget = false;
        if (font != null)
            tmp.font = font;
        PersianUi.Style(tmp);
    }

    private static Image EnsureImage(Transform parent, string name, Sprite sprite, int sibling)
    {
        var existing = parent.Find(name);
        GameObject go;
        if (existing == null)
        {
            go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.layer = parent.gameObject.layer;
            go.transform.SetParent(parent, false);
        }
        else
        {
            go = existing.gameObject;
        }

        go.SetActive(true);
        go.transform.SetSiblingIndex(Mathf.Clamp(sibling, 0, parent.childCount - 1));
        var image = go.GetComponent<Image>();
        if (image == null)
            image = go.AddComponent<Image>();
        image.sprite = sprite;
        image.color = Color.white;
        image.type = Image.Type.Simple;
        return image;
    }

    private static RectTransform EnsureRect(Transform parent, string name, int sibling)
    {
        var existing = parent.Find(name) as RectTransform;
        if (existing == null)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = parent.gameObject.layer;
            go.transform.SetParent(parent, false);
            existing = go.GetComponent<RectTransform>();
        }

        existing.gameObject.SetActive(true);
        existing.SetSiblingIndex(Mathf.Clamp(sibling, 0, parent.childCount - 1));
        return existing;
    }

    private static RTLTextMeshPro EnsureLabel(Transform parent, string name, string text, float size, int sibling, TMP_FontAsset font)
    {
        var existing = parent.Find(name);
        GameObject go;
        RTLTextMeshPro rtl;
        if (existing == null)
        {
            go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
            go.layer = parent.gameObject.layer;
            go.transform.SetParent(parent, false);
            rtl = go.AddComponent<RTLTextMeshPro>();
        }
        else
        {
            go = existing.gameObject;
            rtl = go.GetComponent<RTLTextMeshPro>();
            if (rtl == null)
                rtl = go.AddComponent<RTLTextMeshPro>();
        }

        go.SetActive(true);
        go.transform.SetSiblingIndex(Mathf.Clamp(sibling, 0, parent.childCount - 1));
        rtl.Farsi = true;
        rtl.FixTags = true;
        rtl.ForceFix = true;
        rtl.enableAutoSizing = true;
        rtl.fontSizeMax = size;
        rtl.fontSizeMin = Mathf.Max(10f, size * 0.5f);
        rtl.fontStyle = FontStyles.Normal;
        rtl.alignment = TextAlignmentOptions.Center;
        rtl.color = Color.white;
        rtl.raycastTarget = false;
        if (font != null)
            rtl.font = font;
        PersianUi.SetText(rtl, text);
        return rtl;
    }

    private static Button EnsureButton(Image image)
    {
        var btn = image.GetComponent<Button>();
        if (btn == null)
            btn = image.gameObject.AddComponent<Button>();
        btn.targetGraphic = image;
        btn.transition = Selectable.Transition.ColorTint;
        image.raycastTarget = true;
        return btn;
    }

    private static void StretchPanel(RectTransform rt)
    {
        rt.localScale = Vector3.one;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.localScale = Vector3.one;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;
    }

    private static void StretchInsets(RectTransform rt, Vector2 min, Vector2 max)
    {
        rt.localScale = Vector3.one;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.offsetMin = min;
        rt.offsetMax = max;
    }

    private static void Place(RectTransform rt, Vector2 pos, Vector2 size)
    {
        rt.localScale = Vector3.one;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    private static void PlaceTop(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size, Vector2? pivot = null)
    {
        rt.localScale = Vector3.one;
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot ?? new Vector2(anchorMin.x, 1f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    private static void PlaceLocal(RectTransform rt, Vector2 anchor, Vector2 pos, Vector2 size, Vector2? pivot = null)
    {
        rt.localScale = Vector3.one;
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = pivot ?? new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    private static void Hide(Transform parent, string name)
    {
        if (parent == null)
            return;
        foreach (var t in parent.GetComponentsInChildren<Transform>(true))
        {
            if (t != null && t.name == name)
                t.gameObject.SetActive(false);
        }
    }

    private static void HideChildImages(Transform button)
    {
        if (button == null)
            return;
        foreach (var img in button.GetComponentsInChildren<Image>(true))
        {
            if (img != null && img.transform != button)
                img.gameObject.SetActive(false);
        }
    }

    private static Transform FindDeep(Transform parent, string name)
    {
        if (parent == null)
            return null;
        foreach (var t in parent.GetComponentsInChildren<Transform>(true))
        {
            if (t != null && t.name == name)
                return t;
        }

        return null;
    }

    private static Sprite ImportSprite(string path)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static GameObject FindNamed(string name)
    {
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (go != null && go.name == name)
                return go;
        }

        return null;
    }
}
