using UnityEngine;

// Listener 5: the music follows the game. A run starts: crossfade to the driving river track.
// The run ends (won or lost): crossfade back to the calm track under the results.
public class GameMusic : MonoBehaviour
{
    [SerializeField] GameplayScreen game;
    [SerializeField] AudioClip riverTrack, calmTrack;

    void OnEnable()
    {
        game.GameStarted += OnStarted;
        game.Won += OnEnded;
        game.Died += OnEnded;
    }

    void OnDisable()
    {
        game.GameStarted -= OnStarted;
        game.Won -= OnEnded;
        game.Died -= OnEnded;
    }

    void OnStarted() => MusicPlayer.Instance.Play(riverTrack);
    void OnEnded() => MusicPlayer.Instance.Play(calmTrack);
}
