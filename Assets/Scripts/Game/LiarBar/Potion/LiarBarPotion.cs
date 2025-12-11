using System.Collections;
using Photon.Pun;
using UnityEngine;

public class LiarBarPotion : MonoBehaviour
{
    [SerializeField] MeshRenderer _potion;
    float _potionFill = 0.3f; 
    Coroutine _drinkCoroutine;

    Transform _handTransform;

    public void Init(Transform handTransform)
    {
        _handTransform = handTransform;
        ResetPotion();
    }

    void Update()
    {
        if (_handTransform != null)
        {
            transform.position = _handTransform.position;
            transform.rotation = _handTransform.rotation; // 필요하면 회전도 따라가게
        }
    }

    void ResetPotion()
    {
        // Shader Graph만 초기값으로
        _potion.material.SetFloat("_Fill", _potionFill);
    }

    public void DrinkPotion()
    {
        if (_drinkCoroutine != null)
            StopCoroutine(_drinkCoroutine);

        // 자기 화면에서 Fill 감소
        _drinkCoroutine = StartCoroutine(DrinkRoutine(-1f, _potionFill, 1.5f)); 
    }

    public void ThrowPotion()
    {
        StartCoroutine(ThrowRoutine(_handTransform));
    }

    IEnumerator DrinkRoutine(float target, float start, float duration)
    {
        float elapsed = 0f;
        float animatedFill = start; // 애니메이션용 변수
        PhotonView photonView = GetComponent<PhotonView>();

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            animatedFill = Mathf.Lerp(start, target, elapsed / duration);
            _potion.material.SetFloat("_Fill", animatedFill);
            photonView.RPC("RPC_SetFill", RpcTarget.Others, animatedFill);
            yield return null;
        }

        // 애니메이션 끝에서는 Shader만 target 값으로
        _potion.material.SetFloat("_Fill", target);

        // 끝났으면 다른 클라이언트에 동기화
        photonView.RPC("RPC_SetFill", RpcTarget.Others, target);
    }

    IEnumerator ThrowRoutine(Transform handTransform)
    {
        Vector3 startPos = handTransform.position;
        Vector3 targetPos = startPos + handTransform.right * 1.0f; // 오른쪽 방향으로 1m
        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            yield return null;
        }

        // 던진 후 포션 삭제
        PhotonNetwork.Destroy(gameObject);
    }

    [PunRPC]
    void RPC_SetFill(float fillValue)
    {
        _potion.material.SetFloat("_Fill", fillValue);
    }
}