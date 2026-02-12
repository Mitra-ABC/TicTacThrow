using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// Component for displaying a single coin pack item in the store.
/// Attach this to the CoinPackItem prefab.
/// </summary>
public class CoinPackItem : MonoBehaviour
{
    [SerializeField] private TMP_Text packNameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text coinsAmountText;
    [SerializeField] private TMP_Text bonusText;
    [SerializeField] private Button buyButton;

    private CoinPack currentPack;
    private Action<string> onBuyClicked;

    /// <summary>
    /// Sets the coin pack data and buy callback.
    /// </summary>
    public void SetCoinPack(CoinPack pack, Action<string> onBuy)
    {
        currentPack = pack;
        onBuyClicked = onBuy;

        if (packNameText != null)
        {
            packNameText.text = pack.displayName ?? "Coin Pack";
        }

        if (descriptionText != null)
        {
            descriptionText.text = pack.description ?? "";
        }

        if (coinsAmountText != null)
        {
            coinsAmountText.text = pack.coinsAmount.ToString();
        }

        if (bonusText != null)
        {
            if (pack.bonusCoinsAmount > 0)
            {
                bonusText.text = $"+{pack.bonusCoinsAmount} bonus";
                bonusText.gameObject.SetActive(true);
            }
            else
            {
                bonusText.gameObject.SetActive(false);
            }
        }

        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            // Backend may return "code" or use id; fallback to id so we always send a valid identifier
            string code = !string.IsNullOrEmpty(pack.code) ? pack.code : (pack.id > 0 ? pack.id.ToString() : null);
            buyButton.onClick.AddListener(() => onBuyClicked?.Invoke(code));
        }
    }
}
