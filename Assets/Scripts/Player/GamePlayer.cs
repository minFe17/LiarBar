using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

public class GamePlayer : MonoBehaviourPun
{
    List<ELiarBarCardType> _cards = new List<ELiarBarCardType>();
    List<ELiarBarCardType> _currentCardTypes = new List<ELiarBarCardType>();
    Animator _animator;
    Transform _handCardSlot;
    LiarBarPotion _potion;

    int _totalPotionCount = 6;
    int _deadPotionIndex;
    int _currentPotionIndex;
    bool _isMyTurn = false;

    public Action OnStartTurn;

    public IReadOnlyList<ELiarBarCardType> Cards { get => _cards; }
    public PhotonView PhotonView { get => photonView; }
    public Animator Animator { set => _animator = value; }
    public Transform HandCardSlot { set => _handCardSlot = value; }
    public int TurnIndex { get; private set; }
    public int ViewID { get => photonView.ViewID; }
    public bool IsMyTurn { get => _isMyTurn; }

    void Start()
    {
        TurnIndex = (int)photonView.Owner.CustomProperties["PositionIndex"];
        TurnManager.Instance.RegisterPlayer(this);
        _deadPotionIndex = Random.Range(0, _totalPotionCount);
    }

    T GetCustomProperty<T>(Player player, string key, T defaultValue)
    {
        if (player.CustomProperties.TryGetValue(key, out object value) && value is T typedValue)
            return typedValue;
        return defaultValue;
    }

    public void StartTurn()
    {
        if (!photonView.IsMine)
            return;

        TurnManager.Instance.NotifyTurnStarted();

        _isMyTurn = true;
        OnStartTurn?.Invoke();

        // 마스터 클라에게 요청
        if (!PhotonNetwork.IsMasterClient)
            photonView.RPC("RPC_RequestTurnUI", PhotonNetwork.MasterClient, photonView.ViewID);
        else
            LiarBarTable.Instance.TurnUI.ShowNextPlayer(this);
    }

    public void Win()
    {

    }

    public void Die()
    {
        if (!photonView.IsMine)
            return;
        _animator.SetTrigger("doDie");
        TurnManager.Instance.DiePlayer(this);
        // firebase 데이터 저장
    }

    public void AddCardToHand(ELiarBarCardType randomCard)
    {
        _cards.Add(randomCard);
        if (_cards.Count == 5)
            SimpleSingleton<MediatorManager>.Instance.Notify(EMediatorEventType.InitHandCard, this);
    }

    public void PlayCard(List<ELiarBarCardType> cardTypes)
    {
        if (!photonView.IsMine)
            return;
        _currentCardTypes.AddRange(cardTypes);
        _animator.SetTrigger("doCard");
    }

    public void CallLiar()
    {
        if (!photonView.IsMine)
            return;
        _animator.SetTrigger("doCallLiar");
    }

    public void DrinkPotion()
    {
        _animator.SetTrigger("doDrinkPotion");
        if (!photonView.IsMine)
            return;
        StartCoroutine(SpawnPotionNextFrame());
    }

    IEnumerator SpawnPotionNextFrame()
    {
        yield return null;
        _potion = PhotonNetwork.Instantiate("LiarBarPotion", _handCardSlot.position, Quaternion.identity).GetComponent<LiarBarPotion>();
        _potion.Init(_handCardSlot);
    }

    #region Animation Controller Event
    public void EndCallLiar()
    {
        if (!photonView.IsMine)
            return;
        LiarBarTable.Instance.CheckLiar(ViewID);
    }

    public void CreateCard()
    {
        if (!photonView.IsMine)
            return;

        List<LiarBarCard> cards = new List<LiarBarCard>();

        float duration = 0.5f;

        foreach (ELiarBarCardType type in _currentCardTypes)
        {
            // Photon 네트워크 동기화 생성
            GameObject cardObj = PhotonNetwork.Instantiate("Card", _handCardSlot.position, _handCardSlot.rotation);

            LiarBarCard card = cardObj.GetComponent<LiarBarCard>();
            card.Init(type);
            cards.Add(card);
            card.MoveToTable(LiarBarTable.Instance.GetCenterPosition(), duration);
            duration += 0.2f;
        }

        LiarBarTable.Instance.SavePlayedCards(cards);
        _isMyTurn = false;
        TurnManager.Instance.EndTurn();
        _currentCardTypes.Clear();
    }

    public void DrinkPotionEvent()
    {
        if (!photonView.IsMine)
            return;
        _potion.DrinkPotion();
    }

    public void ThrowPotion()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            TurnManager.Instance.SetNextRoundStartPlayer(TurnIndex); // 다음 라운드 시작 플레이어만 지정
            LiarBarTable.Instance.NewRound(); // NewRoundRoutine 안에서 ContinueGame 호출됨
        }
    }
    #endregion

    [PunRPC]
    void RPC_DoDrinkPotion()
    {
        DrinkPotion();
    }

    [PunRPC]
    void RPC_RequestTurnUI(int viewID)
    {
        GamePlayer player = PhotonView.Find(viewID).GetComponent<GamePlayer>();
        LiarBarTable.Instance.TurnUI.ShowNextPlayer(player);
    }
}