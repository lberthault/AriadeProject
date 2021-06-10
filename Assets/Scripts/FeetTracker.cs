using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class FeetTracker : MonoBehaviour
{

    public const int NO_CHANGE = 0;

    // Data
    private const string dataFileName = "FeetData";
    private List<Step> steps;

    // Simulation
    private SimulationManager simManager;
    private float simTime;

    // Debug
    GameObject directionLine;
    public float directionLineLength = 3f;
    public float directionLineWidth = 0.05f;
    public Material frontFootMaterial;
    public Material backFootMaterial;
    public Material feetDirectionLineMaterial;

    // Tracking
    private FootTracker lFoot, rFoot;
    private FootTracker frontFoot;
    private bool isSetUp = false;

    public List<Step> Steps
    {
        get { return steps; }
        set { steps = value; }
    }

    public Step LastStep
    {
        get
        {
            if (StepCount != 0)
            {
                return steps[StepCount - 1];
            }
            else
            {
                return null;
            };
        }
    }

    public int StepCount
    {
        get
        {
            if (steps.Count != 0)
            {
                if (!steps[steps.Count - 1].isFinished())
                {
                    return steps.Count - 1;
                }
            }
            return steps.Count;
        }
    }

    private void Awake()
    {
        steps = new List<Step>();
        simManager = GameObject.Find("SimulationManager").GetComponent<SimulationManager>();
        SearchFootTrackers();
    }

    /* Detects and asigns automatically the foot trackers */  
    private void SearchFootTrackers()
    {
        foreach (Transform child in transform.GetComponentsInChildren<Transform>())
        {
            GameObject childGo = child.gameObject;
            if (childGo.TryGetComponent(out FootTracker footTracker))
            {
                if (footTracker.foot == Foot.Left)
                {
                    if (lFoot != null)
                    {
                        Debug.LogError("ERROR : more than one object is assigned to left foot");
                    }
                    else
                    {
                        lFoot = footTracker;
                    }
                }
                else
                {
                    if (rFoot != null)
                    {
                        Debug.LogError("ERROR : more than one object is assigned to right foot");
                    }
                    else
                    {
                        rFoot = footTracker;
                    }
                }
            }
        }

        if (lFoot == null)
        {
            Debug.LogError("ERROR : no object is assigned to left foot");
        }
        if (rFoot == null)
        {
            Debug.LogError("ERROR : no object is assigned to right foot");
        }

        if (lFoot != null && rFoot != null)
        {
            isSetUp = true;
        }
    }

    public int Track(float simTime)
    {
        this.simTime = simTime;
        if (isSetUp)
        {
            frontFoot = FrontFoot(lFoot.LastDataSnapshot(), rFoot.LastDataSnapshot());
            TrackFoot(lFoot);
            TrackFoot(rFoot);
            WriteData();
        }        
        return NO_CHANGE;
    }

    private void TrackFoot(FootTracker foot)
    {
        int res = foot.Track(simTime);
        // Set front foot in different color
        if (simManager.mode == SimulationManager.Mode.TEST)
        {
            foot.gameObject.GetComponentInChildren<MeshRenderer>().material = foot == frontFoot ? frontFootMaterial : backFootMaterial;
        }
     
        switch (res)
        {
            case FootTracker.NO_CHANGE: break;
            case FootTracker.STEP_START:
                OnStepStart(foot);
                break;
            case FootTracker.STEP_END:
                OnStepEnd(foot);
                break;

        }
        
    }

    private void OnStepStart(FootTracker foot)
    {
        steps.Add(foot.LastStep);
        DataManager.WriteDataInFile(dataFileName, simManager.GetNavConfig(), "t = " + simTime + " : " + foot.Name + " starts a step");
    }

    private void OnStepEnd(FootTracker foot)
    {
        DataManager.WriteDataInFile(dataFileName, simManager.GetNavConfig(), "t = " + simTime + " : " + foot.Name + " ends a step (total=" + StepCount + ")");
    }

    public float MeanStepFrequency()
    {
        if (steps.Count == 0)
        {
            return -1f;
        }
        return StepCount / (LastStep.End - steps[0].Start);
    }

    public void WriteData()
    {
        //DataWriter.WriteDataInFile(dataFileName, "t = " + simTime + " steps = " + StepCount);
    }

    /* Returns the current front foot */
    private FootTracker FrontFoot(DataSnapshot lFootData, DataSnapshot rFootData)
    {
        if (lFootData == null || rFootData == null)
        {
            return lFoot;
        }
        float thetaL = lFootData.Rotation.y * Mathf.PI / 180f;
        thetaL %= 2 * Mathf.PI;
      
        float thetaR = rFootData.Rotation.y * Mathf.PI / 180f;
        thetaR %= 2 * Mathf.PI;
        // Compute the medium angle between the feet */
        float theta = (thetaL + thetaR) / 2.0f;
        if (thetaL > thetaR && thetaL - thetaR > Mathf.PI)
        {
            theta += Mathf.PI;
        }
  
        theta %= 2 * Mathf.PI;

        /* Compute a director vector of the line passing between the feet */
        Vector3 v = new Vector3(Mathf.Tan(theta), 0, 1);
        if (theta < Mathf.PI/2f || theta > 3f*Mathf.PI / 2f)
        {
            v = -v;
        }

        if (lFootData.Rotation.y > 0.0f &&
            lFootData.Rotation.y < 180f &&
            rFootData.Rotation.y > 180f &&
            rFootData.Rotation.y < 360f)
        {
            v = -v;
        }
        Vector3 posL = ProjectOnFloor(lFootData.Position);
        Vector3 posR = ProjectOnFloor(rFootData.Position);
        Vector3 OC = (posL + posR) / 2.0f;
        if (simManager.drawLines)
        {
            // Draw direction line
            UpdateDirectionLine(OC, OC + v / Mathf.Sqrt(ScalarProduct(v, v)) * directionLineLength, Color.black);
        }
        // Compute distance between both feet and the center of the feet to determine which one if in front
        float dL = ScalarProduct(posL - OC, v) / ScalarProduct(v, v);
        float dR = ScalarProduct(posR - OC, v) / ScalarProduct(v, v);

        return dL < dR ? rFoot : lFoot;
    }

    /* The rate at which the left foot is in front during the xp */
    public float LeftFootInFrontRate()
    {
        int N = lFoot.DataCount();
        int n = 0;
        for (int i = 0; i < N; i++)
        {
            if (FrontFoot(lFoot.GetDataAtIndex(i), rFoot.GetDataAtIndex(i)) == lFoot)
            {
                n++;
            }
        }
        return 1f*n/N;
    }

    public void ResetData()
    {
        lFoot.ResetData();
        rFoot.ResetData();
        steps.Clear();
    }

    /* Re-draws the direction line indicating the orientation of the feet */
    void UpdateDirectionLine(Vector3 start, Vector3 end, Color color)
    {
        LineRenderer lr;

        if (directionLine != null)
        {
            lr = directionLine.GetComponent<LineRenderer>();
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
            return;
        }

        directionLine = new GameObject();
        directionLine.name = "DirectionLine";
        directionLine.transform.position = start;
        directionLine.AddComponent<LineRenderer>();
        lr = directionLine.GetComponent<LineRenderer>();
        lr.material = feetDirectionLineMaterial;
        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = directionLineWidth;
        lr.endWidth = directionLineWidth;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
    }

    private float ScalarProduct(Vector3 v1, Vector3 v2)
    {
        return (v1.x * v2.x) + (v1.y * v2.y) + (v1.z * v2.z);
    }

    public Vector3 ProjectOnFloor(Vector3 v)
    {
        return new Vector3(v.x, 0, v.z);
    }

    public void RegisterData()
    {
        return;
    }
}
