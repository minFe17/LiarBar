using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class PlayerSlotUI : MonoBehaviour
{
    [SerializeField] int _slotIndex;
    [SerializeField] Text _nicknameText;
    [SerializeField] GameObject _readyIcon;
    [SerializeField] GameObject _pageButton;
    [SerializeField] CharacterList _characterList;

    public void SetSlot(PlayerInfo info)
    {
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        _nicknameText.text = info.Nickname;
        _readyIcon.SetActive(info.IsReady);

        _characterList.ShowCharacter(info.SelectedCharacterIndex);
        _pageButton.SetActive(info.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber);
    }

    public void ClearSlot()
    {
        _nicknameText.text = "";
        gameObject.SetActive(false);
    }

    #region Button Event
    public void OnClickChangeCharacter(int direction)
    {
        if (_readyIcon.activeSelf)
            return;
        int newIndex = _characterList.ChangeCharacter(direction);
        MonoSingleton<PhotonManager>.Instance.SetCharacterIndex(newIndex);
    }
    #endregion
}