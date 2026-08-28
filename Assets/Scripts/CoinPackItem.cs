using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// Component for displaying a single coin pack item in the store.
/// Attach this to the CoinPackItem prefab.
/// When IAP is enabled, priceString should come from store SDK (IAPManager.RequestSkuPrices).
/// </summary>
public class CoinPackItem : MonoBehaviour
{
    [SerializeField] private TMP_Text packNameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text coinsAmountText;
    [SerializeField] private TMP_Text bonusText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private Button buyButton;

    private CoinPack currentPack;
    private Action<CoinPack> onBuyClicked;

    /// <summary>
    /// Sets the coin pack data, price string (from SDK when IAP enabled, or "—" / empty), and buy callback.
    /// </summary>
    public void SetCoinPack(CoinPack pack, string priceString, Action<CoinPack> onBuy)
    {
        currentPack = pack;
        onBuyClicked = onBuy;

        if (packNameText != null)
        {
            PersianUi.SetText(packNameText, pack.displayName);
        }

        if (descriptionText != null)
        {
            PersianUi.SetText(descriptionText, pack.description);
        }

        if (coinsAmountText != null)
            coinsAmountText.text = pack.coinsAmount.ToString();

        if (bonusText != null)
        {
            if (pack.bonusCoinsAmount > 0)
            {
                PersianUi.SetText(bonusText, string.Format(GameStrings.CoinBonusFormat, pack.bonusCoinsAmount));
                bonusText.gameObject.SetActive(true);
            }
            else
            {
                bonusText.gameObject.SetActive(false);
            }
        }

        if (priceText != null)
        {
            PersianUi.SetText(priceText, string.IsNullOrEmpty(priceString) ? "—" : priceString);
        }

        if (buyButton != null)
        {
            var buyLabel = buyButton.GetComponentInChildren<TMP_Text>(true);
            if (buyLabel != null)
            {
                PersianUi.SetText(buyLabel, GameStrings.BuyButton);
            }
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(() => onBuyClicked?.Invoke(currentPack));
        }
    }

    public void SetPurchasable(bool purchasable)
    {
        if (buyButton != null)
            buyButton.interactable = purchasable;
    }

    /// <summary>
    /// Backward compatibility: set pack and callback only (no price). Use when IAP is disabled.
    /// </summary>
    public void SetCoinPack(CoinPack pack, Action<string> onBuyWithCode)
    {
        SetCoinPack(pack, "—", onBuyWithCode == null ? (Action<CoinPack>)null : p => onBuyWithCode.Invoke(!string.IsNullOrEmpty(p.code) ? p.code : (p.id > 0 ? p.id.ToString() : "")));
    }
}
