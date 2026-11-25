using UnityEngine;

// Listener 4: gameplay sounds for the gameplay events, through the SfxPlayer (PlayOneShot + pitch).
public class GameplaySfx : MonoBehaviour
{
    [SerializeField] GameplayScreen game;
    [SerializeField] AudioClip start, spark, hit, die, win;

    void OnEnable()
    {
        game.GameStarted += OnStarted;
        game.Collected += OnCollected;
        game.Damaged += OnDamaged;
        game.Died += OnDied;
        game.Won += OnWon;
    }

    void OnDisable()
    {
        game.GameStarted -= OnStarted;
        game.Collected -= OnCollected;
        game.Damaged -= OnDamaged;
        game.Died -= OnDied;
        game.Won -= OnWon;
    }

    void OnStarted() => SfxPlayer.Instance.Play(start, 0.8f, false);
    void OnCollected(int total) => SfxPlayer.Instance.Play(spark, 0.8f);          // repeated a lot: pitch varies
    void OnDamaged(int left, int max) => SfxPlayer.Instance.Play(hit);
    void OnDied() => SfxPlayer.Instance.Play(die, 1f, false);
    void OnWon() => SfxPlayer.Instance.Play(win, 1f, false);
}
