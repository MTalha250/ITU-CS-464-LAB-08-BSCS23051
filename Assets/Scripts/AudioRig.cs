using UnityEngine;
using UnityEngine.Audio;

// One object that holds every sound player (music, gameplay effects, UI) and the Audio Mixer.
// It's in both scenes so each can be tested on its own; the first one survives every load and
// any later copy destroys itself, so the music never doubles.
public class AudioRig : MonoBehaviour
{
    static AudioRig instance;
    [SerializeField] AudioMixer mixer;

    public static AudioMixer Mixer => instance ? instance.mixer : null;

    void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // SetFloat on a mixer doesn't stick in Awake, so the saved volumes go on in Start
    void Start() => VolumeSettings.ApplyAll(mixer);
}
