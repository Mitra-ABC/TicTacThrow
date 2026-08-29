using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Wires waiting-room copy/share without new GameManager fields.
/// </summary>
public class WaitingChrome : MonoBehaviour
{
    [SerializeField] private Button copyButton;
    [SerializeField] private Button shareButton;

    public void Bind(UnityAction copy, UnityAction share)
    {
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
}
