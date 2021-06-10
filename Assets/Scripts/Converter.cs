using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* This class containts useful methods to convert variables in this project */
public class Converter
{

    /* Using the dichotomy search, returns the index of the datasnapshot closest to a specific simulation time */
    public static int TimeToIndex(List<DataSnapshot> data, float time)
    {
        int dataCount = data.Count;
        if (dataCount == 0)
        {
            return -1;
        }
        int iMin = 0;
        int iMax = dataCount - 1;
        float tMin = data[iMin].Time;
        float tMax = data[iMax].Time;
        if (time <= tMin)
        {
            return iMin;
        }
        if (time >= tMax)
        {
            return iMax;
        }
        return BinarySearch(data, time, iMin, iMax);
    }

    private static int BinarySearch(List<DataSnapshot> data, float time, int iMin, int iMax)
    {
        if (iMax - iMin == 1)
        {
            float d1 = time - data[iMin].Time;
            float d2 = data[iMax].Time - time;
            if (d1 < d2)
            {
                return iMin;
            }
            else
            {
                return iMax;
            }
        }
        int iMed = (iMin + iMax) / 2;
        if (data[iMed].Time < time)
        {
            return BinarySearch(data, time, iMed, iMax);
        }
        else
        {
            return BinarySearch(data, time, iMin, iMed);
        }
    }

    /* Rounds a float by keeping a certain number of decimal digits */
    public static float Round(float x, int digits)
    {
        return (float) System.Math.Round((double) x, digits);
    }

    /* Returns the Vector3 corresponding to the center of an Area at a specific height */
    public static Vector3 AreaToVector3(Area area, float height)
    {
        GameObject areaDetectors = GameObject.Find("AreaDetectors");
        foreach (Transform transform in areaDetectors.transform)
        {
            GameObject go = transform.gameObject;
            AreaDetector areaDetector = go.GetComponent<AreaDetector>();
            if (areaDetector.Area.Equals(area))
            {
                return new Vector3(go.transform.position.x, height, go.transform.position.z);
            }
        }
        return Vector3.zero;
    }
}
