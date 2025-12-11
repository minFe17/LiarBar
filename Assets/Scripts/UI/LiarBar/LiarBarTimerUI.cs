using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LiarBarTimerUI : MonoBehaviour
{
    [SerializeField] Text _timerText;

    Coroutine _timerCoroutine;
    int _duration = 30;

    public Action OnTimerFinished;

    void OnEnable()
    {
        _timerCoroutine = StartCoroutine(TimerRoutine());
    }

    void OnDisable()
    {
        if (_timerCoroutine != null)
        {
            StopCoroutine(_timerCoroutine);
            _timerCoroutine = null;
        }
    }

    void OnTimerEnd()
    {
        OnTimerFinished?.Invoke();
    }

    IEnumerator TimerRoutine()
    {
        int t = _duration;

        while (t > 0)
        {
            _timerText.text = t.ToString();
            yield return new WaitForSeconds(1f);
            t--;
        }

        _timerText.text = "0";
        OnTimerEnd();
    }
}