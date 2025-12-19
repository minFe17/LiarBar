using System;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;

public class TurnManager : MonoBehaviourPun
{
    public static TurnManager Instance { get; private set; }

    List<GamePlayer> _players = new List<GamePlayer>();
    Dictionary<int, int> _playerRankings = new Dictionary<int, int>();
    int _currentPlayerIndex = 0;
    int _nextRoundStartIndex = 0;
    int _currentRank;

    bool _isResolvingLiar = false;
    bool _isSettingTurn = false;
    bool _isFirstGame = true; // Ãß°¡!

    public Action OnEndRegisterPlayer;
    public Action OnGameEnd;

    public IReadOnlyList<GamePlayer> Players => _players;
    public int CurrentPlayerIndex => _currentPlayerIndex;
    public IReadOnlyDictionary<int, int> PlayerRankings => _playerRankings;

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
        LiarBarCardManager.Instance.OnSetTableAction += OnSetTableAction;
    }

    void OnSetTableAction()
    {
        if (_isFirstGame)
        {
            _isFirstGame = false;
            StartGame();
        }
    }

    public void RegisterPlayer(GamePlayer player)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (!_players.Contains(player))
            _players.Add(player);

        if (_players.Count == PhotonNetwork.CurrentRoom.PlayerCount)
        {
            _players = _players.OrderBy(p => p.TurnIndex).ToList();

            int[] viewIDs = _players.Select(p => p.ViewID).ToArray();
            photonView.RPC(nameof(RPC_SetPlayersList), RpcTarget.All, viewIDs);

            OnEndRegisterPlayer?.Invoke();
        }
    }

    void StartGame()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        _currentPlayerIndex = 0;
        _currentRank = PhotonNetwork.CurrentRoom.PlayerCount;
        _playerRankings.Clear();

        _isSettingTurn = true;
        photonView.RPC(nameof(RPC_SetCurrentTurn), RpcTarget.All, _currentPlayerIndex);
    }

    void NextTurn()
    {
        if (_isSettingTurn || _isResolvingLiar)
            return;

        _isSettingTurn = true;

        _currentPlayerIndex++;
        if (_currentPlayerIndex >= _players.Count)
            _currentPlayerIndex = 0;

        photonView.RPC(nameof(RPC_SetCurrentTurn), RpcTarget.All, _currentPlayerIndex);
    }

    public void EndTurn()
    {
        if (_isResolvingLiar)
            return;

        photonView.RPC(nameof(RPC_NextTurn), RpcTarget.MasterClient);
    }

    public void BeginResolveLiar()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        _isResolvingLiar = true;
    }

    public void ContinueGame()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        _isResolvingLiar = false;
        _isSettingTurn = true;

        _currentPlayerIndex = _nextRoundStartIndex;
        if (_currentPlayerIndex >= _players.Count)
            _currentPlayerIndex = 0;

        photonView.RPC(nameof(RPC_SetCurrentTurn), RpcTarget.All, _currentPlayerIndex);
    }

    public void SetNextRoundStartPlayer(int turnIndex)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        _nextRoundStartIndex = _players.FindIndex(p => p.TurnIndex == turnIndex);

        if (_nextRoundStartIndex < 0)
            _nextRoundStartIndex = _currentPlayerIndex;
    }

    public void NotifyTurnStarted()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        _isSettingTurn = false;
    }

    public int GetPlayerRank(int viewID)
    {
        return _playerRankings.TryGetValue(viewID, out int rank) ? rank : -1;
    }

    #region Player Death
    void HandleDeath(GamePlayer player)
    {
        int deadIndex = _players.IndexOf(player);
        if (deadIndex < 0)
            return;

        _playerRankings[player.ViewID] = _currentRank;
        photonView.RPC(nameof(RPC_UpdateRanking), RpcTarget.All, player.ViewID, _currentRank);
        _currentRank--;

        _players.RemoveAt(deadIndex);

        if (_currentPlayerIndex >= _players.Count)
            _currentPlayerIndex = 0;

        photonView.RPC(nameof(RPC_RemovePlayer), RpcTarget.Others, player.ViewID);

        if (_players.Count == 1)
        {
            _playerRankings[_players[0].ViewID] = 1;
            photonView.RPC(nameof(RPC_UpdateRanking), RpcTarget.All, _players[0].ViewID, 1);
            _players[0].Win();
        }
    }

    public void DiePlayer(GamePlayer player)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            photonView.RPC(nameof(RPC_DiePlayer), RpcTarget.MasterClient, player.ViewID);
            return;
        }
        HandleDeath(player);
    }
    #endregion

    #region RPCs
    [PunRPC]
    void RPC_SetPlayersList(int[] viewIDs)
    {
        _players = viewIDs.Select(id => PhotonView.Find(id).GetComponent<GamePlayer>()).OrderBy(p => p.TurnIndex).ToList();
    }

    [PunRPC]
    void RPC_NextTurn()
    {
        if (PhotonNetwork.IsMasterClient)
            NextTurn();
    }

    [PunRPC]
    void RPC_SetCurrentTurn(int playerIndex)
    {
        _currentPlayerIndex = playerIndex;
        _isSettingTurn = false;

        for (int i = 0; i < _players.Count; i++)
        {
            GamePlayer player = _players[i];
            bool isTurn = (i == _currentPlayerIndex);
            player.SetMyTurn(isTurn);

            if (isTurn && player.photonView.IsMine)
                player.StartTurn();
        }
    }

    [PunRPC]
    void RPC_DiePlayer(int viewID)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        GamePlayer player = PhotonView.Find(viewID).GetComponent<GamePlayer>();
        if (player != null)
            HandleDeath(player);
    }

    [PunRPC]
    void RPC_RemovePlayer(int viewID)
    {
        GamePlayer player = PhotonView.Find(viewID).GetComponent<GamePlayer>();
        if (player != null)
            _players.Remove(player);
    }

    [PunRPC]
    void RPC_UpdateRanking(int viewID, int rank)
    {
        _playerRankings[viewID] = rank;

        GamePlayer player = PhotonView.Find(viewID)?.GetComponent<GamePlayer>();
        if (player != null)
        {
            player.SetRank(rank);
        }

        if (_playerRankings.Count == PhotonNetwork.CurrentRoom.PlayerCount)
        {
            OnGameEnd?.Invoke();
        }
    }
    #endregion
}