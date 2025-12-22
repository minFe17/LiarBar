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
    int _rank = -1;
    bool _isMyTurn = false;

    public Action OnStartTurn;

    public IReadOnlyList<ELiarBarCardType> Cards => _cards;
    public PhotonView PhotonView => photonView;
    public Animator Animator { set => _animator = value; }
    public Transform HandCardSlot { set => _handCardSlot = value; }
    public int TurnIndex { get; private set; }
    public int ViewID => photonView.ViewID;
    public bool IsMyTurn => _isMyTurn;
    public int Rank => _rank;

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

        photonView.RPC(nameof(RPC_DieAnimation), RpcTarget.All);
        TurnManager.Instance.DiePlayer(this);
        TurnManager.Instance.SetNextRoundStartPlayer(TurnIndex);
        StartCoroutine(ContinueGameRoutine());
    }

    public void SetRank(int rank)
    {
        _rank = rank;
    }

    public void AddCardToHand(ELiarBarCardType randomCard)
    {
        _cards.Add(randomCard);

        if (_cards.Count == 5 && photonView.IsMine)
            SimpleSingleton<MediatorManager>.Instance.Notify(EMediatorEventType.InitHandCard, this);
    }

    public void ClearHand()
    {
        _cards.Clear();
    }

    public void PlayCard(List<ELiarBarCardType> cardTypes)
    {
        if (!photonView.IsMine || !_isMyTurn)
            return;

        _currentCardTypes.AddRange(cardTypes);
        photonView.RPC(nameof(RPC_PlayCardAnimation), RpcTarget.All);
    }

    public void CallLiar()
    {
        if (!photonView.IsMine)
            return;

        photonView.RPC(nameof(RPC_CallLiarAnimation), RpcTarget.All);
    }

    public void DrinkPotion()
    {
        if (!photonView.IsMine)
            return;

        photonView.RPC(nameof(RPC_DrinkPotionAnimation), RpcTarget.All);
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
    }

    public void ThrowPotion()
    {
        if (_potion != null)
            _potion.ThrowPotion();

        if (photonView.IsMine)
        {
            SimpleSingleton<MediatorManager>.Instance.Notify(EMediatorEventType.DrinkPotion);
            photonView.RPC(nameof(RPC_CheckPotionResult), RpcTarget.MasterClient);
        }
    }
    #endregion

    #region RPC
    [PunRPC]
    void RPC_ShowTurnUI(int viewID)
    {
        GamePlayer player = PhotonView.Find(viewID).GetComponent<GamePlayer>();
        LiarBarTable.Instance.TurnUI.ShowNextPlayer(player);

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

    [PunRPC]
    void RPC_CheckPotionResult()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (_deadPotionIndex == _currentPotionIndex)
        {
            photonView.RPC(nameof(RPC_Die), photonView.Owner);
            return;
        }

        _currentPotionIndex++;
        TurnManager.Instance.SetNextRoundStartPlayer(TurnIndex);
        StartCoroutine(ContinueGameRoutine());
    }

    [PunRPC]
    void RPC_Die()
    {
        Die();
    }

    [PunRPC]
    void RPC_DieAnimation()
    {
        _animator.SetTrigger("doDie");
    }

    [PunRPC]
    void RPC_DrinkPotionAnimation()
    {
        _animator.SetTrigger("doDrinkPotion");
    }

    [PunRPC]
    void RPC_PlayCardAnimation()
    {
        _animator.SetTrigger("doCard");
    }

    [PunRPC]
    void RPC_CallLiarAnimation()
    {
        _animator.SetTrigger("doCallLiar");
    }

    IEnumerator ContinueGameRoutine()
    {
        yield return null;
        LiarBarTable.Instance.NewRound();
    }
    #endregion
}