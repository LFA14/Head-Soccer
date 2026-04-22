using UnityEngine;

public class LobbyCharacterSelectManager
{
    private readonly Sprite[] portraitSprites;
    private readonly GameObject[] characterPrefabs;
    private readonly int characterCount;

    public LobbyCharacterSelectManager(Sprite[] portraitSprites, GameObject[] characterPrefabs)
    {
        this.portraitSprites = portraitSprites ?? new Sprite[0];
        this.characterPrefabs = characterPrefabs ?? new GameObject[0];

        int portraitCount = this.portraitSprites.Length;
        int prefabCount = this.characterPrefabs.Length;

        if (portraitCount == 0)
        {
            characterCount = 0;
        }
        else if (prefabCount == 0)
        {
            characterCount = portraitCount;
        }
        else
        {
            characterCount = Mathf.Min(portraitCount, prefabCount);
        }
    }

    public int CharacterCount => characterCount;
    public int LocalCharacterIndex { get; private set; }
    public int OpponentCharacterIndex { get; private set; } = -1;

    public bool HasCharacters => characterCount > 0;

    public Sprite GetLocalPortrait()
    {
        return GetPortrait(LocalCharacterIndex);
    }

    public Sprite GetOpponentPortrait()
    {
        return GetPortrait(OpponentCharacterIndex);
    }

    public GameObject GetLocalPrefab()
    {
        return GetPrefab(LocalCharacterIndex);
    }

    public GameObject GetOpponentPrefab()
    {
        return GetPrefab(OpponentCharacterIndex);
    }

    public void SetLocalIndex(int index)
    {
        LocalCharacterIndex = NormalizeIndex(index);
    }

    public void SetOpponentIndex(int index)
    {
        OpponentCharacterIndex = index < 0 ? -1 : NormalizeIndex(index);
    }

    public int MovePrevious()
    {
        if (!HasCharacters)
        {
            return LocalCharacterIndex;
        }

        LocalCharacterIndex = (LocalCharacterIndex - 1 + characterCount) % characterCount;
        return LocalCharacterIndex;
    }

    public int MoveNext()
    {
        if (!HasCharacters)
        {
            return LocalCharacterIndex;
        }

        LocalCharacterIndex = (LocalCharacterIndex + 1) % characterCount;
        return LocalCharacterIndex;
    }

    public void ClearOpponent()
    {
        OpponentCharacterIndex = -1;
    }

    public Sprite GetPortrait(int index)
    {
        if (index < 0 || index >= portraitSprites.Length)
        {
            return null;
        }

        return portraitSprites[index];
    }

    public GameObject GetPrefab(int index)
    {
        if (index < 0 || index >= characterPrefabs.Length)
        {
            return null;
        }

        return characterPrefabs[index];
    }

    public int PickOpponentIndex(System.Random random)
    {
        if (!HasCharacters)
        {
            OpponentCharacterIndex = -1;
            return OpponentCharacterIndex;
        }

        if (characterCount == 1)
        {
            OpponentCharacterIndex = 0;
            return OpponentCharacterIndex;
        }

        int nextIndex = LocalCharacterIndex;
        while (nextIndex == LocalCharacterIndex)
        {
            nextIndex = random.Next(0, characterCount);
        }

        OpponentCharacterIndex = nextIndex;
        return OpponentCharacterIndex;
    }

    public void EnsureOpponentDiffers(System.Random random)
    {
        if (!HasCharacters || OpponentCharacterIndex < 0 || characterCount <= 1)
        {
            return;
        }

        if (OpponentCharacterIndex == LocalCharacterIndex)
        {
            PickOpponentIndex(random);
        }
    }

    private int NormalizeIndex(int index)
    {
        if (!HasCharacters)
        {
            return 0;
        }

        int wrappedIndex = index % characterCount;
        return wrappedIndex < 0 ? wrappedIndex + characterCount : wrappedIndex;
    }
}
