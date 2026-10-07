using UnityEngine;

/// <summary>
/// Applies the shared page header when a lobby subpage turns on.
/// </summary>
public class SubpageChrome : MonoBehaviour
{
    private void OnEnable()
    {
        PageHeader.ApplyAll();
    }
}
