using UnityEngine;
using UnityEngine.U2D;

public class LiarBarCardInfoUI : MonoBehaviour
{
    [SerializeField] SpriteRenderer _targetCardSprite;

    Animator _animator;

    void Start()
    {
        _animator = GetComponent<Animator>();
        TurnManager.Instance.OnEndRegisterPlayer += StartCardInfoAnimation;
        LiarBarCardManager.Instance.OnSetTableAction += ShowTableCard;
    }

    void StartCardInfoAnimation()
    {
        _animator.SetBool("isStartGame", true);
    }

    void ShowTableCard()
    {
        ELiarBarCardType targetCard = LiarBarCardManager.Instance.TargetCard;
        _targetCardSprite.sprite = LiarBarCardManager.Instance.GetCardSprite(targetCard);
        _animator.SetTrigger("doShowTargetCard");
    }

    #region Animation Event
    public void EndStartGameEvent()
    {
        LiarBarCardManager.Instance.SetTable();
    }

    public void EndTargetCardEvent()
    {
        TurnManager.Instance.ContinueGame();
    }
    #endregion
}