using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using Photon.Realtime;
using Photon.Pun;
using System;

public class SeotdaShowCards : MonoBehaviour
{
    [SerializeField]
    private FlowerCardDB db;

    private const float SPACE_VALUE = 0.06f;

    private Transform _playerLeftHand;
    private List<GameObject> _myCards;
    private PoolingManager _dumyCards;
    private List<(Player player, Transform hand)> _playerLeftHands;
    private int _cardCount = 0;

    private Vector3 _originalScale = new Vector3(0.03f, 0.03f, 0.03f);

    private void Start()
    {
        FindCards();
        
    }
    private void OnEnable()
    {
        EventManager.Instance.Subscribe("AddCard",AddShowCard);
        EventManager.Instance.Subscribe("ResetShowCards",ResetShowCards);
    }
    private void OnDisable()
    {
        EventManager.Instance.UnSubscribe("AddCard", (Action)AddShowCard);
        EventManager.Instance.UnSubscribe("ResetShowCards", (Action)ResetShowCards);
    }
    private void Update()
    {
        for(int i=0;i<_myCards.Count; i++)
        {
            Vector3 position = _myCards[i].transform.position;
            position.y = _playerLeftHand.position.y + SPACE_VALUE;
            _myCards[i].transform.position = position;
        }
    }
    private void UpdateCards()
    {
        //상하좌우 포지션 먼저 맞춰주기 (로테이션도) => 포지션 손위치에 걍 박으면됨 로테이션만 맞춰주기
        //카드 증가하는것도 상-> x감소 하->x증가 좌->z감소 우 ->z증가 이렇게 맞춰줘야됨.

        //상좌하우 로 바꾸기
        Quaternion[] rotations = {
            Quaternion.Euler(0, 0, 0),
            Quaternion.Euler(0, 270, 0),
            Quaternion.Euler(0, 180, 0),
            Quaternion.Euler(0, 90, 0)
        };

        Vector3[] addCardPosition =
        {
            new Vector3(-SPACE_VALUE*(_cardCount+1)-(SPACE_VALUE*0.5f), 0, 0),
             new Vector3(0, 0, -SPACE_VALUE*(_cardCount+1)-(SPACE_VALUE*0.5f)),
             new Vector3(SPACE_VALUE*(_cardCount+1)+(SPACE_VALUE*0.5f), 0, 0),
             new Vector3(0, 0, SPACE_VALUE*(_cardCount+1)+(SPACE_VALUE*0.5f))
        };

        for(int i=0;i<PhotonNetwork.PlayerList.Length;i++)
        {
            bool isAlive = (bool)_playerLeftHands[i].player.CustomProperties["IsAlive"];
            if (!isAlive) continue;

            int index = (int)_playerLeftHands[i].player.CustomProperties["PositionIndex"];

            Vector3 position = _playerLeftHands[i].hand.transform.position + addCardPosition[index];
            if (_playerLeftHands[i].player == MyPlayer.myPlayer)
            {
                GameObject card = Instantiate(_myCards[_cardCount],gameObject.transform);
                _myCards[_cardCount] = card;

                card.transform.localScale = _originalScale;
                card.transform.position = position;
                card.transform.rotation = rotations[index];
            }
            else
            {
                GameObject card = _dumyCards.Pop();
                position.y += SPACE_VALUE;

                card.transform.localScale = _originalScale;
                card.transform.position = position;
                card.transform.rotation = rotations[index];
            }
        }

        _cardCount++;
    }
    private void ResetShowCards()
    {
        _dumyCards.ResetObjects();
        for (int i = 0; i < _myCards.Count; i++)
        {
            Destroy(_myCards[i]);
        }
        _cardCount = 0;
    }
    private void FindPlayerLeftHand()
    {
        _playerLeftHand = MyPlayer.local.LeftHand;
        _playerLeftHands = MyPlayer.playerLeftHands;
    }
    private void AddShowCard()
    {
        if (_cardCount == 0)
            FindPlayerLeftHand();
        UpdateCards();
    }
    private void FindCards()
    {
        SeotdaCardManager manager = GetComponentInParent<SeotdaCardManager>();
        _myCards = manager.MyCards;
        _cardCount = _myCards.Count;

        _dumyCards = new PoolingManager(db.FlowerCardPrefabs[0], gameObject);
    }
}
