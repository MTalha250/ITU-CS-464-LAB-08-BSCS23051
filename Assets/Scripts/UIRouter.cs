using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

// One machine for every screen (Lecture 08's UIManager), as a back stack.
// Go = deeper (slides in from the right), Back = up a level (slides out to the right),
// a modal pops over the page under it, Replace cross-fades, Home unwinds the stack.
public class UIRouter : MonoBehaviour
{
    public static UIRouter Instance { get; private set; }
    public static bool ReduceMotion;               // set by Settings: fades instead of slides
    public static string[] StartPath;              // set before loading a scene: the stack to open it on

    [SerializeField] UIScreen first;
    [SerializeField] float pageTime = 0.35f, modalTime = 0.3f, fadeTime = 0.3f;

    readonly List<UIScreen> stack = new List<UIScreen>();
    InputAction cancel;
    bool busy;

    public UIScreen Current => stack.Count > 0 ? stack[stack.Count - 1] : null;
    public bool Busy => busy;

    void Awake()
    {
        Instance = this;
        foreach (var s in GetComponentsInChildren<UIScreen>(true)) s.gameObject.SetActive(false);
        cancel = InputSystem.actions.FindAction("UI/Cancel");   // Esc, or the phone's back button
        ReduceMotion = PlayerPrefs.GetInt("reduceMotion", 0) == 1;
        Tween.Speed = ReduceMotion ? 0.6f : 1f;
    }

    void Start()
    {
        // coming back from the Gameplay scene: rebuild the back stack, e.g. Title > MainMenu > LevelSelect
        if (StartPath != null)
            foreach (string name in StartPath)
            {
                var t = transform.Find(name);
                if (t) stack.Add(t.GetComponent<UIScreen>());
            }
        StartPath = null;
        if (stack.Count == 0) stack.Add(first);
        UIScreen top = Current;
        Open(top, Vector2.zero, 1f);
        top.OnShow();
        Finish();
    }

    void Update()
    {
        if (!busy && cancel != null && cancel.WasPressedThisFrame()) Current.OnBackPressed();
    }

    public void Go(UIScreen next)
    {
        if (busy || next == Current) return;
        StartCoroutine(Push(next));
    }

    public void Back()
    {
        if (busy || stack.Count < 2) return;
        StartCoroutine(Pop(stack.Count - 2));
    }

    public void Home(UIScreen target)
    {
        if (busy) return;
        int i = stack.IndexOf(target);
        if (i >= 0) StartCoroutine(Pop(i));
        else StartCoroutine(ReplaceAll(target));
    }

    public void Replace(UIScreen next)
    {
        if (busy) return;
        StartCoroutine(CrossFade(next));
    }

    // ---------- transitions ----------

    float Width => ((RectTransform)transform).rect.width;

    IEnumerator Push(UIScreen next)
    {
        Begin();
        UIScreen from = Current;
        from.OnHide(false);
        stack.Add(next);
        if (next.ScreenKind == UIScreen.Kind.Page) UiAudio.Play(UiSound.Open);   // modals play their own
        if (next.ScreenKind == UIScreen.Kind.Modal)
            yield return PopIn(next);
        else if (ReduceMotion)
            yield return Fade(from, next);
        else
        {
            // forward: the new page comes in from the right, the old one drifts left and fades
            Open(next, new Vector2(Width, 0), 1f);
            next.OnShow();
            yield return Tween.Run(this, pageTime, Ease.OutCubic, p =>
            {
                next.Rect.anchoredPosition = new Vector2(Mathf.LerpUnclamped(Width, 0f, p), 0f);
                from.Rect.anchoredPosition = new Vector2(Mathf.LerpUnclamped(0f, -0.3f * Width, p), 0f);
                from.Group.alpha = 1f - p;
            });
            HideBelow();
        }
        Finish();
    }

    IEnumerator Pop(int toIndex)
    {
        Begin();
        UIScreen from = Current;
        UIScreen to = stack[toIndex];
        var popped = stack.GetRange(toIndex + 1, stack.Count - toIndex - 1);
        if (from.ScreenKind == UIScreen.Kind.Page) UiAudio.Play(UiSound.Back);
        foreach (var s in popped) s.OnHide(true);
        stack.RemoveRange(toIndex + 1, popped.Count);

        if (popped.Count == 1 && from.ScreenKind == UIScreen.Kind.Modal && Visible(to))
        {
            // closing a modal: it shrinks a little and fades; the page under it was there all along
            yield return Tween.Run(this, modalTime * 0.6f, Ease.InCubic, p =>
            {
                from.Group.alpha = 1f - p;
                if (from.Card) from.Card.localScale = Vector3.one * Mathf.Lerp(1f, 0.92f, p);
            });
            to.OnShow();
        }
        else
        {
            ShowFrom(toIndex);
            foreach (var s in popped) s.transform.SetAsLastSibling();
            to.OnShow();
            if (popped.Count == 1 && !ReduceMotion)
                // back: the reverse of Go, so the direction tells you where you went
                yield return Tween.Run(this, pageTime, Ease.OutCubic, p =>
                {
                    from.Rect.anchoredPosition = new Vector2(Mathf.LerpUnclamped(0f, Width, p), 0f);
                    to.Rect.anchoredPosition = new Vector2(Mathf.LerpUnclamped(-0.3f * Width, 0f, p), 0f);
                    to.Group.alpha = p;
                });
            else
                // jumping back several levels (Home, Quit) cross-fades instead
                yield return Tween.Run(this, fadeTime, Ease.InOutSine, p =>
                {
                    foreach (var s in popped) s.Group.alpha = 1f - p;
                    to.Group.alpha = p;
                });
        }
        foreach (var s in popped) s.gameObject.SetActive(false);
        Finish();
    }

