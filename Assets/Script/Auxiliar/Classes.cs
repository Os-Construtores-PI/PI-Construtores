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

[System.Serializable]
public class Scanner<TInput, TOutput>
{
    private readonly Func<TInput, TOutput> scanFunc;
    private readonly float interval;

    private Timer timer = new Timer();

    public Scanner(float interval, Func<TInput, TOutput> scanFunc)
    {
        this.interval = interval;
        this.scanFunc = scanFunc;
        timer.Start(interval);
    }

    /// <summary>
    /// Executa o scan somente quando o tempo expira.
    /// Caso contrário, retorna default(TOutput).
    /// </summary>
    public (bool executed, TOutput result) Scan(float deltaTime, TInput input)
    {
        if (timer.Tick(deltaTime))
        {
            timer.Start(interval);
            return (true, scanFunc(input));
        }

        return (false, default);
    }
}


[System.Serializable]
public class ConditionalGate
{
    bool entered = false;
    bool exited = false;
    Action onEnter;
    Action onExit;
    public void Setup(Action enterAction, Action exitAction)
    {
        onEnter = enterAction;
        onExit = exitAction;
    }
    public void Enter()
    {
        if(entered || onEnter == null) return;
        entered = true;
        exited = false;
        onEnter.Invoke();
    }
    public void Check(bool condition)
    {
        if (condition)
        {
            Enter();
        }
        else
        {
            Exit();
        }    
    }
    public void Exit()
    {
        if(exited || onExit == null) return;
        entered = false;
        exited = true;
        onExit.Invoke();
    }
    
}