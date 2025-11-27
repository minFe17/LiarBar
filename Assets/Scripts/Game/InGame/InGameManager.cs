using Photon.Pun;
using UnityEngine;

public class InGameManager : MonoBehaviour
{
    static Vector3 Position = Vector3.zero;
    void Start()
    {
        int index = PhotonNetwork.LocalPlayer.ActorNumber - 1;
        Vector3 spawnPos;
        if (index%2 == 0)
        {
            spawnPos = new Vector3(index * 10, 0, 0);

        }
        else
        {
            spawnPos = new Vector3(0, 0, index*10);
        }

        PhotonNetwork.Instantiate("Player", spawnPos, Quaternion.identity);
    }
    void Update()
    {
        
    }
}
