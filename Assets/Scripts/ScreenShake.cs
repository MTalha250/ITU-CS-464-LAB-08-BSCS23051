using UnityEngine;

// Listener 3: shakes the river on a hit, harder when the last light goes out.
public class ScreenShake : MonoBehaviour
{
    [SerializeField] GameplayScreen game;
    [SerializeField] RectTransform target;

    void OnEnable()
    {
        game.Damaged += OnDamaged;
        game.Died += OnDied;
    }

    void OnDisable()
    {
        game.Damaged -= OnDamaged;
        game.Died -= OnDied;
    }

    void OnDamaged(int left, int max) => Tween.Shake(this, target, 22f, 0.3f);
    void OnDied() => Tween.Shake(this, target, 40f, 0.5f);
}
