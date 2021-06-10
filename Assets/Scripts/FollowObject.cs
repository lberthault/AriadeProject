using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowObject : MonoBehaviour
{

    [SerializeField]
    private GameObject obj;

    void Start()
    {
        MatchPositionAndRotation();
    }
    void Update()
    {
        MatchPositionAndRotation();
    }

    private void MatchPositionAndRotation()
    {
        transform.position = obj.transform.position;
        transform.rotation = obj.transform.rotation;
    }
}
