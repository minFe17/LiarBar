using Photon.Pun;
using UnityEngine;

public class InGameManager : MonoBehaviour
{
    private void Start()
    {
        SetPositionAndSpawn();
    }
    private void Update()
    {
        
    }
    private void SetPositionAndSpawn()
    {
        int index = 0; // 기본값
        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("PositionIndex", out object obj))
        {
            index = (int)obj;
        }

        float y = -2.3f;      // 높이
        float dist = 2f;      // 0,0 기준 거리

        Vector3 spawnPos = Vector3.zero;
        switch (index % 4)
        {
            case 0: // 위
                spawnPos = new Vector3(0, y, dist * ((index / 4) + 1));
                break;

            case 1: // 아래
                spawnPos = new Vector3(0, y, -dist * ((index / 4) + 1));
                break;

            case 2: // 왼
                spawnPos = new Vector3(-dist * ((index / 4) + 1), y, 0);
                break;

            case 3: // 오른
                spawnPos = new Vector3(dist * ((index / 4) + 1), y, 0);
                break;
        }

        Vector3 center = new Vector3(0, y, 0); 
        Vector3 dir = center - spawnPos;
        Quaternion rot = Quaternion.LookRotation(dir);

        PhotonNetwork.Instantiate("Player", spawnPos, rot);
        Debug.Log("지금 생성됨");
    }
}
