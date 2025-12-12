using System;
using System.Collections;
using Photon.Pun;
using UnityEngine;
using Random = UnityEngine.Random;

public class LiarBarCard : MonoBehaviourPun
{
    [SerializeField] SpriteRenderer _cardRenderer;
    ELiarBarCardType _cardType;

    Action onComplete;

    public ELiarBarCardType CardType { get => _cardType; }

    void DestroyCard()
    {
        PhotonNetwork.Destroy(gameObject);
    }

    public void Init(ELiarBarCardType type)
    {
        _cardType = type;
    }

    public void MoveToTable(Vector3 targetPosition, float duration = 0.5f)
    {
        if (photonView.IsMine)
        {
            // 모든 클라이언트에서 코루틴 실행
            photonView.RPC("RPC_MoveToTable", RpcTarget.All, targetPosition, duration);
            photonView.RPC("RPC_SetCardSprite", RpcTarget.AllBuffered, (int)_cardType);
        }
    }

    public void Flip(Vector3 targetPosition, System.Action onComplete = null)
    {
        this.onComplete = onComplete;
        photonView.RPC("RPC_FlipCard", RpcTarget.All, targetPosition, onComplete != null);
    }

    #region Coroutines
    IEnumerator MoveCoroutine(Vector3 target, float duration)
    {
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        Vector3 startScale = transform.localScale;
        Vector3 targetScale = new Vector3(2f, 2f, 1f);

        float randomY = Random.Range(-40f, 40f);
        Quaternion targetRot = Quaternion.Euler(0f, randomY, 0f);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            transform.position = Vector3.Lerp(startPos, target, t);
            transform.rotation = Quaternion.Lerp(startRot, targetRot, t);
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = target;
        transform.rotation = targetRot;
        transform.localScale = targetScale;
    }

    IEnumerator MoveAndFlipCoroutine(Vector3 targetPosition, bool hasCallback)
    {
        Vector3 startPos = transform.position;
        float elapsed = 0f;
        float moveDuration = 0.5f;

        while (elapsed < moveDuration)
        {
            float t = elapsed / moveDuration;
            transform.position = Vector3.Lerp(startPos, targetPosition, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
        yield return StartCoroutine(FlipCoroutine(hasCallback));

        if (hasCallback)
            onComplete?.Invoke(); // Flip 끝난 후 호출
    }

    IEnumerator FlipCoroutine(bool hasCallback, float duration = 0.5f)
    {
        float elapsed = 0f;
        Quaternion startRot = transform.rotation;
        Quaternion endRot = Quaternion.Euler(0f, 0f, 180f);

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            transform.rotation = Quaternion.Lerp(startRot, endRot, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = endRot;

        if (hasCallback)
            onComplete?.Invoke();
        Invoke("DestroyCard", 1.3f);
    }
    #endregion

    #region RPCs
    [PunRPC]
    void RPC_MoveToTable(Vector3 targetPosition, float duration)
    {
        StartCoroutine(MoveCoroutine(targetPosition, duration));
    }

    [PunRPC]
    void RPC_FlipCard(Vector3 targetPosition, bool hasCallback)
    {
        StartCoroutine(MoveAndFlipCoroutine(targetPosition, hasCallback));
    }

    [PunRPC]
    void RPC_SetCardSprite(int cardType)
    {
        _cardType = (ELiarBarCardType)cardType;
        _cardRenderer.sprite = LiarBarCardManager.Instance.GetCardSprite(_cardType);
        if (LiarBarCardManager.Instance.TargetCard == _cardType || _cardType == ELiarBarCardType.JokerCard)
            _cardRenderer.color = Color.lightGreen;
        else
            _cardRenderer.color = Color.softRed;
    }
    #endregion
}