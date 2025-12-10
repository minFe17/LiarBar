using Photon.Voice;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System;
using Photon.Pun;
using Photon.Realtime;

public class SeotdaTable : MonoBehaviour
{
    const float SPACE_AREA = 0.4f;
    const float SMOOTH_SPEED = 1f;
    const int MAX_RANGE = 3;

    private List<FlowerCard> _cards = new List<FlowerCard>();
    private bool _isMixed = false;
    private bool _isRotation = false;
    private bool _isSplit = false;
    private bool _isSplitEvent = true;
    private int _spaceCardNum = 0;
    private List<bool> _isAlives = new List<bool>();


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
    private void Update()
    {
        if (!_isMixed)
            MixCard();
        else if (!_isRotation)
            RotationCard();
        else if (!_isSplit)
            SplitCard(); //±»ÀÌ ³ÀµÖ¾ßµÇ³ª?

        if (!_isSplitEvent)
            SplitCardToEvent();
    }
    private void SubscribeEvent()
    {
        EventManager.Instance.Subscribe("OnSplit", OnSplitEvent);
    }
    private void UnSubscribeEvent()
    {
        EventManager.Instance.UnSubscribe("OnSplit",(Action)OnSplitEvent);
    }
    private void OnSplitEvent()
    {
        _isSplitEvent = false;
        _isAlives.Clear();
        for(int i=0;i< PhotonNetwork.PlayerList.Length; i++)
        {
            _isAlives.Add(IsPlayerAlive(GetPlayerByPosition(i)));
        }
    }
    private void SplitCardToEvent()
    {
        _isSplitEvent = true;
        Vector3[] positionList = { new Vector3(0, 0, MAX_RANGE), new Vector3(-MAX_RANGE, 0, 0),
            new Vector3(0, 0, -MAX_RANGE), new Vector3(MAX_RANGE, 0, 0) };

        for (int i = 0; i < positionList.Length; i++)
        {
            if (!_isAlives[i] || !_cards[i].gameObject.activeSelf) continue;
            if (_cards[i].transform.position == positionList[i])
            {
                _cards[i].gameObject.SetActive(false);
                continue;
            }


            _cards[i+_spaceCardNum].transform.position = Vector3.Lerp(_cards[i+_spaceCardNum].transform.position, positionList[i] + this.gameObject.transform.position, SMOOTH_SPEED * Time.deltaTime * 2);

            if (Vector3.Distance(_cards[i+ _spaceCardNum].transform.position, positionList[i]) < 1)
            {
                _cards[i+_spaceCardNum].transform.position = positionList[i];
            }

            _isSplitEvent = false;
        }
        if (_isSplitEvent)
        {
            _spaceCardNum += 4;
            EventManager.Instance.Invoke("SplitCard");
        }
            

    }
    private void SplitCard()
    {
        _isSplit = true;
        Vector3[] positionList = { new Vector3(0, 0, MAX_RANGE), new Vector3(0, 0, -MAX_RANGE), 
            new Vector3(-MAX_RANGE, 0, 0), new Vector3(MAX_RANGE, 0, 0) };

        for (int i = 0; i < positionList.Length; i++)
        {
            if (!_cards[i].gameObject.activeSelf) continue;
            if (_cards[i].transform.position == positionList[i])
            {
                _cards[i].gameObject.SetActive(false);
                continue;
            }

            _cards[i].transform.position = Vector3.Lerp(_cards[i].transform.position, positionList[i] + this.gameObject.transform.position, SMOOTH_SPEED * Time.deltaTime*2);

            if (Vector3.Distance(_cards[i].transform.position, positionList[i]) < 1)
            {
                _cards[i].transform.position = positionList[i];
            }

            _isSplit = false;
        }
        if (_isSplit)
        {
            _spaceCardNum += 4;
            EventManager.Instance.Invoke("SplitCard");
            //OnSplitEvent();
        }

    }
    private void RotationCard()
    {
        _isRotation = true;
        Quaternion rotation = Quaternion.Euler(new Vector3(90, 180, 0));
        foreach (var card in _cards)
        {
            if (card.transform.rotation.x == rotation.x) continue;
            card.transform.rotation = Quaternion.Lerp(card.transform.rotation, rotation, SMOOTH_SPEED * Time.deltaTime * 2);

            if (Quaternion.Angle(card.transform.rotation, rotation) < 0.5f)
            {
                card.transform.rotation = rotation;
                continue;
            }
            _isRotation = false;
        }
    }
    private void MixCard()
    {
        _isMixed = true;
        foreach(var card in _cards)
        {
            if (card.transform.position == Vector3.zero) continue;
            card.transform.position = Vector3.Lerp(card.transform.position, this.gameObject.transform.position, SMOOTH_SPEED * Time.deltaTime);

            if (Vector3.Distance(card.transform.position, Vector3.zero) < 0.01f)
            {
                card.transform.position = Vector3.zero;
            }
            _isMixed = false;
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
}
