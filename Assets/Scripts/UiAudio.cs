using UnityEngine;

public enum UiSound { None, Hover, Press, Back, Open, Close, Pause, Resume, Success, Error }

// Every UI sound goes through here, on the UI mixer group. No button has its own AudioSource.
// It ignores AudioListener.pause, so the menus still click while the game is paused.
public class UiAudio : MonoBehaviour
{
    public static UiAudio Instance { get; private set; }

    [SerializeField] AudioSource source;
    [SerializeField] AudioClip hover, press, back, open, close, pause, resume, success, error;
    float lastHover;

    void Awake()
    {
        if (Instance != null && Instance != this) return;
        Instance = this;
        source.ignoreListenerPause = true;
    }

    public static void Play(UiSound sound)
    {
        if (Instance != null) Instance.PlaySound(sound);
    }

    void PlaySound(UiSound sound)
    {
        AudioClip clip = sound switch
        {
            UiSound.Hover => hover, UiSound.Press => press, UiSound.Back => back, UiSound.Open => open,
            UiSound.Close => close, UiSound.Pause => pause, UiSound.Resume => resume,
            UiSound.Success => success, UiSound.Error => error, _ => null,
        };
        if (clip == null) return;
        if (sound == UiSound.Hover)
        {
            if (Time.unscaledTime - lastHover < 0.06f) return;   // sweeping across buttons shouldn't buzz
            lastHover = Time.unscaledTime;
        }
        bool repeated = sound == UiSound.Hover || sound == UiSound.Press;
        source.pitch = repeated ? Mathf.Pow(2f, Random.Range(-1f, 1f) / 12f) : 1f;   // ±1 semitone
        source.PlayOneShot(clip, sound == UiSound.Hover ? 0.5f : 1f);
    }
}
