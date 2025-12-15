using System;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;
using UnityEngine.U2D;
using Random = UnityEngine.Random;

public class LiarBarCardManager : MonoBehaviourPun
{
    public static LiarBarCardManager Instance { get; private set; }

    Dictionary<ELiarBarCardType, int> _cardCounts = new Dictionary<ELiarBarCardType, int>();
    SpriteAtlas _cardAtlas;
    int _startDealCardIndex;
    ELiarBarCardType _targetCard;

    public Action OnSetTableAction;
    public ELiarBarCardType TargetCard { get => _targetCard; }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        Init();
        _cardAtlas = Resources.Load<SpriteAtlas>("SpriteAtlas/LiarBarCardAtlas");
    }

    void Init()
    {
        _cardCounts[ELiarBarCardType.ACard] = 6;
        _cardCounts[ELiarBarCardType.QCard] = 6;
        _cardCounts[ELiarBarCardType.KCard] = 6;
        _cardCounts[ELiarBarCardType.JokerCard] = 2;
    }

    void DealCardsToPlayers()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        IReadOnlyList<GamePlayer> players = TurnManager.Instance.Players;

        // 모든 플레이어의 손패 초기화 (각 클라이언트에서 실행)
        foreach (GamePlayer player in players)
        {
            photonView.RPC("RPC_ClearHand", player.photonView.Owner);
        }

        for (int i = _startDealCardIndex; i < _startDealCardIndex + players.Count; i++)
        {
            int previousCardIndex = -1;
            for (int j = 0; j < 5; j++)
            {
                bool hasOtherCard = _cardCounts.Count(kv => kv.Value > 0) > 1;
                int randomCard = Random.Range(0, (int)ELiarBarCardType.Max);

                while (_cardCounts[(ELiarBarCardType)randomCard] <= 0 ||
                       (hasOtherCard && previousCardIndex == randomCard))
                {
                    randomCard = Random.Range(0, (int)ELiarBarCardType.Max);
                }

                GamePlayer targetPlayer = players[i % players.Count];
                photonView.RPC("RPC_AddCardToHand", targetPlayer.photonView.Owner, randomCard);
                _cardCounts[(ELiarBarCardType)randomCard]--;
                previousCardIndex = randomCard;
            }
        }

        _startDealCardIndex = (_startDealCardIndex + 1) % players.Count;
    }

    public void SetTable()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        Init();
        _targetCard = (ELiarBarCardType)Random.Range(0, (int)ELiarBarCardType.JokerCard);

        photonView.RPC("RPC_SetTargetCard", RpcTarget.All, (int)_targetCard);
        DealCardsToPlayers();
    }

    public Sprite GetCardSprite(ELiarBarCardType type)
    {
        return _cardAtlas.GetSprite(type.ToString());
    }

    #region RPC
    [PunRPC]
    void RPC_SetTargetCard(int cardType)
    {
        _targetCard = (ELiarBarCardType)cardType;
        OnSetTableAction?.Invoke();
    }

    [PunRPC]
    void RPC_AddCardToHand(int cardType)
    {
        GamePlayer localPlayer = TurnManager.Instance.Players.FirstOrDefault(p => p.photonView.IsMine);
        if (localPlayer != null)
            localPlayer.AddCardToHand((ELiarBarCardType)cardType);
    }

    [PunRPC]
    void RPC_ClearHand()
    {
        GamePlayer localPlayer = TurnManager.Instance.Players.FirstOrDefault(p => p.photonView.IsMine);
        if (localPlayer != null)
            localPlayer.ClearHand();
    }
    #endregion
}