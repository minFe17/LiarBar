using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
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

    public IReadOnlyList<ELiarBarCardType> Cards => _cards;
    public PhotonView PhotonView => photonView;
    public Animator Animator { set => _animator = value; }
    public Transform HandCardSlot { set => _handCardSlot = value; }
    public int TurnIndex { get; private set; }       // 절대 변경하지 않음
    public int ViewID => photonView.ViewID;
    public bool IsMyTurn => _isMyTurn;

    void Start()
    {
        TurnIndex = (int)photonView.Owner.CustomProperties["PositionIndex"];
        TurnManager.Instance.RegisterPlayer(this);
        _deadPotionIndex = Random.Range(0, _totalPotionCount);
    }

    public void StartTurn()
    {
        photonView.RPC(nameof(RPC_ShowTurnUI), RpcTarget.All, ViewID);
    }

    public void Win() { }

    public void Die()
    {
        if (!photonView.IsMine)
            return;

        _animator.SetTrigger("doDie");
        TurnManager.Instance.DiePlayer(this);
    }

    public void AddCardToHand(ELiarBarCardType randomCard)
    {
        _cards.Add(randomCard);

        if (_cards.Count == 5 && photonView.IsMine) // 자기 클라이언트일 때만 UI 갱신
            SimpleSingleton<MediatorManager>.Instance.Notify(EMediatorEventType.InitHandCard, this);
    }

    public void PlayCard(List<ELiarBarCardType> cardTypes)
    {
        if (!photonView.IsMine || !_isMyTurn)
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

    public void SetMyTurn(bool value)
    {
        _isMyTurn = value;
    }

    IEnumerator SpawnPotionNextFrame()
    {
        yield return null;

        _potion = PhotonNetwork.Instantiate("LiarBarPotion", _handCardSlot.position, Quaternion.identity)
            .GetComponent<LiarBarPotion>();

        _potion.Init(_handCardSlot);
    }

    #region Animation Events
    public void EndCallLiar()
    {
        if (!photonView.IsMine)
            return;

        LiarBarTable.Instance.CheckLiar(ViewID);
    }

    public void CreateCard()
    {
        if (!photonView.IsMine || !_isMyTurn)
            return;

        int[] cardInts = _currentCardTypes.Select(c => (int)c).ToArray();
        photonView.RPC(nameof(RPC_RequestCreateCard), RpcTarget.MasterClient, cardInts, ViewID);

        _currentCardTypes.Clear();
        _isMyTurn = false;

        TurnManager.Instance.EndTurn();
    }

    public void DrinkPotionEvent()
    {
        if (!photonView.IsMine)
            return;

        _potion.DrinkPotion();
    }

    public void ThrowPotion()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        // 다음 라운드 시작 플레이어를 MasterClient에 저장
        TurnManager.Instance.SetNextRoundStartPlayer(TurnIndex);

        // 한 프레임 대기 후 새 라운드 시작
        StartCoroutine(ContinueGameRoutine());
    }

    IEnumerator ContinueGameRoutine()
    {
        yield return null;
        LiarBarTable.Instance.NewRound();
    }
    #endregion

    #region RPC
    [PunRPC]
    void RPC_ShowTurnUI(int viewID)
    {
        GamePlayer player = PhotonView.Find(viewID).GetComponent<GamePlayer>();
        LiarBarTable.Instance.TurnUI.ShowNextPlayer(player);

        // 턴 여부 동기화
        _isMyTurn = (player.ViewID == ViewID);

        // 자기 턴이면 이벤트 호출
        if (_isMyTurn)
            OnStartTurn?.Invoke();
    }

    [PunRPC]
    void RPC_DoDrinkPotion()
    {
        DrinkPotion();
    }

    [PunRPC]
    void RPC_RequestCreateCard(int[] cardTypes, int playerViewID)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        GamePlayer player = PhotonView.Find(playerViewID).GetComponent<GamePlayer>();

        float duration = 0.5f;
        List<LiarBarCard> cards = new List<LiarBarCard>();

        foreach (int type in cardTypes)
        {
            GameObject cardObj = PhotonNetwork.Instantiate("Card", player._handCardSlot.position, player._handCardSlot.rotation);
            LiarBarCard card = cardObj.GetComponent<LiarBarCard>();
            card.Init((ELiarBarCardType)type);
            cards.Add(card);

            card.MoveToTable(LiarBarTable.Instance.GetCenterPosition(), duration);
            duration += 0.2f;
        }

        LiarBarTable.Instance.SavePlayedCards(cards);

        TurnManager.Instance.EndTurn();
    }
    #endregion
}