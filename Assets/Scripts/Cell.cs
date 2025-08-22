using UnityEngine;
using UnityEngine.UI;

public class Cell : MonoBehaviour
{
    public int index;
    public Image markImage;
    public Button button;
    public GameManager gameManager;

    void Awake()
    {
        if (button == null) button = GetComponent<Button>();
        if (markImage == null)
        {
            var t = transform.Find("Mark");
            if (t) markImage = t.GetComponent<Image>();
            if (markImage == null) markImage = GetComponentInChildren<Image>(true);
        }
        if (gameManager == null) gameManager = FindObjectOfType<GameManager>();

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }
    }

    public void Clear()
    {
        if (markImage)
        {
            markImage.sprite = null;
            markImage.enabled = false;
        }
        if (button) button.interactable = true;
    }

    public void OnClick()
    {
        if (gameManager) gameManager.OnCellClicked(index, this);
    }

    public void SetMark(Sprite s)
    {
        Debug.Log($"SetMark on {name} with sprite: {(s ? s.name : "NULL")}");
        if (markImage)
        {
            markImage.sprite = s;
            markImage.enabled = true;
            var c = markImage.color; c.a = 1f; markImage.color = c;
        }
        if (button) button.interactable = false;
    }
}
