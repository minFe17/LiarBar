using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.InputSystem;

public class GamePlayer : MonoBehaviourPun
{
    public int TurnIndex { get; private set; }
    public int ViewID => photonView.ViewID;

    public PhotonView PhotonView => photonView;

    void Start()
    {
        TurnManager.Instance.RegisterPlayer(this);
        SetTurnIndex();
    }

    void Update()
    {
        if (!photonView.IsMine)
            return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
            TurnManager.Instance.EndTurn();
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
        // 턴 시작 로직
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
    }
}