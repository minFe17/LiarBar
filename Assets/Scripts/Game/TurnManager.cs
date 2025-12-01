using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;

public class TurnManager : MonoBehaviourPun
{
    public static TurnManager Instance { get; private set; }

    List<GamePlayer> _players = new List<GamePlayer>();
    int _currentPlayerIndex = 0;

    public int CurrentPlayerIndex { get => _currentPlayerIndex; }

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

    #region Player 등록
    public void RegisterPlayer(GamePlayer player)
    {
        if (!_players.Contains(player))
            _players.Add(player);

        _players = _players.OrderBy(p => p.TurnIndex).ToList();
    }
    #endregion

    #region Turn Control
    void NextTurn()
    {
        _currentPlayerIndex++;
        if (_currentPlayerIndex >= _players.Count)
            _currentPlayerIndex = 0;

        photonView.RPC("RPC_SetCurrentTurn", RpcTarget.All, _currentPlayerIndex);
    }

    public void EndTurn()
    {
        photonView.RPC("RPC_NextTurn", RpcTarget.MasterClient);
    }

    public void ContinueGame(int playerIndex)
    {
        photonView.RPC("RPC_ContinueGame", RpcTarget.MasterClient, playerIndex);
    }
    #endregion

    #region Player Death
    void HandleDeath(GamePlayer player)
    {
        int deadIndex = _players.IndexOf(player);
        if (deadIndex < 0)
            return;

        _players.RemoveAt(deadIndex);

        // 죽은 플레이어가 현재 턴 주인보다 앞에 있으면 인덱스 보정
        if (deadIndex < _currentPlayerIndex)
            _currentPlayerIndex--;

        // 죽은 플레이어가 현재 턴 주인이라면 다음 살아있는 플레이어로 이동
        if (_currentPlayerIndex >= _players.Count)
            _currentPlayerIndex = 0;

        // 다른 클라이언트에서도 제거 반영
        photonView.RPC("RPC_RemovePlayer", RpcTarget.Others, player.ViewID);

        if (_players.Count == 1)
        {
            GamePlayer winner = _players[0];
            Debug.Log($"Player {winner.TurnIndex} 승리!");
            // 여기서 승리 UI 표시나 게임 종료 로직 호출
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
    void RPC_NextTurn()
    {
        if (PhotonNetwork.IsMasterClient)
            NextTurn();
    }

    [PunRPC]
    void RPC_SetCurrentTurn(int playerIndex)
    {
        if (_players.Count == 0 || playerIndex >= _players.Count)
            return;

        GamePlayer currentPlayer = _players[playerIndex];
        currentPlayer.StartTurn();
    }

    [PunRPC]
    void RPC_ContinueGame(int playerIndex)
    {
        if (_players.Count == 0 || playerIndex >= _players.Count)
            return;

        _currentPlayerIndex = playerIndex;
        photonView.RPC("RPC_SetCurrentTurn", RpcTarget.All, _currentPlayerIndex);
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
    #endregion
}