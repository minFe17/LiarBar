using System.Collections;
using UnityEngine;

public class LiarBarCard : MonoBehaviour
{
    [SerializeField] SpriteRenderer _cardRenderer;
    ELiarBarCardType _cardType;

    public void Init(ELiarBarCardType type)
    {
        _cardType = type;
    }

    public void MoveToTable(Vector3 targetPosition, float duration = 0.5f)
    {
        StartCoroutine(MoveCoroutine(targetPosition, duration));
    }

    IEnumerator MoveCoroutine(Vector3 target, float duration)
    {
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        float randomY = Random.Range(-40f, 40f);
        Quaternion targetRot = Quaternion.Euler(0f, randomY, 0f);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            transform.position = Vector3.Lerp(startPos, target, t);
            transform.rotation = Quaternion.Lerp(startRot, targetRot, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = target;
        transform.rotation = targetRot;
    }
}