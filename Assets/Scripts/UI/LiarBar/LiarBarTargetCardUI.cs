using UnityEngine;
using UnityEngine.UI;

public class LiarBarTargetCardUI : MonoBehaviour
{
    [SerializeField] Text _cardText;
    [SerializeField] Image _cardImage;

    public void Init()
    {
        LiarBarCardManager.Instance.OnSetTableAction += UpdateTargetCard;
    }

    void UpdateTargetCard()
    {
        ELiarBarCardType targetCard = LiarBarCardManager.Instance.TargetCard;
        string cardStr = targetCard.ToString();
        _cardImage.sprite = LiarBarCardManager.Instance.GetCardSprite(targetCard);
        _cardText.text = $"{cardStr[0]} TABLE";
    }
}