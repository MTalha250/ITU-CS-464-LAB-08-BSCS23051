using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 06 Settings. Master, Music, Effects and UI volume (mixer groups, in decibels), vibration and
// reduce motion. Every change applies at once and is saved in PlayerPrefs, so it survives a restart.
public class SettingsScreen : UIScreen
{
    [SerializeField] Slider[] volumes;               // Master, Music, SFX, UI: same order as VolumeSettings.Parameters
    [SerializeField] TMP_Text[] values;
    [SerializeField] Toggle vibration, reduceMotion;
    bool wired;

    public override void OnShow()
    {
        if (wired) return;
        wired = true;
        for (int i = 0; i < volumes.Length; i++)
        {
            int k = i;
            string parameter = VolumeSettings.Parameters[k];
            volumes[k].SetValueWithoutNotify(VolumeSettings.Get(parameter));
            ShowValue(k);
            volumes[k].onValueChanged.AddListener(v =>
            {
                VolumeSettings.Set(AudioRig.Mixer, parameter, v);
                ShowValue(k);
                UiAudio.Play(UiSound.Hover);      // a tick, so you hear the UI volume as you move it
            });
        }
        vibration.SetIsOnWithoutNotify(PlayerPrefs.GetInt("vibration", 1) == 1);
        reduceMotion.SetIsOnWithoutNotify(PlayerPrefs.GetInt("reduceMotion", 0) == 1);
        vibration.GetComponent<ToggleSwitch>().Sync();
        reduceMotion.GetComponent<ToggleSwitch>().Sync();
        vibration.onValueChanged.AddListener(v => PlayerPrefs.SetInt("vibration", v ? 1 : 0));
        reduceMotion.onValueChanged.AddListener(v =>
        {
            PlayerPrefs.SetInt("reduceMotion", v ? 1 : 0);
            UIRouter.ReduceMotion = v;          // fades instead of slides, and shorter tweens
            Tween.Speed = v ? 0.6f : 1f;
        });
    }

    void ShowValue(int i)
    {
        float v = volumes[i].value;
        values[i].text = v <= 0.0001f ? "Off" : Mathf.RoundToInt(v * 100f) + "%";
    }
}
