using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 04 Lantern loadout. Pick a card to preview it; the big button says what will happen:
// Equip, Equipped (disabled), or Get it in the Shop.
public class LoadoutScreen : UIScreen
{
    [SerializeField] Button[] cards;
    [SerializeField] RectTransform ring, previewLantern;
    [SerializeField] Image previewBody, previewGlow;
    [SerializeField] TMP_Text previewName, coins, actionLabel;
    [SerializeField] Image[] pips;                 // 3 rows of 5: glow, speed, drift
    [SerializeField] Sprite pipOn, pipOff;
    [SerializeField] Button action;
    [SerializeField] Color pipColor, pipOffColor;
    [SerializeField] UIScreen shop;
    int shown;

    public override void OnShow()
    {
        coins.text = GameData.Coins.ToString("N0");
        for (int i = 0; i < cards.Length; i++) Refresh(i);
        Select(GameData.Equipped);
    }

    void Refresh(int i)
    {
        Transform c = cards[i].transform;
        bool owned = GameData.Owned[i], equipped = GameData.Equipped == i;
        c.Find("State/Icon").GetComponent<TMP_Text>().text = equipped ? "\ue86c" : owned ? "\ue5ca" : "\ue897";
        c.Find("State/Label").GetComponent<TMP_Text>().text =
            equipped ? "EQUIPPED" : owned ? "OWNED" : $"{GameData.Price[i]:N0} coins";
        c.Find("Swatch").GetComponent<Image>().color = owned
            ? (Color)GameData.LanternColors[i]
            : Color.Lerp(GameData.LanternColors[i], Color.black, 0.55f);
    }

    public void Select(int i)
    {
        shown = i;
        ring.SetParent(cards[i].transform, false);
        ring.anchoredPosition = Vector2.zero;
        Tween.Scale(this, ring, 1.08f, 1f, 0.25f, Ease.OutBack);

        Color c = GameData.LanternColors[i];
        previewBody.color = c;
        previewGlow.color = new Color(c.r, c.g, c.b, 0.45f);
        previewName.text = GameData.Lanterns[i];
        Tween.Scale(this, previewLantern, 0.85f, 1f, 0.35f, Ease.OutBack);
        for (int k = 0; k < pips.Length; k++)
        {
            bool on = k % 5 < GameData.Stats[i][k / 5];   // filled dot or hollow ring
            pips[k].sprite = on ? pipOn : pipOff;
            pips[k].color = on ? pipColor : pipOffColor;
            if (on) Tween.Scale(this, pips[k].transform, 0.4f, 1f, 0.25f, Ease.OutBack, 0.03f * (k % 5));
        }

        bool owned = GameData.Owned[i], equipped = GameData.Equipped == i;
        action.interactable = !equipped;
        actionLabel.text = equipped ? "EQUIPPED" : owned ? "EQUIP" : "GET IT IN THE SHOP";
    }

    public void Action()
    {
        if (!GameData.Owned[shown])
        {
            UIRouter.Instance.Go(shop);
            return;
        }
        GameData.Equipped = shown;
        UiAudio.Play(UiSound.Success);
        for (int i = 0; i < cards.Length; i++) Refresh(i);
        Tween.Scale(this, cards[shown].transform.Find("State"), 1.35f, 1f, 0.35f, Ease.OutBack);   // the badge stamps on
        action.interactable = false;
        actionLabel.text = "EQUIPPED";
    }
}
