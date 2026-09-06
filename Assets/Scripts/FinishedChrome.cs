using RTLTMPro;
using TMPro;
using UnityEngine;

/// <summary>
/// Victory / defeat result chrome. Number labels stay LTR so score math does not scramble.
/// </summary>
public class FinishedChrome : MonoBehaviour
{
    [SerializeField] private GameObject titleArt;
    [SerializeField] private GameObject trophy;
    [SerializeField] private GameObject confetti;
    [SerializeField] private TMP_Text resultLabel;
    [SerializeField] private TMP_Text subtitle;
    [SerializeField] private TMP_Text scoreOld;
    [SerializeField] private TMP_Text scoreArrow;
    [SerializeField] private TMP_Text scoreNew;
    [SerializeField] private TMP_Text scoreDelta;
    [SerializeField] private TMP_Text coinValue;

    public void Show(string result, string localSymbol, int oldRating, int newRating, int coinsWon = 0)
    {
        var draw = string.Equals(result, GameStrings.ResultDraw, System.StringComparison.OrdinalIgnoreCase);
        var win = !draw && !string.IsNullOrEmpty(result) &&
                  string.Equals(result, localSymbol, System.StringComparison.OrdinalIgnoreCase);

        if (titleArt != null)
            titleArt.SetActive(win);
        if (trophy != null)
            trophy.SetActive(win);
        if (confetti != null)
            confetti.SetActive(win);
        if (resultLabel != null)
            resultLabel.gameObject.SetActive(!win);

        if (subtitle != null)
        {
            PersianUi.SetText(subtitle, win
                ? GameStrings.VictorySubtitle
                : draw
                    ? GameStrings.DrawSubtitle
                    : GameStrings.DefeatSubtitle);
        }

        FillScore(oldRating, newRating > int.MinValue ? newRating : oldRating);
        var coinText = coinsWon > 0
            ? GameStrings.ToPersianDigits($"+{coinsWon}")
            : GameStrings.ToPersianDigits("0");
        SetPlain(coinValue, coinText, new Color(1f, 0.86f, 0.22f, 1f));
    }

    private void FillScore(int oldRating, int newRating)
    {
        var haveOld = oldRating > int.MinValue && oldRating >= 0;
        var haveNew = newRating > int.MinValue && newRating >= 0;
        if (!haveNew && haveOld)
        {
            haveNew = true;
            newRating = oldRating;
            haveOld = false;
        }

        if (haveOld && haveNew && oldRating != newRating)
        {
            SetPlain(scoreOld, GameStrings.ToPersianDigits(oldRating.ToString()), Color.white);
            SetPlain(scoreArrow, "←", Color.white);
            SetPlain(scoreNew, GameStrings.ToPersianDigits(newRating.ToString()), new Color(1f, 0.86f, 0.22f, 1f));
            var delta = newRating - oldRating;
            var deltaText = delta >= 0
                ? GameStrings.ToPersianDigits($"(+{delta})")
                : GameStrings.ToPersianDigits($"({delta})");
            SetPlain(scoreDelta, deltaText, new Color(0.35f, 0.95f, 0.4f, 1f));
            return;
        }

        SetPlain(scoreOld, string.Empty, Color.white);
        SetPlain(scoreArrow, string.Empty, Color.white);
        SetPlain(scoreNew, haveNew ? GameStrings.ToPersianDigits(newRating.ToString()) : string.Empty,
            new Color(1f, 0.86f, 0.22f, 1f));
        SetPlain(scoreDelta, string.Empty, Color.white);
    }

    private static void SetPlain(TMP_Text tmp, string text, Color color)
    {
        if (tmp == null)
            return;
        tmp.color = color;
        tmp.isRightToLeftText = false;
        if (tmp is RTLTextMeshPro rtl)
        {
            rtl.Farsi = false;
            rtl.ForceFix = false;
            rtl.text = text ?? string.Empty;
        }
        else
        {
            tmp.text = text ?? string.Empty;
        }
    }
}
