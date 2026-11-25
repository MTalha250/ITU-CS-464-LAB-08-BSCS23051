using UnityEngine;
using UnityEngine.UI;

// Listener 2: a red flash over the river when a rock hits the lantern.
public class HitFlash : MonoBehaviour
{
    [SerializeField] GameplayScreen game;
    [SerializeField] Image overlay;              // full screen, red, alpha 0, Raycast Target off
    [SerializeField] float alpha = 0.4f, fadeSpeed = 1.6f;

    void OnEnable() => game.Damaged += Flash;
    void OnDisable() => game.Damaged -= Flash;

    void Flash(int left, int max) => SetAlpha(alpha);

    void Update()
    {
        if (overlay.color.a > 0f) SetAlpha(Mathf.MoveTowards(overlay.color.a, 0f, fadeSpeed * Time.unscaledDeltaTime));
    }

    void SetAlpha(float a)
    {
        Color c = overlay.color;
        c.a = a;
        overlay.color = c;
    }
}
