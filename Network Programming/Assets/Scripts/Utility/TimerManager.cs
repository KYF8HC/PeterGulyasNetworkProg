using System;
using System.Collections.Generic;
using UnityEngine;

public class TimerManager : MonoBehaviour
{
    public static TimerManager Instance { get; private set; }

    private List<Timer> _timers = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        for (int i = _timers.Count - 1; i >= 0; i--)
        {
            _timers[i].TimeRemaining -= Time.deltaTime;
            if (_timers[i].TimeRemaining <= 0)
            {
                _timers[i].Callback?.Invoke();

                if (_timers[i].IsRepeating)
                {
                    _timers[i].TimeRemaining = _timers[i].Duration;
                }
                else
                {
                    _timers.RemoveAt(i);
                }
            }
        }
    }

    public string SetTimer(float duration, Action callback, bool isRepeating = false)
    {
        string id = Guid.NewGuid().ToString();
        _timers.Add(new Timer(duration, callback, isRepeating, id));
        return id;
    }

    public void ClearTimer(string id)
    {
        _timers.RemoveAll(t => t.Id == id);
    }

    public void ClearAllTimers()
    {
        _timers.Clear();
    }
}