using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class Lobby : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] InputField _roomIDInputField;

    PhotonManager _photonManager;

    void Start()
    {
        _photonManager = MonoSingleton<PhotonManager>.Instance;
        _photonManager.ConnectToPhoton();
        HideRoomInput();
        SimpleSingleton<NotifyManager>.Instance.SetErrorCode();
    }

    #region UI Events
    public void OnClickCreateRoom()
    {
        string roomID = GenerateRoomName();
        RoomOptions options = new RoomOptions { MaxPlayers = 4 };
        _photonManager.CreateRoom(roomID, options);
    }

    public void OnClickEnterRoom()
    {
        _roomIDInputField.text = "";
        ShowRoomInput();
    }

    public void OnRoomIdEntered(string roomID)
    {
        if (string.IsNullOrEmpty(roomID))
            return;

        _photonManager.JoinRoom(roomID);
        HideRoomInput();
    }

    public void OnClickExitGame()
    {
        Application.Quit();
    }
    #endregion

    #region Helper
    private string GenerateRoomName() => Random.Range(1000, 9999).ToString();

    private void ShowRoomInput() => _roomIDInputField.gameObject.SetActive(true);
    private void HideRoomInput() => _roomIDInputField.gameObject.SetActive(false);
    #endregion
}