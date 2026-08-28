using System.Collections;
using RTLTMPro;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Full-width banner on the active Canvas so the player always sees errors and short notices.
/// </summary>
public class ErrorToast : MonoBehaviour
{
    public enum Kind
    {
        Error,
        Success,
        Info
    }

    private const float DefaultSeconds = 4.5f;

    private CanvasGroup canvasGroup;
    private Image background;
    private TMP_Text label;
    private Coroutine hideRoutine;

    public static ErrorToast Ensure(TMP_Text fontSource = null)
    {
        var existing = FindFirstObjectByType<ErrorToast>();
        if (existing != null)
            return existing;

        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
            return null;

        var root = new GameObject("ErrorToast", typeof(RectTransform), typeof(ErrorToast));
        var toast = root.GetComponent<ErrorToast>();
        toast.Build(canvas.transform, fontSource);
        return toast;
    }

    public void Show(string message, Kind kind = Kind.Error, float seconds = DefaultSeconds)
    {
        if (string.IsNullOrWhiteSpace(message) || label == null)
            return;

        gameObject.SetActive(true);
        transform.SetAsLastSibling();
        PersianUi.SetText(label, message);
        ApplyKind(kind);
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        if (hideRoutine != null)
            StopCoroutine(hideRoutine);
        hideRoutine = StartCoroutine(HideAfter(seconds));
    }

    public void Hide()
    {
        if (hideRoutine != null)
        {
            StopCoroutine(hideRoutine);
            hideRoutine = null;
        }
        if (label != null)
            label.text = string.Empty;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }
    }

    private void Build(Transform canvas, TMP_Text fontSource)
    {
        var rt = (RectTransform)transform;
        rt.SetParent(canvas, false);
        rt.anchorMin = new Vector2(0.06f, 0.86f);
        rt.anchorMax = new Vector2(0.94f, 0.97f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.pivot = new Vector2(0.5f, 1f);
        rt.SetAsLastSibling();

        background = gameObject.AddComponent<Image>();
        canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;

        var textGo = new GameObject("Message", typeof(RectTransform));
        var textRt = (RectTransform)textGo.transform;
        textRt.SetParent(rt, false);
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = new Vector2(16f, 8f);
        textRt.offsetMax = new Vector2(-16f, -8f);

        label = textGo.AddComponent<RTLTextMeshPro>();
        PersianUi.Style(label);
        label.fontSize = 28f;
        label.alignment = TextAlignmentOptions.Midline;
        label.enableWordWrapping = true;
        label.color = Color.white;
        label.text = string.Empty;

        ApplyKind(Kind.Error);
    }

    private void ApplyKind(Kind kind)
    {
        if (background == null)
            return;
        switch (kind)
        {
            case Kind.Success:
                background.color = new Color(0.12f, 0.45f, 0.22f, 0.94f);
                break;
            case Kind.Info:
                background.color = new Color(0.49f, 0.23f, 0.93f, 0.94f);
                break;
            default:
                background.color = new Color(0.96f, 0.25f, 0.37f, 0.94f);
                break;
        }
    }

    private IEnumerator HideAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        Hide();
    }
}
