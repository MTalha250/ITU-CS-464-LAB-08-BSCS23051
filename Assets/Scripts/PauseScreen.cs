using UnityEngine;

// 08 Pause, a modal over the level. Lecture 08's pause: time stops while it's open.
public class PauseScreen : UIScreen
{
    [SerializeField] UIScreen gameplay, settings;

    bool paused;

    // Paused: time stops, gameplay sounds pause (AudioListener.pause), the music dips but keeps going,
    // and the UI sounds still play because the UI player ignores the listener pause.
    public override void OnShow()
    {
        Time.timeScale = 0f;
        AudioListener.pause = true;
        if (paused) return;                 // back from Settings: already paused
        paused = true;
        UiAudio.Play(UiSound.Pause);
        MusicPlayer.Instance.Duck(true);
    }

    public override void OnHide(bool closed)
    {
        if (!closed) return;                // Settings opened on top: stay paused
        paused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
        UiAudio.Play(UiSound.Resume);
        MusicPlayer.Instance.Duck(false);
    }

    public void Resume() => UIRouter.Instance.Back();
    public void Restart() => UIRouter.Instance.Replace(gameplay);
    public void OpenSettings() => UIRouter.Instance.Go(settings);
    public void Quit()
    {
        MusicPlayer.Instance.Duck(false);   // the scene changes without closing the pause menu
        ResultsScreen.ToMenu("Title", "MainMenu");
    }
}
