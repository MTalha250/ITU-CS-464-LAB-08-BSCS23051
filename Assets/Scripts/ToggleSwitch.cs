using TMPro;
using UnityEngine;
using UnityEngine.UI;

// An on/off switch. The knob slides (InOutCubic), the track changes colour,
// and the label says ON or OFF, so it still reads without colour.
[RequireComponent(typeof(Toggle))]
public class ToggleSwitch : MonoBehaviour
{
    [SerializeField] RectTransform knob;
    [SerializeField] Image track;
    [SerializeField] TMP_Text label;
    [SerializeField] Color onColor = Color.cyan, offColor = Color.gray;
    [SerializeField] float travel = 48f;
    Toggle toggle;
    Coroutine running;

    Toggle T => toggle ? toggle : toggle = GetComponent<Toggle>();

    void Awake() => T.onValueChanged.AddListener(on =>
    {
        UiAudio.Play(UiSound.Press);
        Show(on, true);
    });
    void OnEnable() => Show(T.isOn, false);
    public void Sync() => Show(T.isOn, false);

    void Show(bool on, bool animate)
    {
        float from = knob.anchoredPosition.x, to = on ? travel : -travel;
        Color c0 = track.color, c1 = on ? onColor : offColor;
        label.text = on ? "ON" : "OFF";
        if (running != null) StopCoroutine(running);
        if (!animate || !isActiveAndEnabled)
        {
            knob.anchoredPosition = new Vector2(to, 0f);
            track.color = c1;
            return;
        }
        running = Tween.Run(this, 0.2f, Ease.InOutCubic, p =>
        {
            knob.anchoredPosition = new Vector2(Mathf.Lerp(from, to, p), 0f);
            track.color = Color.Lerp(c0, c1, p);
        });
    }
}
