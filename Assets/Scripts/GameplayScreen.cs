using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = UnityEngine.Random;

// 07 Gameplay. Steer the lantern with A/D, the arrows, or by touching the river. Catch sparks,
// dodge the rocks (each one costs a light, 3 lights per run), and reach the end of the river.
// This is the publisher: it runs the rules and raises C# events. It doesn't play sounds, shake
// the screen or update the HUD itself; the listener components do that.
public class GameplayScreen : UIScreen
{
    [SerializeField] RectTransform field, lantern, sparkTemplate, rockTemplate;
    [SerializeField] Image progress, lanternBody;
    [SerializeField] TMP_Text levelText;
    [SerializeField] UIScreen pause, results;
    [SerializeField] float length = 15f, sparkEvery = 0.6f, rockEvery = 1.5f, fallSpeed = 620f, steerSpeed = 1100f;
    [SerializeField] int maxLights = 3;
    [SerializeField] float iFrames = 1f;

    public event Action GameStarted;
    public event Action<int> Collected;              // sparks caught so far
    public event Action<int, int> Damaged;           // lights left, max lights
    public event Action Died;                        // the last light went out
    public event Action Won;                         // reached the end of the river

    public int MaxLights => maxLights;

    readonly List<RectTransform> sparkList = new List<RectTransform>(), rockList = new List<RectTransform>();
    InputAction move;
    float elapsed, nextSpark, nextRock, safeUntil;
    int sparks, lights;
    bool running;

    public override void OnShow()
    {
        Time.timeScale = 1f;
        if (running) return;              // back from Pause: carry on where we were
        running = true;
        move ??= InputSystem.actions.FindAction("Player/Move");
        Clear(sparkList);
        Clear(rockList);
        elapsed = 0f;
        nextSpark = 0.5f;
        nextRock = 1.2f;
        sparks = 0;
        lights = maxLights;
        safeUntil = 0f;
        progress.fillAmount = 0f;
        levelText.text = GameData.LevelTitle(GameData.Level);
        lanternBody.color = GameData.LanternColors[GameData.Equipped];
        lantern.anchoredPosition = new Vector2(0f, lantern.anchoredPosition.y);
        GameStarted?.Invoke();
    }

    public override void OnHide(bool closed)
    {
        if (closed) running = false;
    }

    public override void OnBackPressed() => UIRouter.Instance.Go(pause);
    public void Pause() => UIRouter.Instance.Go(pause);

    void Update()
    {
        if (!running || UIRouter.Instance.Current != this || UIRouter.Instance.Busy) return;
        float dt = Time.deltaTime;
        elapsed += dt;
        progress.fillAmount = elapsed / length;
        Steer(dt);
        lanternBody.enabled = Time.time >= safeUntil || Mathf.Repeat(Time.time, 0.2f) < 0.1f;   // blink while safe

        if ((nextSpark -= dt) <= 0f) { nextSpark = sparkEvery; Spawn(sparkTemplate, sparkList); }
        if ((nextRock -= dt) <= 0f) { nextRock = rockEvery; Spawn(rockTemplate, rockList); }

        for (int i = sparkList.Count - 1; i >= 0; i--)
            if (Move(sparkList, i, dt, 110f))
            {
                sparks++;
                Collected?.Invoke(sparks);
            }

        for (int i = rockList.Count - 1; i >= 0; i--)
            if (Move(rockList, i, dt, 90f) && Time.time >= safeUntil)
            {
                lights--;
                safeUntil = Time.time + iFrames;
                Damaged?.Invoke(lights, maxLights);
                if (lights <= 0) { End(false); return; }
            }

        if (elapsed >= length) End(true);
    }

    void Spawn(RectTransform template, List<RectTransform> list)
    {
        var s = Instantiate(template, field);
        s.gameObject.SetActive(true);
        s.anchoredPosition = new Vector2(Random.Range(-380f, 380f), field.rect.height * 0.5f + 60f);
        list.Add(s);
    }

    // moves one falling thing; true if it touched the lantern (and is gone)
    bool Move(List<RectTransform> list, int i, float dt, float reach)
    {
        var s = list[i];
        s.anchoredPosition += Vector2.down * fallSpeed * dt;
        Vector2 d = s.anchoredPosition - lantern.anchoredPosition;
        bool hit = Mathf.Abs(d.x) < reach && Mathf.Abs(d.y) < reach;
        if (hit || s.anchoredPosition.y < -field.rect.height * 0.5f - 60f)
        {
            Destroy(s.gameObject);
            list.RemoveAt(i);
        }
        return hit;
    }

    void Clear(List<RectTransform> list)
    {
        foreach (var s in list) Destroy(s.gameObject);
        list.Clear();
    }

    void End(bool won)
    {
        running = false;
        lanternBody.enabled = true;
        GameData.Finish(sparks, won);
        if (won) Won?.Invoke();
        else Died?.Invoke();
        UIRouter.Instance.Replace(results);
    }

    void Steer(float dt)
    {
        float x = lantern.anchoredPosition.x;
        float input = move != null ? move.ReadValue<Vector2>().x : 0f;
        x += input * steerSpeed * dt;
        var pointer = Pointer.current;
        if (pointer != null && pointer.press.isPressed &&
            RectTransformUtility.ScreenPointToLocalPointInRectangle(field, pointer.position.ReadValue(), GetComponentInParent<Canvas>().worldCamera, out Vector2 local))
            x = Mathf.MoveTowards(x, local.x, steerSpeed * dt);
        lantern.anchoredPosition = new Vector2(Mathf.Clamp(x, -420f, 420f), lantern.anchoredPosition.y);
    }
}
