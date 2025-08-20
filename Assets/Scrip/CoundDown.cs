using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Countdown : Singleton<BlockMaterialControl>
{
    [System.Serializable]
    public class CountdownEntry
    {
        public string name;
        public float duration;
        public float elapsed;
        public bool isRunning;
        public UnityEvent onComplete;
    }
    private List<CountdownEntry> timers = new List<CountdownEntry>();

    private void Update()
    {
        foreach (var timer in timers)
        {
            if (!timer.isRunning) continue;

            timer.elapsed += Time.deltaTime;
            if (timer.elapsed >= timer.duration)
            {
                timer.isRunning = false;
                timer.onComplete?.Invoke();
            }
        }
    }

    public void InitCountdown(string name, float duration, UnityEvent onComplete)
    {

        CountdownEntry CD = new CountdownEntry();
        CD.duration = duration;
        CD.onComplete = onComplete;
        CD.name = name;
        timers.Add(CD);
    }

    public void StartCountdown(string name)
    {
        var timer = timers.Find(t => t.name == name);
        if (timer != null)
        {
            timer.elapsed=0;
            timer.isRunning = true;
        }
    }

    public void StopCountdown(string name)
    {
        var timer = timers.Find(t => t.name == name);
        if (timer != null)
        {
            timer.isRunning = false;
        }
    }

    public bool IsRunning(string name)
    {
        var timer = timers.Find(t => t.name == name);
        return timer != null && timer.isRunning;
    }

    public bool IsDone(string name)
    {
        var timer = timers.Find(t => t.name == name);
        return timer != null && !timer.isRunning && timer.elapsed >= timer.duration;
    }
}
