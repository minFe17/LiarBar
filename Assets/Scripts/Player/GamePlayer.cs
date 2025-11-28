using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public class GamePlayer : MonoBehaviour
{
    PhotonView _photonView;
    public int TurnIndex { get; set; }

    public int ViewID => _photonView.ViewID;

    private void Awake()
    {
        _photonView = GetComponent<PhotonView>();
    }

    private void Start()
    {
        // 씬에 이미 있는 TurnManager 싱글턴 사용
        TurnManager.Instance.RegisterPlayer(this);
    }

    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            TurnManager.Instance.EndTurn();
        }
    }

    public void StartTurn()
    {
        Debug.Log($"Player {TurnIndex} 시작!");
        // 여기서 턴 시작 로직 실행
    }
}
