using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Listener 1: keeps the HUD in step with the gameplay events (spark count and the 3 lights).
public class GameplayHud : MonoBehaviour
{
    [SerializeField] GameplayScreen game;
    [SerializeField] TMP_Text sparksText;
    [SerializeField] RectTransform sparksChip;
    [SerializeField] Image[] lightIcons;
    [SerializeField] Color lit = Color.yellow, lost = Color.gray;

    void OnEnable()
    {
        game.GameStarted += OnStarted;
        game.Collected += OnCollected;
        game.Damaged += OnDamaged;
    }

    void OnDisable()
    {
        game.GameStarted -= OnStarted;
        game.Collected -= OnCollected;
        game.Damaged -= OnDamaged;
    }

    void OnStarted()
    {
        sparksText.text = "0";
        foreach (var icon in lightIcons) icon.color = lit;
    }

    void OnCollected(int total)
    {
        sparksText.text = total.ToString();
        Tween.Scale(this, sparksChip, 1.25f, 1f, 0.25f, Ease.OutBack);   // the counter answers the catch
    }

    void OnDamaged(int left, int max)
    {
        for (int i = 0; i < lightIcons.Length; i++) lightIcons[i].color = i < left ? lit : lost;
        if (left < lightIcons.Length) Tween.Scale(this, lightIcons[left].transform, 1.5f, 1f, 0.35f, Ease.OutBack);
    }
}
