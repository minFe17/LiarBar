using Photon.Voice;
using UnityEngine;
using System;
using TMPro;
using Utils;

public class SeotdaTurnUI : MonoBehaviour
{
    private const float MAX_TIME = 30f;

    [SerializeField]
    private GameObject _potTurn;
    [SerializeField]
    private GameObject _summitTurn;
    [SerializeField]
    private TextMeshProUGUI _timer;

    [SerializeField]
    private TextMeshProUGUI _myMoney;
    [SerializeField]
    private TextMeshProUGUI _potMoney;
    [SerializeField]
    private TextMeshProUGUI _stakeMoney;

    private bool _isFlow = false;
    private float _time = 0;
    private int _curMyMoney = 0;
    private int _curPotMoney = 0;
    private int _curStakeMoney = 0;

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
        UpdateMoney();

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
    private void UpdateMoney()
    {
        int money = MyPlayer.local.Money;
        if (_curMyMoney != money)
        {
            _myMoney.text = "³» µ· : " + money.ToString();
            _curMyMoney = money;
        }
        money = GetComponentInParent<SeotdaGameManager>().Pot;
        if (_curPotMoney != money)
        {
            _potMoney.text = "¸ðÀÎ µ· : " + money.ToString();
            _curPotMoney = money;
        }
        money = GetComponentInParent<SeotdaGameManager>().Stake;
        if (_curStakeMoney != money)
        {
            _stakeMoney.text = "ÆÇµ· : " + money.ToString();
            _curStakeMoney = money;
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
