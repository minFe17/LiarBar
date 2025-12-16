using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class LiarBarCurrentCardUI : MonoBehaviour, IMediatorEvent
{
    [SerializeField] Text _playerText;
    [SerializeField] Text _cardText;

    void Start()
    {
        SimpleSingleton<MediatorManager>.Instance.Register(EMediatorEventType.UpdateThrowCardUI, this);
    }

    string GetNickname(GamePlayer player)
    {
        Player photonPlayer = player.PhotonView.Owner;

        if (photonPlayer.CustomProperties.TryGetValue("Nickname", out object nickname))
            return nickname.ToString();

        // Nickname이 없으면 기본 NickName 반환
        return photonPlayer.NickName;
    }

    void IMediatorEvent.HandleEvent(object data)
    {
        int count = (int)data;
        if (!_cardText.gameObject.activeSelf)
            _cardText.gameObject.SetActive(true);

        // 현재 턴 플레이어 (카드를 낸 사람)
        int currentIndex = TurnManager.Instance.CurrentPlayerIndex - 1;
        if (currentIndex < 0)
            currentIndex = TurnManager.Instance.Players.Count - 1;

        GamePlayer throwPlayer = TurnManager.Instance.Players[currentIndex];

        // 닉네임 가져오기
        string nickname = GetNickname(throwPlayer);
        _playerText.text = nickname;

        string cardType = LiarBarCardManager.Instance.TargetCard.ToString();
        char cardName = cardType[0];
        _cardText.text = $"{count} x {cardName} CARD";
    }
}