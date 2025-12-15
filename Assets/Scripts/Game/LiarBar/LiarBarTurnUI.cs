using System.Collections;
using Photon.Pun;
using UnityEngine;

public class LiarBarTurnUI : MonoBehaviourPun
{
    void Awake()
    {
        gameObject.SetActive(false);
    }

    void RotateToPlayer(GamePlayer player)
    {
        Vector3 dir = player.transform.position - transform.position;
        float angle = Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg;

        // 모든 클라이언트에 회전 RPC
        photonView.RPC("RPC_RotateBar", RpcTarget.All, angle);
    }

    public void ShowNextPlayer(GamePlayer player)
    {
        if (!gameObject.activeInHierarchy)
            gameObject.SetActive(true);

        if (PhotonNetwork.IsMasterClient)
        {
            // 마스터 클라에서만 계산
            Vector3 dir = player.transform.position - transform.position;
            float angle = Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg;

            // 모든 클라이언트에게 전달
            photonView.RPC("RPC_RotateBar", RpcTarget.All, angle);
        }
    }

    IEnumerator RotateToAngleRoutine(float targetAngle, float duration = 0.5f)
    {
        Quaternion startRot = transform.rotation;
        Quaternion endRot = Quaternion.Euler(90f, 0f, targetAngle);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            transform.rotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }

        transform.rotation = endRot;
    }

    [PunRPC]
    void RPC_RotateBar(float angle)
    {
        gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(RotateToAngleRoutine(angle));
    }
}