    IEnumerator CrossFade(UIScreen next)
    {
        Begin();
        UIScreen from = Current;
        from.OnHide(true);
        stack.RemoveAt(stack.Count - 1);
        // drop any modal still showing under it (Pause under Results, for example)
        while (Current != null && Current.ScreenKind == UIScreen.Kind.Modal)
        {
            Current.OnHide(true);
            if (Current != from) Current.gameObject.SetActive(false);
            stack.RemoveAt(stack.Count - 1);
        }
        if (Current == next)                       // Restart: the same page again, from the start
        {
            next.OnHide(true);
            stack.RemoveAt(stack.Count - 1);
        }
        stack.Add(next);
        yield return Fade(from, next);
        Finish();
    }

    IEnumerator ReplaceAll(UIScreen next)
    {
        Begin();
        UIScreen from = Current;
        foreach (var s in stack) s.OnHide(true);
        stack.Clear();
        stack.Add(next);
        yield return Fade(from, next);
        Finish();
    }

    // a modal: the dim fades in while the card pops with OutBack, so it reads as "on top"
    IEnumerator PopIn(UIScreen next)
    {
        Open(next, Vector2.zero, 0f);
        next.OnShow();
        yield return Tween.Run(this, modalTime, Ease.OutBack, p =>
        {
            next.Group.alpha = Mathf.Clamp01(p * 2f);
            if (next.Card) next.Card.localScale = Vector3.one * Mathf.LerpUnclamped(0.85f, 1f, p);
        });
    }

    // used for Replace, Home to a screen not in the stack, and Reduce Motion
    IEnumerator Fade(UIScreen from, UIScreen to)
    {
        Open(to, Vector2.zero, 0f);
        to.OnShow();
        yield return Tween.Run(this, fadeTime, Ease.InOutSine, p =>
        {
            from.Group.alpha = 1f - p;
            to.Group.alpha = p;
            to.Rect.localScale = Vector3.one * Mathf.Lerp(1.04f, 1f, p);
        });
        from.gameObject.SetActive(false);
        HideBelow();
    }

    // ---------- helpers ----------

    void Open(UIScreen s, Vector2 pos, float alpha)
    {
        s.gameObject.SetActive(true);
        s.transform.SetAsLastSibling();
        s.Rect.anchoredPosition = pos;
        s.Rect.localScale = Vector3.one;
        s.Group.alpha = alpha;
        if (s.Card) s.Card.localScale = Vector3.one;
    }

    bool Visible(UIScreen s) => s.gameObject.activeSelf && s.Group.alpha > 0.99f;

    // everything from the top page up stays visible; pages under it are switched off
    void HideBelow()
    {
        int top = stack.FindLastIndex(s => s.ScreenKind == UIScreen.Kind.Page);
        for (int i = 0; i < stack.Count; i++)
            if (i < top) stack[i].gameObject.SetActive(false);
    }

    void ShowFrom(int index)
    {
        int page = index;
        while (page > 0 && stack[page].ScreenKind == UIScreen.Kind.Modal) page--;
        for (int i = page; i <= index; i++)
        {
            var s = stack[i];
            if (!s.gameObject.activeSelf) Open(s, Vector2.zero, 1f);
            s.transform.SetSiblingIndex(s.transform.parent.childCount - 1);
            s.Group.alpha = 1f;
            s.Rect.anchoredPosition = Vector2.zero;
        }
    }

    void Begin()
    {
        busy = true;
        foreach (var s in stack) s.Group.interactable = s.Group.blocksRaycasts = false;
    }

    void Finish()
    {
        busy = false;
        foreach (var s in stack) s.Group.interactable = s.Group.blocksRaycasts = s == Current;
        var current = Current;
        current.Rect.anchoredPosition = Vector2.zero;
        current.Group.alpha = 1f;
        if (EventSystem.current)
            EventSystem.current.SetSelectedGameObject(current.FirstSelected ? current.FirstSelected.gameObject : null);
    }
}
