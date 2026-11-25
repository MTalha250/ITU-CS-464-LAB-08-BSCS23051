using UnityEngine;

// 08 Pause, a modal over the level. Lecture 08's pause: time stops while it's open.
public class PauseScreen : UIScreen
{
    [SerializeField] UIScreen gameplay, settings;

    public override void OnShow()
    {
        Time.timeScale = 0f;
        AudioListener.pause = true;
    }

    public override void OnHide(bool closed)
    {
        if (!closed) return;                // Settings opened on top: stay paused
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    public void Resume() => UIRouter.Instance.Back();
    public void Restart() => UIRouter.Instance.Replace(gameplay);
    public void OpenSettings() => UIRouter.Instance.Go(settings);
    public void Quit() => ResultsScreen.ToMenu("Title", "MainMenu");
}
