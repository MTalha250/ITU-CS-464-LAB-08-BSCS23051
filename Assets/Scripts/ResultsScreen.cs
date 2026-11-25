using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 09 Results. The stars you earned pop in one by one (OutBack), then the score counts up,
// so the result arrives in the order it matters.
public class ResultsScreen : UIScreen
{
    [SerializeField] TMP_Text title, levelName, score, sparks, coins;
    [SerializeField] TMP_Text[] stars;
    [SerializeField] Color starOn, starOff;
    [SerializeField] Button next;
    [SerializeField] UIScreen gameplay;

    public override void OnShow()
    {
        int earned = GameData.LastStars;
        title.text = earned > 0 ? "Level complete!" : "So close!";
        levelName.text = GameData.LevelTitle(GameData.Level);
        sparks.text = $"{GameData.LastSparks} sparks";
        coins.text = $"+{GameData.LastCoins} coins";
        next.interactable = earned > 0 && GameData.Level < GameData.LevelCount;

        for (int i = 0; i < stars.Length; i++)
        {
            bool on = i < earned;
            stars[i].text = on ? "\ue838" : "\ue83a";      // filled star or outline
            stars[i].color = on ? starOn : starOff;
            if (on) Tween.Scale(this, stars[i].transform, 0f, 1f, 0.45f, Ease.OutBack, 0.3f + 0.25f * i);
            else stars[i].transform.localScale = Vector3.one;
        }
        int target = GameData.LastScore;
        Tween.Run(this, 0.9f, Ease.OutQuad, p => score.text = Mathf.RoundToInt(target * p).ToString("N0"), 0.3f + 0.25f * earned);
    }

    public void Next()
    {
        GameData.Level++;
        UIRouter.Instance.Replace(gameplay);
    }

    public void Retry() => UIRouter.Instance.Replace(gameplay);
    // the menus are in the other scene: load it, opening on the right screen with Back still working
    public void Levels() => ToMenu("Title", "MainMenu", "LevelSelect");
    public void Home() => ToMenu("Title", "MainMenu");

    public static void ToMenu(params string[] path)
    {
        UIRouter.StartPath = path;
        SceneLoader.Instance.Load("Riverlight", "Back to the menu");
    }
    public override void OnBackPressed() => Levels();
}
