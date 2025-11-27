using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utils;

public class CharacterSelectUI : MonoBehaviourPunCallbacks
{
    [Header("Room ID")]
    [SerializeField] Text _roomIDText;

    [Header("Nickname Input")]
    [SerializeField] GameObject _nicknameInputField;

    [Header("Select Character UI")]
    [SerializeField] GameObject _selectCharacterUI;

    [Header("Player UI Slots")]
    [SerializeField] PlayerSlotUI[] _playerSlots;

    [Header("Voice")]
    [SerializeField] Toggle _voiceToggle;

    PhotonManager _photonManager;
    bool _isReady = false;

    void Start()
    {
        _photonManager = MonoSingleton<PhotonManager>.Instance;

        CheckNickname();
        ShowRoomID();

        UpdateAllPlayerUI();
    }

    void CheckNickname()
    {
        if (string.IsNullOrEmpty(_photonManager.Nickname))
        {
            _nicknameInputField.SetActive(true);
            _selectCharacterUI.SetActive(false);
        }
    }

    void ShowRoomID()
    {
        string roomID = _photonManager.RoomID;
        _roomIDText.text = $"Room ID : {roomID}";
    }

    void UpdateAllPlayerUI()
    {
        // 슬롯 초기화
        foreach (PlayerSlotUI slot in _playerSlots)
            slot.ClearSlot();

        // 현재 방에 있는 플레이어만 빈 슬롯 순서대로 채우기
        for (int i = 0; i < PhotonNetwork.PlayerList.Length && i < _playerSlots.Length; i++)
        {
            Player player = PhotonNetwork.PlayerList[i];

            PlayerInfo info = new PlayerInfo
            {
                Nickname = GetCustomProperty(player, "Nickname", player.NickName),
                IsReady = GetCustomProperty(player, "IsReady", false),
                SelectedCharacterIndex = GetCustomProperty(player, "SelectedCharacterIndex", 0),
                ActorNumber = player.ActorNumber
            };

            _playerSlots[i].SetSlot(info);
        }
    }

    // Generic helper 함수
    T GetCustomProperty<T>(Player player, string key, T defaultValue)
    {
        if (player.CustomProperties.TryGetValue(key, out object value) && value is T typedValue)
            return typedValue;
        return defaultValue;
    }

    void CheckAllPlayerReady()
    {
        if (PhotonNetwork.PlayerList.Length != 2) //멀티 인원
            return;

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            Player player = PhotonNetwork.PlayerList[i];

            PlayerInfo info = new PlayerInfo
            {
                IsReady = GetCustomProperty(player, "IsReady", false),
               
            };
            if (!info.IsReady)
                return;
        }
        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.LoadLevel("IngameScene");
    }

    #region UI Event
    public void OnRegisterNickname(string nickname)
    {
        _photonManager.SetNickname(nickname);
        _nicknameInputField.SetActive(false);
        _selectCharacterUI.SetActive(true);
    }

    public void OnClickExitRoom()
    {
        _photonManager.ExitRoom();
    }

    public void OnClickReadyButton()
    {
        // 토글
        _isReady = !_isReady;

        // Photon에 업데이트
        _photonManager.SetReady(_isReady);

        // 슬롯 UI 갱신
        UpdateAllPlayerUI();
    }

    public void OnClickVoiceToggle()
    {
        SimpleSingleton<VoiceManager>.Instance.ChangeVoiceChat(_voiceToggle.isOn);
    }
    #endregion

    #region Photon Callbacks
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        // 슬롯 전체 갱신
        UpdateAllPlayerUI();
        CheckAllPlayerReady();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        // PlayerInfo에서 제거
        _photonManager.PlayerInfo.Remove(otherPlayer.ActorNumber);

        // 슬롯 전체 갱신
        UpdateAllPlayerUI();
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("LobbyScene");
    }
    #endregion
}