using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Utils;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    Dictionary<int, PlayerInfo> _playerInfo = new Dictionary<int, PlayerInfo>();

    public string RoomID { get => PhotonNetwork.CurrentRoom.Name; }
    public string Nickname { get => PhotonNetwork.LocalPlayer.NickName; }
    public Dictionary<int, PlayerInfo> PlayerInfo { get => _playerInfo; }

    #region Photon Connection
    public void ConnectToPhoton()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        if (!PhotonNetwork.IsConnected)
            PhotonNetwork.ConnectUsingSettings();

        // 네트워크 속도 세팅
        PhotonNetwork.SendRate = 80;          // 1초에 60번 전송
        PhotonNetwork.SerializationRate = 40; // 1초에 30번 직렬화
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon!");
        PhotonNetwork.JoinLobby();
    }
    #endregion

    #region Room
    public void CreateRoom(string roomID, RoomOptions options)
    {
        PhotonNetwork.CreateRoom(roomID, options);
    }

    public void JoinRoom(string roomID)
    {
        PhotonNetwork.JoinRoom(roomID);
    }

    public void SetNickname(string nickname)
    {
        PhotonNetwork.NickName = nickname;

        Hashtable props = new Hashtable { { "Nickname", nickname } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    public void ExitRoom()
    {
        PhotonNetwork.LeaveRoom();
    }
    #endregion

    #region Player Info
    public void UpdateLocalPlayerInfo()
    {
        Hashtable props = new Hashtable
        {
            { "Nickname", PhotonNetwork.NickName },
            { "IsReady", false },
            { "SelectedCharacterIndex", 0 },
            { "PositionIndex", 0 }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    public void SetReady(bool isReady)
    {
        Hashtable props = new Hashtable { { "IsReady", isReady } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    public void SetCharacterIndex(int index)
    {
        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { "SelectedCharacterIndex", index } });
    }
    public void SetPositionIndex(int index)
    {
        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { "PositionIndex", index } });
    }
    #endregion

    #region Photon Callbacks
    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.LoadLevel("SelectCharacterScene");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        SimpleSingleton<NotifyManager>.Instance.Notify("방 입장 실패: ", returnCode);
    }
    #endregion
}