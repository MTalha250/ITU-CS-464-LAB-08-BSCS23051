using System.Collections;
using UnityEngine;

// Two looping AudioSources on the Music group, so one track can fade out while the next fades in.
// Both ignore AudioListener.pause: the music keeps going (ducked) under the pause menu.
public class MusicPlayer : MonoBehaviour
{
    public static MusicPlayer Instance { get; private set; }

    [SerializeField] AudioSource a, b;
    [SerializeField] float fadeSeconds = 1.5f, duckLevel = 0.35f;
    AudioSource current;
    float duck = 1f;
    Coroutine fading, ducking;

    void Awake()
    {
        if (Instance != null && Instance != this) return;   // a duplicate rig is being destroyed
        Instance = this;
        current = a;
        a.ignoreListenerPause = b.ignoreListenerPause = true;
    }

    public void Play(AudioClip clip)
    {
        if (current.clip == clip && current.isPlaying) return;
        AudioSource from = current, to = current == a ? b : a;
        to.clip = clip;
        to.volume = 0f;
        to.Play();
        if (fading != null) StopCoroutine(fading);
        fading = StartCoroutine(Crossfade(from, to));
        current = to;
    }

    IEnumerator Crossfade(AudioSource from, AudioSource to)
    {
        float start = from.volume;
        for (float t = 0f; t < fadeSeconds; t += Time.unscaledDeltaTime)   // unscaled: works while paused
        {
            float p = t / fadeSeconds;
            from.volume = Mathf.Lerp(start, 0f, p);
            to.volume = p * duck;
            yield return null;
        }
        from.Stop();
        to.volume = duck;
        fading = null;
    }

    // Pause dips the music to 35% instead of stopping it, so the menu doesn't go dead quiet
    public void Duck(bool on)
    {
        if (ducking != null) StopCoroutine(ducking);
        ducking = StartCoroutine(DuckTo(on ? duckLevel : 1f));
    }

    IEnumerator DuckTo(float target)
    {
        float start = duck;
        for (float t = 0f; t < 0.3f; t += Time.unscaledDeltaTime)
        {
            duck = Mathf.Lerp(start, target, t / 0.3f);
            if (fading == null) current.volume = duck;
            yield return null;
        }
        duck = target;
        if (fading == null) current.volume = duck;
    }

    public AudioClip Playing => current ? current.clip : null;
}
