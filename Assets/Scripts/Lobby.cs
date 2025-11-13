using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utils;

public class Lobby : MonoBehaviourPunCallbacks
{
    [Header("UI")]
    [SerializeField] InputField _roomIdInputField;

    Dictionary<short, string> _errorCodeDict = new Dictionary<short, string>();

    void Start()
    {
        ConnectToPhoton();
        HideRoomInput();
        SetErrorCode();
    }

    void SetErrorCode()
    {
        _errorCodeDict.Add(32757, "방 가득 참");
        _errorCodeDict.Add(32758, "방 없음");
    }

    #region Photon Connection
    void ConnectToPhoton()
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
        _roomIdInputField.text = "";
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

    public void ExitGame()
    {
        Application.Quit();
    }
    #endregion

    #region Helper
    private string GenerateRoomName() => Random.Range(1000, 9999).ToString();

    private void ShowRoomInput() => _roomIdInputField.gameObject.SetActive(true);
    private void HideRoomInput() => _roomIdInputField.gameObject.SetActive(false);
    #endregion

    #region Photon Callbacks
    public override void OnCreatedRoom()
    {
        Debug.Log("방 생성 완료: " + PhotonNetwork.CurrentRoom.Name);
    }

    public override void OnJoinedRoom()
    {
        SceneManager.LoadScene("SelectCharacterScene");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        SimpleSingleton<NotifyManager>.Instance.Notify($"방 입장 실패: {_errorCodeDict[returnCode]}");
    }
    #endregion
}