using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public class LiarBarUI : MonoBehaviourPun
{
    [SerializeField] LiarBarTargetCardUI _targetCardUI;
    [SerializeField] LiarBarPlayerCardUI _playerCardUI;
    [SerializeField] LiarBarPotionUI _potionUI;
    [SerializeField] GameResultUI _gameResultUI;

    void Start()
    {
        _targetCardUI.Init();
        _playerCardUI.Init();
        _potionUI.Init();
    }

    #region Input System
    void OnShowCard(InputValue value)
    {
        if (!value.isPressed)
            return;
        _playerCardUI.ShowCard();
    }

    void OnSwitchCard(InputValue value)
    {
        float val = value.Get<float>();
        if (Mathf.Approximately(val, 0f))
            return;

        int direction = (int)Mathf.Sign(val);
        _playerCardUI.SwitchCard(direction);
    }

    void OnSelectCard(InputValue value)
    {
        if (!value.isPressed)
            return;

        _playerCardUI.SelectCard();
    }

    void OnThrowCard(InputValue value)
    {
        if (!value.isPressed)
            return;

        _playerCardUI.ThrowCard();
    }

    void OnCallLiar(InputValue value)
    {
        if (!value.isPressed)
            return;

        // 낸 카드가 없으면(라운드 시작) return
        _playerCardUI.CallLiar();
        photonView.RPC("RPC_CallLiar", RpcTarget.All);
    }
    #endregion

    [PunRPC]
    void RPC_CallLiar()
    {
        _playerCardUI.OffUI();
    }
}