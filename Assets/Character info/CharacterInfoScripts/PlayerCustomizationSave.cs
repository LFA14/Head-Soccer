using UnityEngine;

public static class PlayerCustomizationSave
{
    private static readonly string[] CharacterIds =
    {
        "ArgentinaCharacter",
        "Character",
        "EgyptCharacter",
        "SaudiCharacter"
    };

    private static readonly string[][] Aliases =
    {
        new[] { "argentinacharacter", "argentinaplayer", "argentina", "leo" },
        new[] { "character", "brazilplayer", "brazil", "ronaldinho" },
        new[] { "egyptcharacter", "egyptplayer", "egypt", "faroun" },
        new[] { "saudicharacter", "saudiplayer", "saudi", "turki" }
    };

    private static readonly Color[] Colors =
    {
        new Color(1f, 1f, 1f, 1f),
        new Color(0.95f, 0.08f, 0.06f, 1f),
        new Color(1f, 0.75f, 0.05f, 1f),
        new Color(0.08f, 0.45f, 1f, 1f),
        new Color(0.05f, 0.75f, 0.28f, 1f),
        new Color(0.5f, 0.15f, 0.95f, 1f),
        new Color(0.05f, 0.05f, 0.05f, 1f)
    };

    public static int CharacterCount
    {
        get { return CharacterIds.Length; }
    }

    public static int ColorCount
    {
        get { return Colors.Length; }
    }

    public static string GetCharacterId(int index)
    {
        return CharacterIds[Mathf.Clamp(index, 0, CharacterIds.Length - 1)];
    }

    public static Color GetColor(int index)
    {
        return Colors[Mathf.Clamp(index, 0, Colors.Length - 1)];
    }

    public static string NormalizeCharacterId(string characterId)
    {
        string key = NormalizeKey(characterId);

        for (int i = 0; i < Aliases.Length; i++)
        {
            for (int j = 0; j < Aliases[i].Length; j++)
            {
                if (key == Aliases[i][j])
                    return CharacterIds[i];
            }
        }

        return string.IsNullOrWhiteSpace(characterId) ? CharacterIds[0] : characterId.Trim();
    }

    public static int GetShoeColorIndex(string characterId)
    {
        return GetIndex(characterId, "shoes");
    }

    public static int GetTieColorIndex(string characterId)
    {
        return GetIndex(characterId, "tie");
    }

    public static void SetShoeColorIndex(string characterId, int index)
    {
        SetIndex(characterId, "shoes", index);
    }

    public static void SetTieColorIndex(string characterId, int index)
    {
        SetIndex(characterId, "tie", index);
    }

    public static Color GetShoeColor(string characterId)
    {
        return GetColor(GetShoeColorIndex(characterId));
    }

    public static Color GetTieColor(string characterId)
    {
        return GetColor(GetTieColorIndex(characterId));
    }

    private static int GetIndex(string characterId, string partId)
    {
        return Mathf.Clamp(PlayerPrefs.GetInt(GetKey(characterId, partId), 0), 0, Colors.Length - 1);
    }

    private static void SetIndex(string characterId, string partId, int index)
    {
        PlayerPrefs.SetInt(GetKey(characterId, partId), Wrap(index));
        PlayerPrefs.Save();
    }

    private static string GetKey(string characterId, string partId)
    {
        return "PlayerCustomization_" + NormalizeCharacterId(characterId) + "_" + partId;
    }

    private static int Wrap(int index)
    {
        return (index % Colors.Length + Colors.Length) % Colors.Length;
    }

    private static string NormalizeKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return value.Trim()
            .Replace("(Clone)", string.Empty)
            .Replace(" ", string.Empty)
            .ToLowerInvariant();
    }
}
