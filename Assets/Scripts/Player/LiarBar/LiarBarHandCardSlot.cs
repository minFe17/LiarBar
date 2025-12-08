using UnityEngine;

public class LiarBarHandCardSlot : MonoBehaviour
{
    void Start()
    {
        GamePlayer player = GetComponentInParent<GamePlayer>();
        player.HandCardSlot = this.transform;
    }
}