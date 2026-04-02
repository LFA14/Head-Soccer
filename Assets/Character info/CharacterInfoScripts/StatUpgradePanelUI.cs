using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatUpgradePanelUI : MonoBehaviour
{
    [Header("IDs")]
    public string characterId;
    public string statId;

    [Header("UI")]
    public TMP_Text buttonText;
    public Button upgradeButton;
    public GameObject[] glowGems;

    public void Setup(string newCharacterId, string newStatId)
    {
        characterId = newCharacterId;
        statId = newStatId;
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (string.IsNullOrEmpty(characterId) || string.IsNullOrEmpty(statId))
            return;

        int level = CharacterUpgradeSave.GetLevel(characterId, statId);

        for (int i = 0; i < glowGems.Length; i++)
        {
            if (glowGems[i] != null)
                glowGems[i].SetActive(i < level);
        }

        if (level >= 5)
        {
            if (buttonText != null)
                buttonText.text = "MAX";

            if (upgradeButton != null)
                upgradeButton.interactable = false;

            return;
        }

        int cost = CharacterUpgradeSave.GetUpgradeCost(level);

        if (buttonText != null)
            buttonText.text = cost.ToString();

        if (upgradeButton != null)
        {
            if (CoinManager.Instance != null)
                upgradeButton.interactable = CoinManager.Instance.Coins >= cost;
            else
                upgradeButton.interactable = false;
        }
    }

    public void UpgradeStat()
    {
        if (string.IsNullOrEmpty(characterId) || string.IsNullOrEmpty(statId))
            return;

        if (CoinManager.Instance == null)
            return;

        int level = CharacterUpgradeSave.GetLevel(characterId, statId);

        if (level >= 5)
            return;

        int cost = CharacterUpgradeSave.GetUpgradeCost(level);

        if (CoinManager.Instance.Coins < cost)
            return;

        CoinManager.Instance.AddCoins(-cost);
        CharacterUpgradeSave.SetLevel(characterId, statId, level + 1);

        RefreshUI();
    }
}