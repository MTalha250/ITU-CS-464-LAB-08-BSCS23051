using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// The loading screen. Loads a scene in the background (LoadSceneAsync) and shows the real progress.
// allowSceneActivation stays off until the bar has reached 100%, so you can't land in a half-loaded level.
public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }
    static bool arriving;                       // the next scene fades the loading screen away

    [SerializeField] CanvasGroup panel;
    [SerializeField] Image bar;
    [SerializeField] TMP_Text percent, label;
    bool loading;

    void Awake()
    {
        Instance = this;
        panel.gameObject.SetActive(arriving);
        panel.alpha = arriving ? 1f : 0f;
        panel.blocksRaycasts = arriving;
    }

    IEnumerator Start()
    {
        if (!arriving) yield break;
        arriving = false;
        Show(1f);
        yield return Tween.Fade(this, panel, 1f, 0f, 0.35f, Ease.OutQuad);
        panel.blocksRaycasts = false;
        panel.gameObject.SetActive(false);
    }

    public void Load(string scene, string text)
    {
        if (loading) return;
        loading = true;
        StartCoroutine(LoadRoutine(scene, text));
    }

    IEnumerator LoadRoutine(string scene, string text)
    {
        Time.timeScale = 1f;                    // these all survive a scene load, so reset them first
        AudioListener.pause = false;
        label.text = text;
        Show(0f);
        panel.gameObject.SetActive(true);
        panel.blocksRaycasts = true;            // nothing behind it can be tapped while loading
        yield return Tween.Fade(this, panel, 0f, 1f, 0.25f, Ease.OutQuad);

        AsyncOperation op = SceneManager.LoadSceneAsync(scene);
        op.allowSceneActivation = false;
        while (op.progress < 0.9f)              // Unity reports 0 to 0.9 while loading
        {
            Show(op.progress / 0.9f);
            yield return null;
        }
        Show(1f);
        yield return new WaitForSecondsRealtime(0.3f);
        arriving = true;
        op.allowSceneActivation = true;
    }

    void Show(float p)
    {
        bar.fillAmount = p;
        percent.text = Mathf.RoundToInt(p * 100f) + "%";
    }
}
