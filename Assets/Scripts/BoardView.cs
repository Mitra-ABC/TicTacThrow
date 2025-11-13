using System;
using UnityEngine;

public class BoardView : MonoBehaviour
{
    [SerializeField] private BoardCell[] cells;

    private Action<int> onCellClicked;

    public void Initialize(Action<int> onCellSelected)
    {
        onCellClicked = onCellSelected;

        if (cells == null || cells.Length == 0)
        {
            cells = GetComponentsInChildren<BoardCell>(true);
        }

        for (int i = 0; i < cells.Length; i++)
        {
            var cell = cells[i];
            if (cell == null) continue;
            cell.SetIndex(i);
            cell.Configure(HandleCellClick);
        }
    }

    public void RenderBoard(string[] board, bool allowInteraction)
    {
        if (cells == null) return;

        for (int i = 0; i < cells.Length; i++)
        {
            var symbol = board != null && i < board.Length ? board[i] : null;
            cells[i]?.SetMark(symbol);

            bool canInteract = allowInteraction && string.IsNullOrEmpty(symbol);
            cells[i]?.SetInteractable(canInteract);
        }
    }

    public void Clear()
    {
        RenderBoard(null, false);
    }

    private void HandleCellClick(int index)
    {
        onCellClicked?.Invoke(index);
    }
}

