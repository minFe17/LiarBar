using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class PlayerSlotUI : MonoBehaviour
{
    [SerializeField] int _slotIndex;
    [SerializeField] Text _nicknameText;
    [SerializeField] Image _characterImage;
    [SerializeField] GameObject _readyIcon;
    [SerializeField] GameObject _pageButton;

    Sprite GetCharacterSprite(int index)
    {
        // 캐릭터 스프라이트 반환
        return null;
    }

    public void UpdateUI()
    {
        PhotonManager photonManager = MonoSingleton<PhotonManager>.Instance;

        if (photonManager.PlayerInfo.TryGetValue(_slotIndex + 1, out PlayerInfo info))
        {
            _nicknameText.text = info.Nickname;
            _readyIcon.SetActive(info.IsReady);
            _characterImage.sprite = GetCharacterSprite(info.SelectedCharacterIndex);
        }
    }
  
    public void SetSlot(PlayerInfo info)
    {
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        _nicknameText.text = info.Nickname;
        _readyIcon.SetActive(info.IsReady);

        // 스프라이트는 따로 필요
        //_characterImage.sprite = GetCharacterSprite(info.SelectedCharacterIndex);

        _pageButton.SetActive(info.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber);
    }

    public void ClearSlot()
    {
        _nicknameText.text = "";
        _characterImage.sprite = null;
        gameObject.SetActive(false);
    }
}