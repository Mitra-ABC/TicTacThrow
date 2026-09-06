using UnityEngine;
using UnityEngine.UI;

public static class AvatarCatalog
{
    public const int MinId = 1;
    public const int MaxId = 10;
    public const int DefaultId = 1;

    private static readonly Sprite[] Cache = new Sprite[MaxId + 1];

    public static int Clamp(int id)
    {
        return id >= MinId && id <= MaxId ? id : DefaultId;
    }

    public static Sprite Get(int id)
    {
        id = Clamp(id);
        if (Cache[id] == null)
            Cache[id] = Resources.Load<Sprite>($"Avatars/{id}");
        return Cache[id];
    }

    public static void Apply(Image image, int id)
    {
        if (image == null)
            return;
        var sprite = Get(id);
        if (sprite != null)
            image.sprite = sprite;
        image.enabled = sprite != null;
        image.preserveAspect = true;
        image.color = Color.white;
    }
}
