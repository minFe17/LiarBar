using System.Collections.Generic;
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

    bool _isReady = false;
    bool _hasInitialized = false;

    FirebaseREST _firebaseRest;
    PhotonManager _photonManager;

    void Start()
    {
        _photonManager = MonoSingleton<PhotonManager>.Instance;
        _firebaseRest = MonoSingleton<FirebaseREST>.Instance;

        ShowRoomID();
        Invoke(nameof(TryInitialize), 0.5f);
    }

    void TryInitialize()
    {
        Debug.Log($"[TryInitialize] InRoom: {PhotonNetwork.InRoom}, State: {PhotonNetwork.NetworkClientState}");

        if (PhotonNetwork.InRoom && PhotonNetwork.NetworkClientState == ClientState.Joined)
        {
            InitializePlayer();
        }
        else
        {
            Debug.Log("[TryInitialize] 아직 방 입장 안 됨, 재시도...");
            Invoke(nameof(TryInitialize), 0.3f);
        }
    }

    void InitializePlayer()
    {
        Debug.Log($"[InitializePlayer] 호출됨. _hasInitialized: {_hasInitialized}");

        if (_hasInitialized)
        {
            Debug.Log("[InitializePlayer] 이미 초기화됨");
            return;
        }

        if (!PhotonNetwork.InRoom || PhotonNetwork.NetworkClientState != ClientState.Joined)
        {
            Debug.Log($"[InitializePlayer] 방 입장 대기 중... 현재 상태: {PhotonNetwork.NetworkClientState}");
            Invoke(nameof(InitializePlayer), 0.3f);
            return;
        }

        if (_firebaseRest.User == null)
        {
            Debug.Log("[InitializePlayer] Firebase User가 아직 로드되지 않았습니다. 0.3초 후 재시도...");
            Invoke(nameof(InitializePlayer), 0.3f);
            return;
        }

        string nickname = _firebaseRest.User.GetField<string>("nickname");

        if (string.IsNullOrEmpty(nickname))
        {
            Debug.Log("[InitializePlayer] 닉네임이 비어있습니다. Firebase 로드 대기 중...");
            Invoke(nameof(InitializePlayer), 0.3f);
            return;
        }

        _hasInitialized = true;
        Debug.Log($"[InitializePlayer] 플레이어 초기화 완료: {nickname}");

        _photonManager.SetNickname(nickname);

        if (PhotonNetwork.IsMasterClient)
        {
            // 마스터는 직접 할당
            AssignPositionIndexToPlayer(PhotonNetwork.LocalPlayer);
        }
        else
        {
            // 다른 플레이어는 RPC로 요청
            PhotonView pv = GetComponent<PhotonView>();
            if (pv == null)
            {
                // PhotonView가 없으면 추가
                GameObject go = GameObject.Find("CharacterSelectUI");
                if (go != null)
                    pv = go.GetComponent<PhotonView>();
            }

            if (pv != null && pv.ViewID != 0)
            {
                pv.RPC(nameof(RPC_RequestPositionIndex), RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber);
            }
            else
            {
                // PhotonView가 없으면 직접 할당 (fallback)
                Invoke(nameof(AssignRandomPositionIndex), 0.2f);
            }
        }
    }

    [PunRPC]
    void RPC_RequestPositionIndex(int actorNumber)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        Debug.Log($"[RPC_RequestPositionIndex] ActorNumber {actorNumber} 요청 받음");

        Player targetPlayer = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber);
        if (targetPlayer != null)
        {
            AssignPositionIndexToPlayer(targetPlayer);
        }
    }

    void AssignPositionIndexToPlayer(Player player)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        // 이미 PositionIndex가 있으면 무시
        if (player.CustomProperties.ContainsKey("PositionIndex"))
        {
            int existingIndex = (int)player.CustomProperties["PositionIndex"];
            Debug.Log($"[AssignPositionIndexToPlayer] {player.NickName}은 이미 PositionIndex {existingIndex}를 가지고 있음");
            UpdateAllPlayerUI();
            return;
        }

        // 사용 중인 인덱스 수집
        HashSet<int> usedIndexes = new HashSet<int>();
        foreach (Player p in PhotonNetwork.PlayerList)
        {
            int posIndex = GetCustomProperty(p, "PositionIndex", -1);
            if (posIndex >= 0)
            {
                usedIndexes.Add(posIndex);
                Debug.Log($"[AssignPositionIndexToPlayer] 사용 중: {posIndex} ({p.NickName})");
            }
        }

        // 사용 가능한 인덱스 찾기
        int maxPlayers = PhotonNetwork.CurrentRoom.MaxPlayers;
        List<int> availableIndexes = new List<int>();
        for (int i = 0; i < maxPlayers; i++)
        {
            if (!usedIndexes.Contains(i))
                availableIndexes.Add(i);
        }

        if (availableIndexes.Count > 0)
        {
            int chosenIndex = availableIndexes[0]; // 첫 번째 사용 가능한 인덱스
            Debug.Log($"[AssignPositionIndexToPlayer] {player.NickName}에게 PositionIndex {chosenIndex} 할당");

            Hashtable props = new Hashtable { { "PositionIndex", chosenIndex } };
            player.SetCustomProperties(props);

            Invoke(nameof(UpdateAllPlayerUI), 0.2f);
        }
        else
        {
            Debug.LogError($"[AssignPositionIndexToPlayer] 사용 가능한 PositionIndex가 없습니다!");
        }
    }

    void ShowRoomID()
    {
        string roomID = _photonManager.RoomID;
        _roomIDText.text = $"Room ID : {roomID}";
    }

    void UpdateAllPlayerUI()
    {
        Debug.Log($"[UpdateAllPlayerUI] 플레이어 수: {PhotonNetwork.PlayerList.Length}");

        foreach (PlayerSlotUI slot in _playerSlots)
            slot.ClearSlot();

        List<Player> sortedPlayers = new List<Player>(PhotonNetwork.PlayerList);
        sortedPlayers.Sort((a, b) =>
        {
            int posA = GetCustomProperty(a, "PositionIndex", -1);
            int posB = GetCustomProperty(b, "PositionIndex", -1);
            return posA.CompareTo(posB);
        });

        foreach (Player player in sortedPlayers)
        {
            int posIndex = GetCustomProperty(player, "PositionIndex", -1);

            if (posIndex >= 0 && posIndex < _playerSlots.Length)
            {
                PlayerInfo info = new PlayerInfo
                {
                    Nickname = GetCustomProperty(player, "Nickname", player.NickName),
                    IsReady = GetCustomProperty(player, "IsReady", false),
                    SelectedCharacterIndex = GetCustomProperty(player, "SelectedCharacterIndex", 0),
                    PositionIndex = posIndex,
                    ActorNumber = player.ActorNumber,
                    IsAlive = GetCustomProperty(player, "IsAlive", true)
                };

                _playerSlots[posIndex].SetSlot(info);
            }
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
        if (!PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("GameMode", out object gameModeObj))
            return;

        if (PhotonNetwork.PlayerList.Length != 4)
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

    void AssignRandomPositionIndex()
    {
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("PositionIndex"))
        {
            int myIndex = (int)PhotonNetwork.LocalPlayer.CustomProperties["PositionIndex"];
            Debug.Log($"[AssignRandomPositionIndex] 이미 PositionIndex가 할당됨: {myIndex}");
            UpdateAllPlayerUI();
            return;
        }

        HashSet<int> usedIndexes = new HashSet<int>();
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
                continue;

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
            int chosenIndex = availableIndexes[0];
            Debug.Log($"[AssignRandomPositionIndex] PositionIndex 할당: {chosenIndex}");
            _photonManager.SetPositionIndex(chosenIndex);
            Invoke(nameof(UpdateAllPlayerUI), 0.2f);
        }
    }

    #region UI Event
    public void OnRegisterNickname(string nickname)
    {
        _photonManager.SetNickname(nickname);
        _nicknameInputField.SetActive(false);
        _selectCharacterUI.SetActive(true);
        AssignRandomPositionIndex();
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
        VivoxController.Instance.ChangeVoiceChat(_voiceToggle.isOn);
    }
    #endregion

    #region Photon Callbacks
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        Debug.Log($"[OnPlayerPropertiesUpdate] {targetPlayer.NickName} 속성 변경됨");
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
        Debug.Log($"[OnJoinedRoom] 호출됨 - 상태: {PhotonNetwork.NetworkClientState}, IsMasterClient: {PhotonNetwork.IsMasterClient}");
        InitializePlayer();
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("LobbyScene");
    }
    #endregion
}