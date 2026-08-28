using System;
using System.Collections;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays a booster: either Buy button or "Time left" when already active.
/// </summary>
public class BoosterItem : MonoBehaviour
{
    [SerializeField] private TMP_Text boosterNameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private TMP_Text durationText;
    [SerializeField] private TMP_Text timeRemainingText; // وقتی بوستر فعاله نشون داده میشه
    [SerializeField] private Button buyButton;
    [SerializeField] private GameObject buyButtonContainer; // اختیاری: برای مخفی کردن کل دکمه

    private BoosterType currentBooster;
    private Action<string> onBuyClicked;
    private Coroutine countdownCoroutine;

    /// <summary>
    /// booster: نوع بوستر، active: اگر کاربر این بوستر رو خریده و فعاله (با expiresAt)، onBuy: کلیک خرید.
    /// </summary>
    public void SetBooster(BoosterType booster, BoosterInfo active, Action<string> onBuy)
    {
        if (countdownCoroutine != null) { StopCoroutine(countdownCoroutine); countdownCoroutine = null; }
        currentBooster = booster;
        onBuyClicked = onBuy;

        if (boosterNameText != null)
        {
            PersianUi.SetText(boosterNameText, booster.displayName);
        }
        if (descriptionText != null)
        {
            PersianUi.SetText(descriptionText, booster.description);
        }

        bool isActive = active != null && !string.IsNullOrEmpty(active.expiresAt);
        if (timeRemainingText != null)
        {
            PersianUi.Style(timeRemainingText);
            timeRemainingText.gameObject.SetActive(isActive);
            if (isActive)
                countdownCoroutine = StartCoroutine(UpdateTimeRemaining(active.expiresAt, timeRemainingText));
        }
        if (buyButton != null)
        {
            buyButton.gameObject.SetActive(!isActive);
            buyButton.onClick.RemoveAllListeners();
            var buyLabel = buyButton.GetComponentInChildren<TMP_Text>(true);
            if (buyLabel != null)
            {
                PersianUi.SetText(buyLabel, GameStrings.BuyButton);
            }
            if (!isActive)
                buyButton.onClick.AddListener(() => onBuyClicked?.Invoke(booster.code));
        }
        if (buyButtonContainer != null)
            buyButtonContainer.SetActive(!isActive);
        if (priceText != null)
        {
            priceText.gameObject.SetActive(!isActive);
            if (!isActive)
            {
                PersianUi.SetText(priceText, string.Format(GameStrings.BoosterPriceFormat, booster.priceCoins));
            }
        }
        if (durationText != null)
        {
            durationText.gameObject.SetActive(!isActive);
            if (!isActive)
            {
                PersianUi.SetText(durationText, string.Format(GameStrings.BoosterDurationFormat, booster.durationMinutes));
            }
        }
    }

    private IEnumerator UpdateTimeRemaining(string expiresAtIso, TMP_Text label)
    {
        if (!DateTime.TryParse(expiresAtIso, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime target))
        {
            PersianUi.SetText(label, GameStrings.BoosterExpired);
            yield break;
        }
        var wait = new WaitForSeconds(1f);
        while (label != null)
        {
            var remaining = target - DateTime.UtcNow;
            if (remaining <= TimeSpan.Zero)
            {
                PersianUi.SetText(label, GameStrings.BoosterExpired);
                yield break;
            }
            PersianUi.SetText(label, GameStrings.FormatRemaining(remaining));
            yield return wait;
        }
    }
}
