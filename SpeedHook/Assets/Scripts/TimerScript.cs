using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LootLocker.Requests;

public class TimerScript : MonoBehaviour
{
    public MANAGER manager;
    public float timer = 0;
    private bool isTimerRunning = false;

    public PlatformTriggerScript startPlatform;
    public PlatformTriggerScript endPlatform;

    public TextMeshProUGUI timerText;

    public Text t;

    public bool isTimerSet = false;

    void Start()
    {
        StartCoroutine(resetTimerDelay());
    }

    IEnumerator resetTimerDelay()
    {
        yield return new WaitForSeconds(.1f);
        ResetTimer();
    }

    void Update()
    {
        if (isTimerRunning && isTimerSet)
        {
            timer += Time.deltaTime;
        }

        if (isTimerSet && !startPlatform.PlayerInsideZone())
        {
            StartTimer();
        }
        if (isTimerRunning && endPlatform.PlayerInsideZone())
        {
            StopTimer();
        }
        if (!isTimerSet && startPlatform.PlayerInsideZone())
        {
            ResetTimer();
        }

        UpdateUI();
    }

    public void UpdateUI()
    {
        t.text = $"Time: {timer:#.000}";
    }

    public void ResetTimer()
    {
        timer = 0;
        isTimerSet = true;
    }

    public void StartTimer()
    {
        isTimerRunning = true;
    }

    public void StopTimer()
    {
        isTimerRunning = false;
        isTimerSet = false;
        StartCoroutine(Finish());


    }

    public IEnumerator Finish()
    {

        int miliseconds = Mathf.RoundToInt(timer * 1000);
        yield return manager.SubmitScoreRoutine(miliseconds);
        manager.EndScreen();
    }

    public float CurrentTimer()
    {
        return timer;
    }
}
