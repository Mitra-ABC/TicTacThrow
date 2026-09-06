using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class BoardCell : MonoBehaviour
{
    [SerializeField] private int cellIndex;
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text label;
    [SerializeField] private Image markImage;

    private Action<int> onClicked;

    private void Awake()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (label == null)
        {
            label = GetComponentInChildren<TMP_Text>(true);
        }

        if (markImage == null)
        {
            var mark = transform.Find("MarkImage");
            if (mark != null)
                markImage = mark.GetComponent<Image>();
        }

        HideLegacyCircle();
        button.onClick.AddListener(HandleClick);
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(HandleClick);
        }
    }

    private void HideLegacyCircle()
    {
        var img = GetComponent<Image>();
        if (img == null)
            return;
        var color = img.color;
        color.a = 0f;
        img.color = color;
        img.raycastTarget = true;
    }

    public void Configure(Action<int> onCellClicked)
    {
        onClicked = onCellClicked;
    }

    public void SetMark(string symbol)
    {
        SetMark(symbol, null, null);
    }

    public void SetMark(string symbol, Sprite markX, Sprite markO)
    {
        var empty = string.IsNullOrWhiteSpace(symbol);
        Sprite sprite = null;
        if (!empty && string.Equals(symbol.Trim(), GameStrings.SymbolX, System.StringComparison.OrdinalIgnoreCase))
            sprite = markX;
        else if (!empty && string.Equals(symbol.Trim(), GameStrings.SymbolO, System.StringComparison.OrdinalIgnoreCase))
            sprite = markO;

        if (markImage == null)
        {
            var mark = transform.Find("MarkImage");
            if (mark != null)
                markImage = mark.GetComponent<Image>();
        }

        if (markImage != null && (sprite != null || empty))
        {
            markImage.sprite = sprite;
            markImage.enabled = sprite != null;
            markImage.preserveAspect = true;
            if (label != null)
                label.text = string.Empty;
            return;
        }

        if (label != null)
            label.text = empty ? string.Empty : symbol.Trim();
    }

    public void SetInteractable(bool interactable)
    {
        if (button != null)
        {
            button.interactable = interactable;
        }
    }

    public void SetIndex(int index)
    {
        cellIndex = index;
    }

    private void HandleClick()
    {
        onClicked?.Invoke(cellIndex);
    }
}

