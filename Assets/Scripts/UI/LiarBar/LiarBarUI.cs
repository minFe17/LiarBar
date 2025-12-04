using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class LiarBarUI : MonoBehaviour
{
    [Header("Table Info")]
    [SerializeField] Text _cardText;
    [SerializeField] Image _cardImage;

    SpriteAtlas _cardAtlas;

    void Start()
    {
        LiarBarCardManager.Instance.OnSetTableAction += UpdateTargetCard;
        _cardAtlas = Resources.Load<SpriteAtlas>("SpriteAtlas/LiarBarCardAtlas");
    }

    void UpdateTargetCard()
    {
        ELiarBarCardType targetCard = LiarBarCardManager.Instance.TargetCard;
        string cardStr = targetCard.ToString();
        _cardImage.sprite = _cardAtlas.GetSprite(cardStr);
        _cardText.text = $"{cardStr[0]} TABLE";
    }
}