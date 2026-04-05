using UnityEngine;
using TMPro;

public class TournamentResultSceneUI : MonoBehaviour
{
    public TMP_Text titleText;
    public TMP_Text subtitleText;
    public TMP_Text rewardText;

    public GameObject qualifiedVisual;
    public GameObject championVisual;
    public GameObject eliminatedVisual;

    private void Start()
    {
        ShowResult();
    }

    void ShowResult()
    {
        if (TournamentResultData.Instance == null)
            return;

        var data = TournamentResultData.Instance;

        if (qualifiedVisual != null) qualifiedVisual.SetActive(false);
        if (championVisual != null) championVisual.SetActive(false);
        if (eliminatedVisual != null) eliminatedVisual.SetActive(false);

        if (data.wonTournament)
        {
            if (titleText != null) titleText.text = "CHAMPION!";
            if (subtitleText != null) subtitleText.text = "You won the tournament.";
            if (rewardText != null) rewardText.text = data.rewardCoins.ToString();

            if (championVisual != null) championVisual.SetActive(true);
        }
        else if (data.qualified)
        {
            if (titleText != null) titleText.text = "QUALIFIED!";
            if (subtitleText != null) subtitleText.text = "You advanced to the final.";
            if (rewardText != null) rewardText.text = data.rewardCoins.ToString();

            if (qualifiedVisual != null) qualifiedVisual.SetActive(true);
        }
        else
        {
            if (titleText != null) titleText.text = "ELIMINATED";
            if (subtitleText != null) subtitleText.text = "You were knocked out.";
            if (rewardText != null) rewardText.text = data.rewardCoins.ToString();

            if (eliminatedVisual != null) eliminatedVisual.SetActive(true);
        }
    }
}