using Photon.Pun;
using UnityEngine;

public class InGameManager : MonoBehaviour
{
    static Vector3 Position = Vector3.zero;
    void Start()
    {
        GameObject omject = PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity);
    }
    void Update()
    {
        
    }
}
