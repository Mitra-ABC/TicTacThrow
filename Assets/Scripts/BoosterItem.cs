using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// Component for displaying a single booster item in the store.
/// Attach this to the BoosterItem prefab.
/// </summary>
public class BoosterItem : MonoBehaviour
{
    [SerializeField] private TMP_Text boosterNameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private TMP_Text durationText;
    [SerializeField] private Button buyButton;

    private BoosterType currentBooster;
    private Action<string> onBuyClicked;

    /// <summary>
    /// Sets the booster data and buy callback.
    /// </summary>
    public void SetBooster(BoosterType booster, Action<string> onBuy)
    {
        currentBooster = booster;
        onBuyClicked = onBuy;

        if (boosterNameText != null)
        {
            boosterNameText.text = booster.displayName ?? booster.code;
        }

        if (descriptionText != null)
        {
            descriptionText.text = booster.description ?? "";
        }

        if (priceText != null)
        {
            priceText.text = string.Format(GameStrings.BoosterPriceFormat, booster.priceCoins);
        }

        if (durationText != null)
        {
            durationText.text = string.Format(GameStrings.BoosterDurationFormat, booster.durationMinutes);
        }

        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(() => onBuyClicked?.Invoke(booster.code));
        }
    }
}
