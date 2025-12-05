using Photon.Voice;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System;

public class SeotdaTable : MonoBehaviour
{
    const float SPACE_AREA = 0.4f;
    const float SMOOTH_SPEED = 1f;
    private List<FlowerCard> _cards = new List<FlowerCard>();
    private bool _isMixed = false;
    private bool _isRotation = false;
    private bool _isSplit = false;
    private bool _isSplitEvent = true;
    private int _spaceCardNum = 0;


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
            SplitCard();
        else if (!_isSplitEvent)
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
    }
    private void SplitCardToEvent()
    {
        _isSplitEvent = true;
        Vector3[] positionList = { new Vector3(4, 0, 0), new Vector3(-4, 0, 0), new Vector3(0, 0, 4), new Vector3(0, 0, -4) };

        for (int i = 0; i < positionList.Length; i++)
        {
            if (_cards[i+_spaceCardNum].transform.position == positionList[i]) continue;

            _cards[i+_spaceCardNum].transform.position = Vector3.Lerp(_cards[i+_spaceCardNum].transform.position, positionList[i], SMOOTH_SPEED * Time.deltaTime);

            if (Vector3.Distance(_cards[i+ _spaceCardNum].transform.position, positionList[i]) < 0.01f)
            {
                _cards[i+_spaceCardNum].transform.position = positionList[i];
            }

            _isSplitEvent = false;
        }
        if (_isSplitEvent)
            _spaceCardNum+=4;

    }
    private void SplitCard()
    {
        _isSplit = true;
        Vector3[] positionList = { new Vector3(4, 0, 0), new Vector3(-4, 0, 0), new Vector3(0, 0, 4), new Vector3(0, 0, -4) };
        
        for (int i = 0; i < positionList.Length; i++)
        {
            if (_cards[i].transform.position == positionList[i]) continue;

            _cards[i].transform.position = Vector3.Lerp(_cards[i].transform.position, positionList[i], SMOOTH_SPEED * Time.deltaTime);

            if (Vector3.Distance(_cards[i].transform.position, positionList[i]) < 0.01f)
            {
                _cards[i].transform.position = positionList[i];
            }

            _isSplit = false;
        }
        if (_isSplit)
            _spaceCardNum+=4;

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
            card.transform.position = Vector3.Lerp(card.transform.position, Vector3.zero, SMOOTH_SPEED * Time.deltaTime);

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
}
