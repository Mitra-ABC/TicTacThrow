using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BoardCell : MonoBehaviour
{
    [SerializeField] private int cellIndex;
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text label;

    private Action<int> onClicked;

    private void Awake()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (label == null)
        {
            label = GetComponentInChildren<TMP_Text>();
        }

        if (button != null)
        {
            button.onClick.AddListener(HandleClick);
        }
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(HandleClick);
        }
    }

    public void Configure(Action<int> onCellClicked)
    {
        onClicked = onCellClicked;
    }

    public void SetMark(string symbol)
    {
        if (label != null)
        {
            label.text = string.IsNullOrEmpty(symbol) ? string.Empty : symbol;
        }
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

