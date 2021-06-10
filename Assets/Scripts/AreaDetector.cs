using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Makes the link between the area detector gameobject equipped with a collider and the abstract Area class */
public class AreaDetector : MonoBehaviour
{

    public int line, column;
    public Texture Texture { get; set; }

    public Area Area
    {
        get { return new Area(line, column); }
    }

    public void DisplayLandmarks(HololensTracker.Direction from, bool checkBigArea)
    {
        if (Area.InBigArea() && checkBigArea)
        {
            foreach (Area area in Area.BigAreaAreas())
            {
                area.GetAreaDetector().DisplayLandmarks(from, false);
            }
        }
        List<Landmark.LandmarkPosition> landmarksToDisplay = GetLandmarksFromDirection(from);
        if (Texture != null)
        {
            Component[] renderers = GetComponentsInChildren(typeof(Renderer));
            foreach (Component renderer in renderers)
            {
                Landmark landmark = renderer.gameObject.GetComponent<Landmark>();
                if (landmarksToDisplay.Contains(landmark.position))
                {
                    ((Renderer)renderer).enabled = true;
                    ((Renderer)renderer).material.mainTexture = Texture;
                    SimulationManager.SetObscurable(renderer.gameObject);
                }
            }
        }
    }

    public void RemoveLandmarks(bool checkBigArea)
    {
        if (Area.InBigArea() && checkBigArea)
        {
            foreach (Area area in Area.BigAreaAreas())
            {
                area.GetAreaDetector().RemoveLandmarks(false);
            }
            return;
        }
        Component[] renderers = GetComponentsInChildren(typeof(Renderer));
        foreach (Component renderer in renderers)
        {
                ((Renderer)renderer).enabled = false;           
        }
    }

    private List<Landmark.LandmarkPosition> GetLandmarksFromDirection(HololensTracker.Direction from)
    {
        List<Landmark.LandmarkPosition> res = new List<Landmark.LandmarkPosition>();
        if (Area.InBigArea())
        {
            if (Area.line == 2 && Area.column == 3)
            {
                res.Add(Landmark.LandmarkPosition.TOP_RIGHT_FACE_DOWN);
            } else if (Area.line == 2 && Area.column == 4)
            {
                res.Add(Landmark.LandmarkPosition.BOTTOM_RIGHT_FACE_LEFT);
            } else if (Area.line == 3 && Area.column == 3)
            {
                res.Add(Landmark.LandmarkPosition.TOP_LEFT_FACE_RIGHT);
            } else
            {
                res.Add(Landmark.LandmarkPosition.BOTTOM_LEFT_FACE_UP);
            }
        } else
        {
            if (from == HololensTracker.Direction.DOWN || from == HololensTracker.Direction.UP)
            {
                res.Add(Landmark.LandmarkPosition.BOTTOM_LEFT_FACE_UP);
                res.Add(Landmark.LandmarkPosition.BOTTOM_RIGHT_FACE_UP);
                res.Add(Landmark.LandmarkPosition.TOP_LEFT_FACE_DOWN);
                res.Add(Landmark.LandmarkPosition.TOP_RIGHT_FACE_DOWN);
            }
            else
            {

                res.Add(Landmark.LandmarkPosition.BOTTOM_LEFT_FACE_RIGHT);
                res.Add(Landmark.LandmarkPosition.BOTTOM_RIGHT_FACE_LEFT);
                res.Add(Landmark.LandmarkPosition.TOP_LEFT_FACE_RIGHT);
                res.Add(Landmark.LandmarkPosition.TOP_RIGHT_FACE_LEFT);
            }
        }
     
        return res;
    }
}
