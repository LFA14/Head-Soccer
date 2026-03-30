using UnityEngine;

public static class CharacterUpgradeSave
{
    public static int GetLevel(string characterId, string statId)
    {
        return PlayerPrefs.GetInt(characterId + "_" + statId, 1);
    }

    public static void SetLevel(string characterId, string statId, int level)
    {
        PlayerPrefs.SetInt(characterId + "_" + statId, Mathf.Clamp(level, 1, 5));
        PlayerPrefs.Save();
    }

    public static int GetUpgradeCost(int currentLevel)
    {
        if (currentLevel >= 5)
            return 0;

        return currentLevel * 10;
    }

    public static void ResetCharacterStat(string characterId, string statId)
    {
        PlayerPrefs.DeleteKey(characterId + "_" + statId);
        PlayerPrefs.Save();
    }
}