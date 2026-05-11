using UnityEngine;

public static class PlayerCustomizationApplier
{
    private static readonly string[] ShoeNames = { "boot", "boots", "shoe", "shoes" };
    private static readonly string[] TieNames = { "collar", "tie", "band" };

    public static void ApplyToPlayer(GameObject playerRoot)
    {
        if (playerRoot == null)
            return;

        ApplyToPlayer(playerRoot, playerRoot.name);
    }

    public static void ApplyToPlayer(GameObject playerRoot, string characterId)
    {
        if (playerRoot == null)
            return;

        string normalizedId = PlayerCustomizationSave.NormalizeCharacterId(characterId);
        ApplyColor(playerRoot, ShoeNames, PlayerCustomizationSave.GetShoeColor(normalizedId));
        ApplyColor(playerRoot, TieNames, PlayerCustomizationSave.GetTieColor(normalizedId));
    }

    private static void ApplyColor(GameObject root, string[] nameTokens, Color color)
    {
        SpriteRenderer[] renderers = root.GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null || !NameMatches(renderers[i].gameObject.name, nameTokens))
                continue;

            renderers[i].color = color;
        }
    }

    private static bool NameMatches(string objectName, string[] tokens)
    {
        string key = string.IsNullOrWhiteSpace(objectName) ? string.Empty : objectName.ToLowerInvariant();
        for (int i = 0; i < tokens.Length; i++)
        {
            if (key.Contains(tokens[i]))
                return true;
        }

        return false;
    }
}
