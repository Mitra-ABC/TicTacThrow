using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Mirrors the join-room input into six digit boxes without new GameManager fields.
/// </summary>
public class JoinRoomChrome : MonoBehaviour
{
    [SerializeField] private TMP_InputField input;
    [SerializeField] private TMP_Text[] digits;

    private bool syncing;
    private Button submitButton;

    private void Awake()
    {
        if (input == null)
            return;
        input.contentType = TMP_InputField.ContentType.Standard;
        input.characterValidation = TMP_InputField.CharacterValidation.None;
        input.keyboardType = TouchScreenKeyboardType.NumberPad;
        input.characterLimit = GameStrings.RoomCodeLength;
        input.onValueChanged.AddListener(_ => Refresh());
        var submit = transform.Find("SubmitJoinButton");
        if (submit == null)
        {
            foreach (var t in GetComponentsInChildren<Transform>(true))
            {
                if (t != null && t.name == "SubmitJoinButton")
                {
                    submit = t;
                    break;
                }
            }
        }

        if (submit != null)
            submitButton = submit.GetComponent<Button>();
        Refresh();
    }

    private void OnEnable()
    {
        if (input != null)
            input.ActivateInputField();
        Refresh();
    }

    public void Refresh()
    {
        if (syncing)
            return;

        var digitsOnly = GameStrings.NormalizeRoomCode(input != null ? input.text : string.Empty);
        if (digitsOnly.Length > GameStrings.RoomCodeLength)
            digitsOnly = digitsOnly.Substring(0, GameStrings.RoomCodeLength);

        if (input != null && input.text != digitsOnly)
        {
            syncing = true;
            input.characterLimit = GameStrings.RoomCodeLength;
            input.text = digitsOnly;
            syncing = false;
        }

        if (digits != null)
        {
            for (var i = 0; i < digits.Length; i++)
            {
                if (digits[i] == null)
                    continue;
                PersianUi.SetText(digits[i], i < digitsOnly.Length
                    ? GameStrings.ToPersianDigits(digitsOnly[i].ToString())
                    : string.Empty);
            }
        }

        if (submitButton != null)
            submitButton.interactable = digitsOnly.Length == GameStrings.RoomCodeLength;
    }
}
