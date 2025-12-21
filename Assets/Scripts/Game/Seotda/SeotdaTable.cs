using Photon.Voice;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System;
using Photon.Pun;
using Photon.Realtime;

public class SeotdaTable : MonoBehaviourPun
{
    const float SPACE_AREA = 0.4f;
    const float SMOOTH_SPEED = 1f;
    const int MAX_RANGE = 3;

    private List<FlowerCard> _cards = new List<FlowerCard>();

    private bool _isSplitEvent = true;
    private List<bool> _isAlives = new List<bool>();

    private Vector3 _originPos;
    private float _timer = 0f;
    private int _splitNum = 0;

    private void Awake()
    {
        FindCards();
    }
    private void OnEnable()
    {
        SubscribeEvent();
    }
    private void OnDisable()
    {
        UnSubscribeEvent();
    }
    private void Start()
    {
        MixCard();
        RotationCard();

    }
    private void Update()
    {
        if (!_isSplitEvent)
            SplitCardToEvent();

        if(_timer>2.0f && _splitNum==0)
            ResetSeotdaTable();
        else if(_timer>3.0f && _splitNum == 1)
        {
            OnSplitEvent();
            GetComponent<SeotdaTurnManager>().StartGame();
            return;
        }
        else if(_timer<=3.0f)
            _timer += Time.deltaTime;
    }
    //리셋 만들어주기
    private void SubscribeEvent()
    {
        EventManager.Instance.Subscribe("OnSplit", OnSplitEvent);
        EventManager.Instance.Subscribe<bool>("ResetSeotdaTable", ResetSeotdaTable);
    }
    private void UnSubscribeEvent()
    {
        EventManager.Instance.UnSubscribe("OnSplit",(Action)OnSplitEvent);
        EventManager.Instance.UnSubscribe("ResetSeotdaTable", (Action<bool>)ResetSeotdaTable);
    }
    private void ResetSeotdaTable(bool isRestart = false)
    {
        _timer = 0f;
        EventManager.Instance.Invoke("OffEndGameUI");
        EventManager.Instance.Invoke("ResetGameManager");
        EventManager.Instance.Invoke("OffEndGameUI");
        if (!PhotonNetwork.IsMasterClient) return;
        OnSplitEvent();
        photonView.RPC("RPC_ResetCards", RpcTarget.All);
        if(isRestart)
            GetComponent<SeotdaTurnManager>().ReStartGame();
        else
            GetComponent<SeotdaTurnManager>().StartGame();
    }
    private void OnSplitEvent()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        photonView.RPC("RPC_OnSplitEvent", RpcTarget.All);
    }
    private void SplitCardToEvent()
    { 
        _isSplitEvent = true;
        Vector3[] positionList = { new Vector3(0, transform.position.y, MAX_RANGE), new Vector3(-MAX_RANGE, transform.position.y, 0),
            new Vector3(0, transform.position.y, -MAX_RANGE), new Vector3(MAX_RANGE, transform.position.y, 0) };

        for (int i = 0; i < positionList.Length; i++)
        {
            if (!_isAlives[i] || !_cards[i].gameObject.activeSelf) continue;
            if (_cards[i].transform.position == positionList[i])
            {
                _cards[i].gameObject.SetActive(false);
                _cards[i].gameObject.transform.position = _originPos;
                continue;
            }
            _cards[i].transform.position = Vector3.Lerp(_cards[i].transform.position, positionList[i] , SMOOTH_SPEED * Time.deltaTime * 2);
            if (Vector3.Distance(_cards[i].transform.position, positionList[i]) < 1)
            {
                _cards[i].transform.position = positionList[i];
            }
            _isSplitEvent = false;
        }
        if (_isSplitEvent)
        {
            for (int i = 0; i < positionList.Length; i++)
            {
                _cards[i].gameObject.SetActive(true);
            }
            EventManager.Instance.Invoke("SplitCard");
        }
            

    }

    private void RotationCard()
    {
        Quaternion rotation = Quaternion.Euler(new Vector3(90, 180, 0));
        foreach (var card in _cards)
        {
            card.transform.rotation = rotation;
        }
    }
    private void MixCard()
    {
        _originPos = new Vector3(0, transform.position.y-0.01f, 0);
        foreach(var card in _cards)
        {
            card.transform.position = _originPos;
        }
    }
    private void FindCards()
    {
        int count = 0;
        FlowerCard[] cards = GetComponentsInChildren<FlowerCard>();
        Vector3 rotation = new Vector3(270, 180, 0);
        Vector3 position = new Vector3(-SPACE_AREA * 1.5f, 0, SPACE_AREA * 1.5f);
        foreach (FlowerCard card in cards)
        {
            card.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            card.transform.rotation = Quaternion.Euler(rotation);
            card.transform.localPosition = position;
            count++;

            if(count == 5)
            {
                position.x = -SPACE_AREA * 1.5f;
                position.z-= SPACE_AREA;
                count = 0;
            }
            else
            {
                position.x += SPACE_AREA;
            }

            _cards.Add(card);
            Debug.Log(card.Month +","+ card.Type);
        }
    }

    private bool IsPlayerAlive(Player player)
    {
        if (player.CustomProperties.TryGetValue("IsAlive", out object isAlive))
            return (bool)isAlive;

        return true; 
    }
    private Player GetPlayerByPosition(int positionIndex)
    {
        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (player.CustomProperties.TryGetValue("PositionIndex", out object index))
            {
                if ((int)index == positionIndex)
                    return player;
            }
        }

        return null;
    }

    [PunRPC]
    private void RPC_OnSplitEvent()
    {
        _splitNum++;
        _isSplitEvent = false;
        _isAlives.Clear();
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            _isAlives.Add(IsPlayerAlive(GetPlayerByPosition(i)));
        }
    }
}
