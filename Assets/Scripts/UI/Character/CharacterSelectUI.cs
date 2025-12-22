using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using Unity.Services.Vivox;
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

    bool _isReady = false;
    bool _hasInitialized = false; // 초기화 플래그 추가

    FirebaseREST _firebaseRest;
    PhotonManager _photonManager;

    void Start()
    {
        _photonManager = MonoSingleton<PhotonManager>.Instance;
        _firebaseRest = MonoSingleton<FirebaseREST>.Instance;

        ShowRoomID();

        // Start에서 초기화 시도 (Firebase 로드 완료 후)
        InitializePlayer();
    }

    void InitializePlayer()
    {
        // 이미 초기화했으면 무시
        if (_hasInitialized)
            return;

        // Firebase User가 null이면 대기
        if (_firebaseRest.User == null)
        {
            Debug.Log("Firebase User가 아직 로드되지 않았습니다. 0.5초 후 재시도...");
            Invoke(nameof(InitializePlayer), 0.5f);
            return;
        }

        string nickname = _firebaseRest.User.GetField<string>("nickname");

        // 닉네임이 비어있으면 대기
        if (string.IsNullOrEmpty(nickname))
        {
            Debug.Log("닉네임이 비어있습니다. Firebase 로드 대기 중...");
            Invoke(nameof(InitializePlayer), 0.5f);
            return;
        }

        // 초기화 완료
        _hasInitialized = true;
        Debug.Log($"플레이어 초기화 완료: {nickname}");

        _photonManager.SetNickname(nickname);
        AssignRandomPositionIndex();
        UpdateAllPlayerUI();
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
                PositionIndex = GetCustomProperty(player, "PositionIndex", 0),
                ActorNumber = player.ActorNumber,
                IsAlive = GetCustomProperty(player, "IsAlive", true)
            };

            _playerSlots[i].SetSlot(info);
        }
    }

    T GetCustomProperty<T>(Player player, string key, T defaultValue)
    {
        if (player.CustomProperties.TryGetValue(key, out object value) && value is T typedValue)
            return typedValue;
        return defaultValue;
    }

    void CheckAllPlayerReady()
    {
        // 1. 게임 모드가 선택되었는지 확인
        if (!PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("GameMode", out object gameModeObj))
            return;

        // 2. 인원 체크
        if (PhotonNetwork.PlayerList.Length != 4)
            return;

        // 3. 모든 플레이어 Ready 체크
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

        // 4. 모든 조건 만족 → 게임 시작
        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.LoadLevel("IngameScene");
    }

    void AssignRandomPositionIndex()
    {
        // 이미 다른 플레이어가 사용중인 인덱스 수집
        HashSet<int> usedIndexes = new HashSet<int>();
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            int posIndex = GetCustomProperty(player, "PositionIndex", -1);
            if (posIndex >= 0)
                usedIndexes.Add(posIndex);
        }

        int maxPlayers = PhotonNetwork.CurrentRoom.MaxPlayers;
        List<int> availableIndexes = new List<int>();
        for (int i = 0; i < maxPlayers; i++)
        {
            if (!usedIndexes.Contains(i))
                availableIndexes.Add(i);
        }

        if (availableIndexes.Count > 0)
        {
            int chosenIndex = availableIndexes[Random.Range(0, availableIndexes.Count)];
            _photonManager.SetPositionIndex(chosenIndex);
        }
    }

    #region UI Event
    public void OnRegisterNickname(string nickname)
    {
        _photonManager.SetNickname(nickname);
        _nicknameInputField.SetActive(false);
        _selectCharacterUI.SetActive(true);
        AssignRandomPositionIndex();
        UpdateAllPlayerUI();
    }

    public void OnClickExitRoom()
    {
        _photonManager.ExitRoom();
    }

    public void OnClickReadyButton()
    {
        _isReady = !_isReady;
        _photonManager.SetReady(_isReady);
        UpdateAllPlayerUI();
    }

    public void OnClickVoiceToggle()
    {
        // 비복스 음소거 토글
    }
    #endregion

    #region Photon Callbacks
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        UpdateAllPlayerUI();
        CheckAllPlayerReady();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        _photonManager.PlayerInfo.Remove(otherPlayer.ActorNumber);
        UpdateAllPlayerUI();
    }

    public override void OnJoinedRoom()
    {
        // OnJoinedRoom은 초기화만 트리거
        Debug.Log("OnJoinedRoom 호출됨");
        InitializePlayer();
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("LobbyScene");
    }
    #endregion
}