using System;
using UnityEngine;

public class BoardView : MonoBehaviour
{
    [SerializeField] private BoardCell[] cells;

    private Action<int> onCellClicked;
    private bool boardHasServerData;

    public void Initialize(Action<int> onCellSelected)
    {
        onCellClicked = onCellSelected ?? throw new ArgumentNullException(nameof(onCellSelected));

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
            cell.SetMark(null);
            cell.SetInteractable(true);
        }

        boardHasServerData = false;
    }

    public void RenderBoard(string[] board, bool allowInteraction)
    {
        if (cells == null || cells.Length == 0) return;

        if (board != null)
        {
            boardHasServerData = true;
        }

        bool effectiveAllowInteraction = boardHasServerData && allowInteraction;

        for (int i = 0; i < cells.Length; i++)
        {
            var cell = cells[i];
            if (cell == null) continue;

            var symbol = board != null && i < board.Length ? board[i] : null;
            bool cellEmpty = IsSymbolEmpty(symbol);
            cell.SetMark(cellEmpty ? null : symbol?.Trim());

            bool canInteract = !boardHasServerData || (effectiveAllowInteraction && cellEmpty);
            cell.SetInteractable(canInteract);
        }
    }

    public void Clear()
    {
        boardHasServerData = false;
        if (cells == null) return;

        for (int i = 0; i < cells.Length; i++)
        {
            var cell = cells[i];
            if (cell == null) continue;
            cell.SetMark(null);
            cell.SetInteractable(true);
        }
    }

    private void HandleCellClick(int index)
    {
        onCellClicked?.Invoke(index);
    }

    private static bool IsSymbolEmpty(string symbol)
    {
        if (string.IsNullOrWhiteSpace(symbol)) return true;
        return string.Equals(symbol, "null", StringComparison.OrdinalIgnoreCase)
               || string.Equals(symbol, "empty", StringComparison.OrdinalIgnoreCase)
               || symbol == "-";
    }
}

