using Photon.Voice;
using UnityEngine;
using System;
using TMPro;

public class SeotdaTurnUI : MonoBehaviour
{
    private const float MAX_TIME = 30f;

    [SerializeField]
    private GameObject _potTurn;
    [SerializeField]
    private GameObject _summitTurn;
    [SerializeField]
    private TextMeshProUGUI _timer;

    private bool _isFlow = false;
    private float _time = 0;
    

    private void Start()
    {
        OffUI();
    }
    private void OnEnable()
    {
        EventManager.Instance.Subscribe("OffTurnUI", OffUI);
        EventManager.Instance.Subscribe("OnSummitUI", OnSummitUI);
        EventManager.Instance.Subscribe("OnMoneyUI", OnMoneyUI);
    }
    private void OnDisable()
    {
        EventManager.Instance.UnSubscribe("OffTurnUI", (Action)OffUI);
        EventManager.Instance.UnSubscribe("OnSummitUI",(Action) OnSummitUI);
        EventManager.Instance.UnSubscribe("OnMoneyUI", (Action)OnMoneyUI);
    }
    private void Update()
    {
        if (!_isFlow) return;
        
        _time -= Time.deltaTime;
        _timer.text = ((int)_time).ToString();

        if(_time<10)
            _timer.color = Color.red;

        if(_time<0)
        {
            _timer.color = Color.white;
            _isFlow = false;
            EventManager.Instance.Invoke("EndTime");
        }    
    }

    private void OffUI()
    {
        _potTurn.SetActive(false);
        _summitTurn.SetActive(false);
        _isFlow = false;
    }
    private void OnSummitUI()
    {
        _summitTurn.SetActive(true);
        SetFlow();
    }
    private void OnMoneyUI()
    {
        _potTurn.SetActive(true);
        SetFlow();
    }
    private void SetFlow()
    {
        _isFlow = true;
        _time = MAX_TIME;
        _timer.color = Color.white;
    }
}
