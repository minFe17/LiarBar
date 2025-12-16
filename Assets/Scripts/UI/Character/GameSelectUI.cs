using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class GameSelectUI : MonoBehaviourPun
{
    [SerializeField] List<Button> _buttons;

    List<Image> _images = new List<Image>();

    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
            return;
        for(int i=0; i< _buttons.Count; i++)
        {
            _buttons[i].enabled = false;
            _images.Add(_buttons[i].GetComponent<Image>());
            _images[i].color = _buttons[i].colors.normalColor;
        }
    }

    public void ClickGameMode(int gameMode)
    {
        if(!PhotonNetwork.IsMasterClient)
            return;
        EGameMode gameModeType = (EGameMode)gameMode;
        SimpleSingleton<GameModeManager>.Instance.GameModeType = gameModeType;
        photonView.RPC(nameof(RPC_ChangeGameMode), RpcTarget.Others, gameMode);
        // 플레이어 중에 돈이 0인 사람 있으면 입장불가? or return?
    }

    [PunRPC]
    void RPC_ChangeGameMode(int gameMode)
    {
        EGameMode gameModeType = (EGameMode)gameMode;
        SimpleSingleton<GameModeManager>.Instance.GameModeType = gameModeType;

        for(int i=0; i<_buttons.Count; i++)
        {
            if(i == (int)gameModeType)
                _images[i].color = _buttons[i].colors.selectedColor;
            else
                _images[i].color = _buttons[i].colors.normalColor;
        }
    }
}