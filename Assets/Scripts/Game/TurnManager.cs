using ExitGames.Client.Photon;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TurnManager : MonoBehaviourPun
{
    public static TurnManager Instance { get; private set; }

    List<GamePlayer> _players = new List<GamePlayer>();
    int _currentPlayerIndex = 0;
    int _nextRoundStartIndex = 0;

    bool _isResolvingLiar = false;   // 라이어 판정 중
    bool _isSettingTurn = false;     // 턴 중복 세팅 방지 락

    public Action OnEndRegisterPlayer;

    public IReadOnlyList<GamePlayer> Players => _players;
    public int CurrentPlayerIndex => _currentPlayerIndex;

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

    void OnEnable()
    {
        if (LiarBarCardManager.Instance != null)
            LiarBarCardManager.Instance.OnSetTableAction += StartGame;
    }

    void OnDisable()
    {
        if (LiarBarCardManager.Instance != null)
            LiarBarCardManager.Instance.OnSetTableAction -= StartGame;
    }

    #region Player 등록
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
            photonView.RPC("RPC_SetPlayersList", RpcTarget.All, viewIDs);
            photonView.RPC("RPC_SetCurrentTurn", RpcTarget.All, 0);

            OnEndRegisterPlayer?.Invoke();
        }
    }
    #endregion

    void StartGame()
    {
        photonView.RPC("RPC_SetCurrentTurn", RpcTarget.All, _currentPlayerIndex);
    }

    #region Turn Control
    void NextTurn()
    {
        if (_isSettingTurn)
            return;

        _isSettingTurn = true;

        _currentPlayerIndex++;
        if (_currentPlayerIndex >= _players.Count)
            _currentPlayerIndex = 0;

        photonView.RPC("RPC_SetCurrentTurn", RpcTarget.All, _currentPlayerIndex);
    }

    public void EndTurn()
    {
        if (_isResolvingLiar)
            return;

        photonView.RPC("RPC_NextTurn", RpcTarget.MasterClient);
    }

    public void ContinueGame()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (_isSettingTurn)
            return;

        _isSettingTurn = true;
        _isResolvingLiar = false;

        _currentPlayerIndex = _nextRoundStartIndex;
        photonView.RPC("RPC_SetCurrentTurn", RpcTarget.All, _currentPlayerIndex);
    }

    public void SetNextRoundStartPlayer(int playerIndex)
    {
        photonView.RPC("RPC_SetNextRoundStartPlayer", RpcTarget.MasterClient, playerIndex);
    }

    public void BeginResolveLiar()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        _isResolvingLiar = true;
    }

    public void NotifyTurnStarted()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        _isSettingTurn = false;
    }
    #endregion

    #region Player Death
    void HandleDeath(GamePlayer player)
    {
        int deadIndex = _players.IndexOf(player);
        if (deadIndex < 0)
            return;

        _players.RemoveAt(deadIndex);

        Hashtable props = new Hashtable();
        props["IsAlive"] = false;
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        if (_currentPlayerIndex >= _players.Count)
            _currentPlayerIndex = 0;

        photonView.RPC("RPC_RemovePlayer", RpcTarget.Others, player.ViewID);

        if (_players.Count == 1)
        {
            GamePlayer winner = _players[0];
            Debug.Log($"Player {winner.TurnIndex} 승리!");
            winner.Win();
        }
    }

    public void DiePlayer(GamePlayer player)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("RPC_DiePlayer", RpcTarget.MasterClient, player.ViewID);
            return;
        }
        HandleDeath(player);
    }
    #endregion

    #region RPC
    [PunRPC]
    void RPC_SetPlayersList(int[] viewIDs)
    {
        _players = viewIDs
            .Select(id => PhotonView.Find(id).GetComponent<GamePlayer>())
            .OrderBy(p => p.TurnIndex)
            .ToList();
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

        GamePlayer currentPlayer = _players[playerIndex];

        if (currentPlayer.photonView.IsMine)
            currentPlayer.StartTurn();
    }

    [PunRPC]
    void RPC_DiePlayer(int viewID)
    {
        GamePlayer player = PhotonView.Find(viewID).GetComponent<GamePlayer>();
        if (player != null && PhotonNetwork.IsMasterClient)
            HandleDeath(player);
    }

    [PunRPC]
    void RPC_RemovePlayer(int viewID)
    {
        GamePlayer player = PhotonView.Find(viewID).GetComponent<GamePlayer>();
        if (player != null && _players.Contains(player))
            _players.Remove(player);
    }

    [PunRPC]
    void RPC_SetNextRoundStartPlayer(int playerIndex)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        _nextRoundStartIndex = playerIndex;
    }
    #endregion
}
