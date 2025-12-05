using System;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Utils;

public class GamePlayer : MonoBehaviourPun
{
    List<ELiarBarCardType> _cards = new List<ELiarBarCardType>();

    bool _isMyTurn = false;

    public Action OnStartTurn;

    public IReadOnlyList<ELiarBarCardType> Cards { get => _cards; }
    public PhotonView PhotonView { get => photonView; }
    public int TurnIndex { get; private set; }
    public int ViewID { get => photonView.ViewID; }
    public bool IsMyTurn { get => _isMyTurn; }

    void Start()
    {
        TurnManager.Instance.RegisterPlayer(this);
        SetTurnIndex();
    }

    void SetTurnIndex()
    {
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            Player player = PhotonNetwork.PlayerList[i];

            if (player.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                TurnIndex = GetCustomProperty(player, "PositionIndex", 0);
                break;
            }
        }
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
        if (TurnManager.Instance.CurrentPlayerIndex != TurnIndex)
            return;
        _isMyTurn = true;
        OnStartTurn?.Invoke();
    }

    public void Win()
    {

    }

    public void Die()
    {
        if (!photonView.IsMine)
            return;

        TurnManager.Instance.DiePlayer(this);
        // firebase 데이터 저장
    }

    public void AddCardToHand(ELiarBarCardType randomCard)
    {
        Debug.Log(randomCard);
        _cards.Add(randomCard);
        if (_cards.Count == 5)
            SimpleSingleton<MediatorManager>.Instance.Notify(EMediatorEventType.InitHandCard, this);
    }

    public void PlayCard(ELiarBarCardType card)
    {
        //TurnManager.Instance.EndTurn();
        _isMyTurn = false;
    }

    public void CallLiar()
    {
        // 애니메이션
    }
}