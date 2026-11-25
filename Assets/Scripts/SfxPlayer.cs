using UnityEngine;

// Gameplay sound effects (Lecture 09): a few AudioSources on the SFX group, PlayOneShot so short
// sounds can overlap, and a small random pitch so a repeated sound doesn't sound copy-pasted.
// These do NOT ignore AudioListener.pause, so gameplay sounds freeze with the game.
public class SfxPlayer : MonoBehaviour
{
    public static SfxPlayer Instance { get; private set; }

    [SerializeField] AudioSource[] sources;
    [SerializeField] float pitchRange = 2f;      // ± semitones
    int next;

    void Awake()
    {
        if (Instance != null && Instance != this) return;
        Instance = this;
    }

    public void Play(AudioClip clip, float volume = 1f, bool vary = true)
    {
        AudioSource source = sources[next];
        next = (next + 1) % sources.Length;
        source.pitch = vary ? Mathf.Pow(2f, Random.Range(-pitchRange, pitchRange) / 12f) : 1f;
        source.PlayOneShot(clip, volume);
    }
}
