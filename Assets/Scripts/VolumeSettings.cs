using UnityEngine;
using UnityEngine.Audio;

// The four exposed mixer volumes. Sliders run 0 to 1; the mixer wants decibels, and hearing is
// logarithmic, so the value goes through Log10 x 20: 1 -> 0 dB, 0.5 -> -6 dB, 0.1 -> -20 dB.
// 0 is clamped to 0.0001 (-80 dB, silent) because Log10(0) is minus infinity.
public static class VolumeSettings
{
    public static readonly string[] Parameters = { "MasterVolume", "MusicVolume", "SfxVolume", "UiVolume" };
    const float Min = 0.0001f;

    public static float Get(string parameter) => PlayerPrefs.GetFloat(parameter, 1f);   // default: full volume

    public static float ToDecibels(float value) => Mathf.Log10(Mathf.Clamp(value, Min, 1f)) * 20f;

    public static void Set(AudioMixer mixer, string parameter, float value)
    {
        mixer.SetFloat(parameter, ToDecibels(value));
        PlayerPrefs.SetFloat(parameter, value);
        PlayerPrefs.Save();
    }

    public static void ApplyAll(AudioMixer mixer)
    {
        foreach (string p in Parameters) mixer.SetFloat(p, ToDecibels(Get(p)));
    }
}
