using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.InputSystem;

public class GamePlayer : MonoBehaviour
{
    PhotonView _photonView;
    public int TurnIndex { get; private set; }

    public int ViewID => _photonView.ViewID;

    void Awake()
    {
        _photonView = GetComponent<PhotonView>();
    }

    void Start()
    {
        TurnManager.Instance.RegisterPlayer(this);

        SetTurnIndex();
    }

    void Update()
    {
        if (!_photonView.IsMine)
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
        if (!_photonView.IsMine)
            return; 
        if(TurnManager.Instance.CurrentPlayerIndex != TurnIndex)
            return;
    }

    public void Win()
    {

    }

    public void Die()
    {
        if (!_photonView.IsMine)
            return;

        TurnManager.Instance.DiePlayer(this);
        // firebase 기록도 여기서 해도 OK, 중복 발생 X
    }
}