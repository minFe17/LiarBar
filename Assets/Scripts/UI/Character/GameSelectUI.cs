using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class GameSelectUI : MonoBehaviourPunCallbacks
{
    [SerializeField] List<Button> _buttons;

    List<Image> _images = new List<Image>();

    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
            return;
        for (int i = 0; i < _buttons.Count; i++)
        {
            _buttons[i].enabled = false;
            _images.Add(_buttons[i].GetComponent<Image>());
            _images[i].color = _buttons[i].colors.normalColor;
        }

        LoadCurrentGameMode();
    }

    void LoadCurrentGameMode()
    {
        // Room CustomProperties에서 게임 모드 가져오기
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("GameMode", out object gameModeObj))
        {
            int gameMode = (int)gameModeObj;
            EGameMode gameModeType = (EGameMode)gameMode;

            UpdateUI(gameMode);
        }
    }

    public void ClickGameMode(int gameMode)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        EGameMode gameModeType = (EGameMode)gameMode;

        // Room CustomProperties에 저장
        Hashtable roomProps = new Hashtable
        {
            { "GameMode", gameMode }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);
    }

    void UpdateUI(int selectedMode)
    {
        if (PhotonNetwork.IsMasterClient)
            return;
        for (int i = 0; i < _buttons.Count; i++)
        {
            if (i == selectedMode)
                _images[i].color = _buttons[i].colors.selectedColor;
            else
                _images[i].color = _buttons[i].colors.normalColor;
        }
    }

    // Room Properties가 변경되면 자동으로 호출됨
    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.TryGetValue("GameMode", out object gameModeObj))
        {
            int gameMode = (int)gameModeObj;
            EGameMode gameModeType = (EGameMode)gameMode;

            UpdateUI(gameMode);
        }
    }
}