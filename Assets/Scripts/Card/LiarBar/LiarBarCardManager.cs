using System;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Random = UnityEngine.Random;

public class LiarBarCardManager : MonoBehaviourPun
{
    public static LiarBarCardManager Instance { get; private set; }

    Dictionary<ELiarBarCardType, int> _cardCounts = new Dictionary<ELiarBarCardType, int>();

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
        for (int i = _startDealCardIndex; i < _startDealCardIndex + players.Count; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                int randomCard = Random.Range(0, (int)ELiarBarCardType.Max);
                while (_cardCounts[(ELiarBarCardType)randomCard] <= 0)
                {
                    randomCard = Random.Range(0, _cardCounts.Count);
                }

                GamePlayer targetPlayer = players[i % players.Count];

                // RPC로 다른 클라이언트에도 카드 추가 요청
                photonView.RPC("RPC_AddCardToHand", targetPlayer.photonView.Owner, randomCard);

                _cardCounts[(ELiarBarCardType)randomCard]--;
            }
        }
        _startDealCardIndex = (_startDealCardIndex + 1) % players.Count;
    }

    public void SetTable()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        _targetCard = (ELiarBarCardType)Random.Range(0, (int)ELiarBarCardType.JokerCard);

        // 모든 클라이언트에 TargetCard 전달
        photonView.RPC("RPC_SetTargetCard", RpcTarget.All, (int)_targetCard);
        DealCardsToPlayers();
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
    #endregion
}