using UnityEngine;

// In the menu scene: ask for the calm track. Coming back from a level, this crossfades into it.
public class MenuMusic : MonoBehaviour
{
    [SerializeField] AudioClip calmTrack;

    void Start() => MusicPlayer.Instance.Play(calmTrack);
}
