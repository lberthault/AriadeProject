using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Landmark : MonoBehaviour
{
    public enum LandmarkPosition
    {
        BOTTOM_LEFT_FACE_UP,
        BOTTOM_RIGHT_FACE_UP,
        TOP_RIGHT_FACE_DOWN,
        TOP_RIGHT_FACE_LEFT,
        BOTTOM_LEFT_FACE_RIGHT,
        BOTTOM_RIGHT_FACE_LEFT,
        TOP_LEFT_FACE_RIGHT,
        TOP_LEFT_FACE_DOWN
    }

    public LandmarkPosition position;
}
