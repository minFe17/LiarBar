using System.Collections;
using Photon.Pun;
using UnityEngine;

public class LiarBarTurnUI : MonoBehaviourPun
{
    void Start()
    {
        TurnManager.Instance.ShowNextTrunPlayer += ShowNextPlayer;
    }

    void ShowNextPlayer()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            GamePlayer target = TurnManager.Instance.Players[TurnManager.Instance.CurrentPlayerIndex];

            // 마스터 클라이언트가 각도 계산
            Vector3 dir = target.transform.position - transform.position;
            float angle = Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg; 

            // 모든 클라이언트에 angle 전달
            photonView.RPC("RPC_RotateBar", RpcTarget.All, angle);
        }
    }

    [PunRPC]
    void RPC_RotateBar(float angle)
    {
        StartCoroutine(RotateToAngleRoutine(angle));
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
}
