using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class ColorPuzzle
{
    public string id;
    public bool canFlash;
    public CodeCapturer codeCapturer;
    public float durationDesired;
    public List<PuzzleLampObject> lamps;
}

[Serializable]
public class CustomPositiveFloatRange
{
    [SerializeField, Range(0.01f, 10f)] private float min = 0.5f;
    [SerializeField, Range(0.01f, 10f)] private float max = 1.5f;

    private const float MIN_LIMIT = 0.01f;
    private const float MAX_LIMIT = 10f;

    public float Min
    {
        get => min;
        set
        {
            min = Mathf.Clamp(value, MIN_LIMIT, MAX_LIMIT);
            if (min > max)
                max = min;
        }
    }

    public float Max
    {
        get => max;
        set
        {
            max = Mathf.Clamp(value, MIN_LIMIT, MAX_LIMIT);
            if (max < min)
                min = max;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Garante consistência ao editar no Inspector
        Min = min;
        Max = max;
    }
#endif

    public float GetRandom() => UnityEngine.Random.Range(min, max);
    public bool IsValid() => min >= MIN_LIMIT && max <= MAX_LIMIT && min <= max;
}

    
[System.Serializable]
public class Timer
{
    private float current;
    private float duration;
    private bool active;

    public bool IsActive => active;
    public bool IsDone => !active;

    public void Start(float duration)
    {
        this.duration = duration;
        current = 0f;
        active = true;
    }

    public void Stop() => active = false;

    public bool Tick(float deltaTime)
    {
        if (!active) return false;
        current += deltaTime;
        if (current >= duration)
        {
            active = false;
            return true; // terminou
        }
        return false;
    }
}

