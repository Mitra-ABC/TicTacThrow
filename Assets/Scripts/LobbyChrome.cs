using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Wires lobby plus buttons to the store without new GameManager fields.
/// </summary>
public class LobbyChrome : MonoBehaviour
{
    [SerializeField] private Button heartsPlus;
    [SerializeField] private Button coinsPlus;

    public void BindStore(UnityAction handler)
    {
        if (handler == null)
            return;
        if (heartsPlus != null)
        {
            heartsPlus.onClick.RemoveListener(handler);
            heartsPlus.onClick.AddListener(handler);
        }

        if (coinsPlus != null)
        {
            coinsPlus.onClick.RemoveListener(handler);
            coinsPlus.onClick.AddListener(handler);
        }
    }
}
