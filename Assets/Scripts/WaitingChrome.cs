using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Waiting-room copy/share plus Persian labels that stay readable.
/// </summary>
public class WaitingChrome : MonoBehaviour
{
    [SerializeField] private Button copyButton;
    [SerializeField] private Button shareButton;

    private void Awake()
    {
        ApplyLabels();
    }

    private void OnEnable()
    {
        ApplyLabels();
    }

    public void Bind(UnityAction copy, UnityAction share)
    {
        ApplyLabels();
        if (copyButton != null && copy != null)
        {
            copyButton.onClick.RemoveListener(copy);
            copyButton.onClick.AddListener(copy);
        }

        if (shareButton != null && share != null)
        {
            shareButton.onClick.RemoveListener(share);
            shareButton.onClick.AddListener(share);
        }
    }

    public void ApplyLabels()
    {
        SetLabel("WaitCreatedTitle", GameStrings.RoomCreatedTitle);
        SetLabel("WaitingStatusLabel", GameStrings.WaitingForFriend);
        SetLabel("WaitCodeCaption", GameStrings.YourRoomCode);
        SetLabel("WaitCopyLabel", GameStrings.CopyButton);
        SetLabel("WaitShareLabel", GameStrings.ShareButton);
        SetLabel("CancelWaitingButtonLabel", GameStrings.CancelRoomButton);

        var cancel = FindTmpOn("CancelWaitingButton");
        if (cancel != null)
            PersianUi.SetText(cancel, GameStrings.CancelRoomButton);
    }

    private void SetLabel(string objectName, string value)
    {
        var tmp = FindTmp(objectName);
        if (tmp != null)
            PersianUi.SetText(tmp, value);
    }

    private TMP_Text FindTmp(string objectName)
    {
        foreach (var t in GetComponentsInChildren<Transform>(true))
        {
            if (t != null && t.name == objectName)
                return t.GetComponent<TMP_Text>();
        }

        return null;
    }

    private TMP_Text FindTmpOn(string objectName)
    {
        foreach (var t in GetComponentsInChildren<Transform>(true))
        {
            if (t != null && t.name == objectName)
                return t.GetComponentInChildren<TMP_Text>(true);
        }

        return null;
    }
}
