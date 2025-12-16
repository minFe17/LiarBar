using System;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;

public class TurnManager : MonoBehaviourPun
{
    public static TurnManager Instance { get; private set; }

    List<GamePlayer> _players = new List<GamePlayer>();
    int _currentPlayerIndex = 0;
    int _nextRoundStartIndex = 0;

    bool _isResolvingLiar = false;
    bool _isSettingTurn = false;

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

    void Start()
    {
        LiarBarCardManager.Instance.OnSetTableAction += StartGame;
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
        if(_currentPlayerIndex >= _players.Count)
            _currentPlayerIndex = 0;

        photonView.RPC(nameof(RPC_SetCurrentTurn), RpcTarget.All, _currentPlayerIndex);
    }

    public void SetNextRoundStartPlayer(int turnIndex)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        // TurnIndex를 _players 리스트의 인덱스로 변환
        _nextRoundStartIndex = _players.FindIndex(p => p.TurnIndex == turnIndex);

        // 찾지 못한 경우 현재 플레이어 인덱스 유지
        if (_nextRoundStartIndex < 0)
            _nextRoundStartIndex = _currentPlayerIndex;
    }

    public void NotifyTurnStarted()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        _isSettingTurn = false;
    }

    void HandleDeath(GamePlayer player)
    {
        int deadIndex = _players.IndexOf(player);
        if (deadIndex < 0)
            return;

        _players.RemoveAt(deadIndex);

        if (_currentPlayerIndex >= _players.Count)
            _currentPlayerIndex = 0;

        photonView.RPC(nameof(RPC_RemovePlayer), RpcTarget.Others, player.ViewID);

        if (_players.Count == 1)
            _players[0].Win();
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

        // 모든 플레이어의 턴 상태를 업데이트
        for (int i = 0; i < _players.Count; i++)
        {
            GamePlayer player = _players[i];
            bool isTurn = (i == _currentPlayerIndex);
            player.SetMyTurn(isTurn);

            // 자기 턴이면서 자신의 플레이어인 경우에만 StartTurn 호출
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
    #endregion
}