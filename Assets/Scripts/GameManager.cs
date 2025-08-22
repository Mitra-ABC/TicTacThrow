using UnityEngine;
using UnityEngine.UI;
using TMPro;  

public class GameManager : MonoBehaviour
{
    private int[] board = new int[9];

    public Cell[] cells;           // 9 تا Cell را در Inspector در این آرایه بچین
    public Sprite xSprite;         // تصویر X
    public Sprite oSprite;         // تصویر O
    public TextMeshProUGUI statusText; // متن وضعیت (نوبت یا برد/مساوی)
    public Button resetButton;     // دکمه ریست

    private bool xTurn = true;     // نوبت X شروع
    private bool gameOver = false;

    void Start()
    {
        NewGame();
        if (resetButton != null)
            resetButton.onClick.AddListener(NewGame);
    }

    public void NewGame()
    {
        for (int i = 0; i < 9; i++) board[i] = 0;
        xTurn = true;
        gameOver = false;

        if (cells != null)
        {
            foreach (var c in cells) c.Clear();
        }

        SetStatus("Turn: X");
    }

    public void OnCellClicked(int index, Cell cell)
    {
        if (gameOver) return;
        if (index < 0 || index > 8) return;
        if (board[index] != 0) return;

        int player = xTurn ? 1 : 2;
        board[index] = player;

        // نمایش نشانه
        var mark = xTurn ? xSprite : oSprite;
        cell.SetMark(mark);

        // بررسی برد/مساوی
        int winner = CheckWinner();
        if (winner != 0)
        {
            gameOver = true;
            SetStatus(winner == 1 ? "Win: X" : "Win: O");
            return;
        }

        if (IsBoardFull())
        {
            gameOver = true;
            SetStatus("Oops!");
            return;
        }

        // تغییر نوبت
        xTurn = !xTurn;
        SetStatus(xTurn ? "Turn: X" : "Turn: O");
    }

    private void SetStatus(string msg)
    {
        if (statusText != null) statusText.text = msg;
        else Debug.Log(msg);
    }

    private bool IsBoardFull()
    {
        for (int i = 0; i < 9; i++)
            if (board[i] == 0) return false;
        return true;
    }

    private int CheckWinner()
    {
        int[][] lines = new int[][]
        {
            new int[]{0,1,2}, new int[]{3,4,5}, new int[]{6,7,8}, // ردیف‌ها
            new int[]{0,3,6}, new int[]{1,4,7}, new int[]{2,5,8}, // ستون‌ها
            new int[]{0,4,8}, new int[]{2,4,6}                     // قطرها
        };

        foreach (var line in lines)
        {
            int a = line[0], b = line[1], c = line[2];
            if (board[a] != 0 && board[a] == board[b] && board[b] == board[c])
                return board[a]; // 1 یا 2
        }
        return 0; // هنوز برنده‌ای نیست
    }
}
