using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 03 Level select. Open levels show their number and stars; locked ones show a lock.
// Tapping a locked level shakes it ("no") and says how to open it.
public class LevelSelectScreen : UIScreen
{
    [SerializeField] Button[] tiles;
    [SerializeField] RectTransform ring;
    [SerializeField] TMP_Text levelTitle, levelBest, lanternName, message;
    [SerializeField] Color openColor, lockedColor, starOn, starOff;
    int selected;
    Coroutine messageFade;

    public override void OnShow()
    {
        lanternName.text = GameData.Lanterns[GameData.Equipped];
        message.alpha = 0f;
        for (int i = 0; i < tiles.Length; i++)
        {
            Refresh(i);
            Tween.Scale(this, tiles[i].transform, 0.6f, 1f, 0.35f, Ease.OutBack, 0.1f + 0.025f * i);
        }
        Select(Mathf.Min(GameData.Level, GameData.Unlocked));
    }

    void Refresh(int i)
    {
        bool open = i + 1 <= GameData.Unlocked;
        Transform t = tiles[i].transform;
        t.Find("Number").gameObject.SetActive(open);
        t.Find("Lock").gameObject.SetActive(!open);
        var stars = t.Find("Stars").GetComponent<TMP_Text>();
        stars.gameObject.SetActive(open);
        stars.text = StarText(GameData.Stars[i]);
        tiles[i].targetGraphic.color = open ? openColor : lockedColor;
    }

    string StarText(int n)
    {
        string s = "";
        for (int k = 0; k < 3; k++)   // filled star or outline star: a shape cue, not only colour
            s += k < n ? $"<color=#{ColorUtility.ToHtmlStringRGB(starOn)}>\ue838</color>" : $"<color=#{ColorUtility.ToHtmlStringRGB(starOff)}>\ue83a</color>";
        return s;
    }

    public void Select(int level)
    {
        if (level > GameData.Unlocked)
        {
            Tween.Shake(this, (RectTransform)tiles[level - 1].transform);
            Say($"Finish level {GameData.Unlocked} to open level {level}");
            return;
        }
        selected = level;
        ring.SetParent(tiles[level - 1].transform, false);
        ring.anchoredPosition = Vector2.zero;
        Tween.Scale(this, ring, 1.25f, 1f, 0.3f, Ease.OutBack);   // the ring snaps onto the new choice
        levelTitle.text = GameData.LevelTitle(level);
        levelBest.text = GameData.Stars[level - 1] > 0 ? "Best  " + StarText(GameData.Stars[level - 1]) : "Not played yet";
    }

    void Say(string text)
    {
        message.text = text;
        if (messageFade != null) StopCoroutine(messageFade);
        messageFade = StartCoroutine(SayRoutine());
    }

    System.Collections.IEnumerator SayRoutine()
    {
        yield return Tween.Run(this, 0.2f, Ease.OutQuad, p => message.alpha = p);
        yield return new WaitForSecondsRealtime(1.8f);
        yield return Tween.Run(this, 0.4f, Ease.InCubic, p => message.alpha = 1f - p);
    }

    public void Play()
    {
        GameData.Level = selected;
        SceneLoader.Instance.Load("Gameplay", GameData.LevelTitle(selected));   // the level is its own scene now
    }
}
