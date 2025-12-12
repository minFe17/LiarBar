using Photon.Voice;
using UnityEngine;
using System;

public class SeotdaTurnUI : MonoBehaviour
{
    [SerializeField]
    private GameObject _potTurn;
    [SerializeField]
    private GameObject _summitTurn;

    void Start()
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

    private void OffUI()
    {
        _potTurn.SetActive(false);
        _summitTurn.SetActive(false);
    }
    private void OnSummitUI()
    {
        _summitTurn.SetActive(true);
    }
    private void OnMoneyUI()
    {
        _potTurn.SetActive(true);
    }
}
