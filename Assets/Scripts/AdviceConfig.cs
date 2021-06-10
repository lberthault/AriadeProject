using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdviceConfig
{
    public GameObject AdvicePrefab { get; }
    public GameObject WrongWayAdvicePrefab { get; }
    public float AdviceBaseHeight { get; }
    public float AdviceBaseOffsetCoef { get; }
    public List<float> AdviceRotationY { get; }
    public float AdviceRotationX { get; }


    public AdviceConfig(GameObject prefab, GameObject wrongWayPrefab, float baseHeight, float baseOffsetCoef, List<float> rotationY, float rotationX)
    {
        AdvicePrefab = prefab;
        WrongWayAdvicePrefab = wrongWayPrefab;
        AdviceBaseHeight = baseHeight;
        AdviceBaseOffsetCoef = baseOffsetCoef;
        AdviceRotationY = rotationY;
        AdviceRotationX = rotationX;
    }
}
