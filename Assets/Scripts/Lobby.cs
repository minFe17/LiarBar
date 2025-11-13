using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Lobby : MonoBehaviourPunCallbacks
{
    [Header("UI")]
    [SerializeField] GameObject _roomIdInputField;

    void Start()
    {
        ConnectToPhoton();
        HideRoomInput();
    }

    #region Photon Connection
    private void ConnectToPhoton()
    {
        if (!PhotonNetwork.IsConnected)
            PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon!");
        PhotonNetwork.JoinLobby();
    }
    #endregion

    #region UI Events
    public void OnClickCreateRoom()
    {
        string roomName = GenerateRoomName();
        RoomOptions options = new RoomOptions { MaxPlayers = 4 };
        PhotonNetwork.CreateRoom(roomName, options);
        Debug.Log($"방 생성 시도 : {roomName}");
    }

    public void OnClickEnterRoom()
    {
        ShowRoomInput();
    }

    public void OnRoomIdEntered(string roomId)
    {
        if (string.IsNullOrEmpty(roomId))
        {
            Debug.LogWarning("방 ID를 입력하세요.");
            return;
        }

        PhotonNetwork.JoinRoom(roomId);
        Debug.Log($"방 입장 시도: {roomId}");
        HideRoomInput();
    }
    #endregion

    #region Helper
    private string GenerateRoomName() => Random.Range(1000, 9999).ToString();

    private void ShowRoomInput() => _roomIdInputField.SetActive(true);
    private void HideRoomInput() => _roomIdInputField.SetActive(false);
    #endregion

    #region Photon Callbacks
    public override void OnCreatedRoom()
    {
        Debug.Log("방 생성 완료: " + PhotonNetwork.CurrentRoom.Name);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"방 생성 실패: {message}");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("방 입장 성공: " + PhotonNetwork.CurrentRoom.Name);
        SceneManager.LoadScene("SelectCharacterScene");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"방 입장 실패: {message}");
    }
    #endregion
}