using NUnit.Framework;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;


public class MyPlayer : MonoBehaviour
{
    public static MyPlayer local;
    public static List<(Player player, Transform hand)> playerLeftHands = new();
    public static Player myPlayer;

    private Transform _head; //빼도된다
    private Transform _leftHand;
    

    public Transform Head
        { get { return _head; } }
    public Transform LeftHand
        { get { return _leftHand; } }


    private void Awake()
    {
        
    }
    private void Start()
    {
        PhotonView view = GetComponent<PhotonView>();
        Transform leftHand = FindLeftHandInActiveModel(transform.Find("StandCharacter"));

        playerLeftHands.Add((view.Owner, leftHand));


        if (!view.IsMine) return;

        local = this;
        myPlayer = view.Owner;
        _leftHand = leftHand;
        _head = FindHeadInActiveModel(transform.Find("StandCharacter"));
    }
    private Transform FindLeftHandInActiveModel(Transform parent)
    {
        string[] path = { "Root", "Hips", "Spine", "Spine1", "LeftShoulder", "LeftArm", "LeftForeArm", "LeftHand" };
        foreach (Transform child in parent)
        {
            if (!child.gameObject.activeInHierarchy) continue;

            Transform bone = FindBone(child, path);
            if (bone != null) return bone;
        }
        return null;
    }

    private Transform FindHeadInActiveModel(Transform parent)
    {
        string[] path = { "Root", "Hips", "Spine", "Spine1", "Neck", "Head" };
        foreach (Transform child in parent)
        {
            if (!child.gameObject.activeInHierarchy) continue;

            Transform bone = FindBone(child, path);
            if (bone != null) return bone;
        }
        return null;
    }

    private Transform FindBone(Transform root, string[] path)
    {
        if (root == null || !root.gameObject.activeInHierarchy)
            return null;
        Transform current = root;

        foreach (var part in path)
        {
            if (current == null)
            {
                Debug.LogWarning($"Not Found: {current.name}/{part}");
                return null;
            }
            current = current.Find(part);
            if (current == null || !current.gameObject.activeInHierarchy)
                return null; // 비활성화된 중간 뼈대가 있으면 null
        }

        return current;
    }
}
