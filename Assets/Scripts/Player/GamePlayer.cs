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
    int _rank = -1; // 순위 추가
    bool _isMyTurn = false;

    public Action OnStartTurn;

    public IReadOnlyList<ELiarBarCardType> Cards => _cards;
    public PhotonView PhotonView => photonView;
    public Animator Animator { set => _animator = value; }
    public Transform HandCardSlot { set => _handCardSlot = value; }
    public int TurnIndex { get; private set; }
    public int ViewID => photonView.ViewID;
    public bool IsMyTurn => _isMyTurn;
    public int Rank => _rank; // 순위 프로퍼티 추가

    void Start()
    {
        InGameManager manager = FindObjectOfType<InGameManager>();
        if (manager.Mode != EGameMode.LiarBar)
            return;

        if (photonView.IsMine)
        {
            Camera camera = GetComponentInChildren<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color32(92, 92, 92, 255);
        }

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

    // 순위 설정 메서드 추가
    public void SetRank(int rank)
    {
        _rank = rank;
        Debug.Log($"{photonView.Owner.NickName} 순위: {rank}등");
    }

    public void AddCardToHand(ELiarBarCardType randomCard)
    {
        _cards.Add(randomCard);

        if (_cards.Count == 5 && photonView.IsMine)
            SimpleSingleton<MediatorManager>.Instance.Notify(EMediatorEventType.InitHandCard, this);
    }

    // 새 라운드 시작 시 카드 초기화
    public void ClearHand()
    {
        _cards.Clear();
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
        if (!photonView.IsMine)
            return;

        _animator.SetTrigger("doDrinkPotion");
        StartCoroutine(SpawnPotionNextFrame());
    }

    public void SetMyTurn(bool value)
    {
        _isMyTurn = value;
    }

    IEnumerator SpawnPotionNextFrame()
    {
        yield return null;

        _potion = PhotonNetwork.Instantiate("LiarBarPotion", _handCardSlot.position, Quaternion.identity).GetComponent<LiarBarPotion>();

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
        photonView.RPC(nameof(RPC_UpdateThrowCardUI), RpcTarget.All, ViewID, _currentCardTypes.Count);

        _currentCardTypes.Clear();
        _isMyTurn = false;

        TurnManager.Instance.EndTurn();
    }

    public void DrinkPotionEvent()
    {
        if (!photonView.IsMine)
            return;

        _potion.DrinkPotion();
        SimpleSingleton<MediatorManager>.Instance.Notify(EMediatorEventType.DrinkPotion);
    }

    public void ThrowPotion()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        _potion.ThrowPotion();
        if (_deadPotionIndex == _currentPotionIndex)
        {
            Die();
            return;
        }

        _currentPotionIndex++;
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

        // 해당 플레이어만 턴 활성화
        _isMyTurn = (viewID == ViewID);

        if (_isMyTurn && photonView.IsMine)
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
    }

    [PunRPC]
    void RPC_UpdateThrowCardUI(int viewId, int count)
    {
        SimpleSingleton<MediatorManager>.Instance.Notify(EMediatorEventType.UpdateThrowCardUI, count);
    }
    #endregion
}