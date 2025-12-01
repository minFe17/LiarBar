using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;

public class TurnManager : MonoBehaviourPun
{
    public static TurnManager Instance { get; private set; }

    List<GamePlayer> _players = new List<GamePlayer>();
    int _currentPlayerIndex = 0;

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

    public void RegisterPlayer(GamePlayer player)
    {
        if (!_players.Contains(player))
            _players.Add(player);

        _players = _players.OrderBy(p => p.TurnIndex).ToList();
    }

    public void EndTurn()
    {
        photonView.RPC("RPC_NextTurn", RpcTarget.MasterClient);
    }

    void NextTurn()
    {
        _currentPlayerIndex++;
        if (_currentPlayerIndex >= _players.Count)
            _currentPlayerIndex = 0;

        photonView.RPC("RPC_SetCurrentTurn", RpcTarget.All, _currentPlayerIndex);
    }

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
    #endregion
}
