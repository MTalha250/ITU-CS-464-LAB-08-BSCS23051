using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 05 Shop. Buying asks first (a small pop-up over the shop), then the coin count runs down
// and the row stamps OWNED. If you can't afford it the button shakes and says how many you need.
public class ShopScreen : UIScreen
{
    static readonly string[] Names = { "Firefly lantern", "Aurora lantern", "Spark Magnet", "Extra Light" };
    static readonly int[] Prices = { 600, 1500, 200, 350 };
    static readonly string[] Descs = { "Bright and quick", "The brightest lantern", "Pulls in sparks", "+1 light a level" };

    [SerializeField] TMP_Text coins, toast, confirmTitle, confirmBody;
    [SerializeField] Button[] buy;
    [SerializeField] CanvasGroup confirm;
    [SerializeField] RectTransform confirmCard;
    [SerializeField] Color buyColor, ownedColor, inkColor, ownedTextColor;
    int pending = -1;
    Coroutine toastRun;

    bool IsOwned(int i) => i < 2 && GameData.Owned[i + 2];

    public override void OnShow()
    {
        coins.text = GameData.Coins.ToString("N0");
        toast.alpha = 0f;
        Close(false);
        for (int i = 0; i < buy.Length; i++) Refresh(i);
    }

    void Refresh(int i)
    {
        Transform b = buy[i].transform;
        bool owned = IsOwned(i);
        bool afford = GameData.Coins >= Prices[i];
        var icon = b.Find("Icon").GetComponent<TMP_Text>();
        var label = b.Find("Label").GetComponent<TMP_Text>();
        icon.text = owned ? "\ue5ca" : "\ue263";          // a tick, not just a colour change
        label.text = owned ? "OWNED" : Prices[i].ToString("N0");
        icon.color = label.color = owned ? ownedTextColor : inkColor;
        buy[i].targetGraphic.color = owned ? ownedColor : buyColor;
        buy[i].interactable = !owned;
        string note = owned ? "In your lanterns" : afford ? Descs[i] : $"Need {Prices[i] - GameData.Coins:N0} more coins";
        if (i == 2) note += $"  ·  {GameData.Magnets} owned";
        if (i == 3) note += $"  ·  {GameData.ExtraLights} owned";
        b.parent.Find("Note").GetComponent<TMP_Text>().text = note;
    }

    public void Ask(int i)
    {
        if (IsOwned(i)) return;
        if (GameData.Coins < Prices[i])
        {
            Tween.Shake(this, (RectTransform)buy[i].transform);
            UiAudio.Play(UiSound.Error);
            Toast($"You need {Prices[i] - GameData.Coins:N0} more coins");
            return;
        }
        pending = i;
        confirmTitle.text = $"Buy {Names[i]}?";
        confirmBody.text = $"{Prices[i]:N0} coins. You'll have {GameData.Coins - Prices[i]:N0} left.";
        confirm.gameObject.SetActive(true);
        confirm.blocksRaycasts = true;
        UiAudio.Play(UiSound.Open);
        Tween.Run(this, 0.3f, Ease.OutBack, p =>
        {
            confirm.alpha = Mathf.Clamp01(p * 2f);
            confirmCard.localScale = Vector3.one * Mathf.LerpUnclamped(0.85f, 1f, p);
        });
    }

    public void Confirm()
    {
        int i = pending, before = GameData.Coins;
        GameData.Coins -= Prices[i];
        if (i < 2) GameData.Owned[i + 2] = true;
        else if (i == 2) GameData.Magnets++;
        else GameData.ExtraLights++;
        Close(true);
        UiAudio.Play(UiSound.Success);
        // the balance counts down, so you can see where the coins went
        Tween.Run(this, 0.6f, Ease.OutQuad, p => coins.text = Mathf.RoundToInt(Mathf.Lerp(before, GameData.Coins, p)).ToString("N0"));
        for (int k = 0; k < buy.Length; k++) Refresh(k);
        Tween.Scale(this, buy[i].transform, 1.25f, 1f, 0.35f, Ease.OutBack);
        Toast(i < 2 ? $"{Names[i]} added to your lanterns" : $"{Names[i]} added");
    }

    public void Cancel()
    {
        UiAudio.Play(UiSound.Close);
        Close(true);
    }

    void Close(bool animate)
    {
        pending = -1;
        confirm.blocksRaycasts = false;
        if (!animate || !confirm.gameObject.activeSelf)
        {
            confirm.alpha = 0f;
            confirm.gameObject.SetActive(false);
            return;
        }
        Tween.Run(this, 0.18f, Ease.InCubic, p =>
        {
            confirm.alpha = 1f - p;
            confirmCard.localScale = Vector3.one * Mathf.Lerp(1f, 0.92f, p);
            if (p >= 1f) confirm.gameObject.SetActive(false);
        });
    }

    void Toast(string text)
    {
        toast.text = text;
        if (toastRun != null) StopCoroutine(toastRun);
        toastRun = StartCoroutine(ToastRoutine());
    }

    IEnumerator ToastRoutine()
    {
        RectTransform rt = toast.rectTransform;
        Vector2 rest = new Vector2(rt.anchoredPosition.x, 260f);
        yield return Tween.Run(this, 0.3f, Ease.OutCubic, p =>
        {
            toast.alpha = p;
            rt.anchoredPosition = rest + Vector2.down * 40f * (1f - p);
        });
        yield return new WaitForSecondsRealtime(1.8f);
        yield return Tween.Run(this, 0.3f, Ease.InCubic, p => toast.alpha = 1f - p);
    }

    public override void OnBackPressed()
    {
        if (pending >= 0) Cancel();
        else base.OnBackPressed();
    }
}
