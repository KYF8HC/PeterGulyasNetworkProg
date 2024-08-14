using System;

public class Timer
{
    public float Duration { get; private set; }
    public Action Callback { get; private set; }
    public bool IsRepeating { get; private set; }
    public float TimeRemaining { get; set; }
    public string Id { get; private set; }

    public Timer(float duration, Action callback, bool isRepeating, string id)
    {
        Duration = duration;
        Callback = callback;
        IsRepeating = isRepeating;
        TimeRemaining = duration;
        Id = id;
    }
}