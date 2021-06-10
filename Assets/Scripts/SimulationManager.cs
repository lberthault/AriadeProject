using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using QTMRealTimeSDK;
using QualisysRealTime.Unity;
using UnityEditor;
using Microsoft.MixedReality.Toolkit;
using UnityEngine.SpatialTracking;

public class SimulationManager : MonoBehaviour
{


    // Simulation
    public float SimTime { get; set; }
    private NavConfig navConfig;
    [SerializeField]
    private string participantName;
    private int trialState;
    public int TrialState
    {
        get { return trialState; }
    }
    public const int TRIAL_NOT_STARTED = 0;
    public const int TRIAL_ONGOING = 1;
    public const int TRIAL_ENDED = 2;

    // Debug

    public enum Mode
    {
        XP,
        TEST
    }

    [SerializeField]
    public bool drawLines, setOcclusion, manualController, drawGUI;
    public Mode mode;
    [SerializeField]
    private List<GameObject> invisibleObjects;
    [SerializeField]
    private List<GameObject> obscurableObjects;
    [SerializeField]
    private Material invisibleMaterial;
    [SerializeField]
    private Material pathLineMaterial;
    [SerializeField]
    private float pathLineHeight = 0.7f;
    [SerializeField]
    private float pathLineWidth = 0.05f;
    [SerializeField]
    private float debugMovementSpeed;
    [SerializeField]
    private float debugMouseSensitivity;

    // Path
    public enum PathName
    {
        A,
        B,
        C,
        T
    }
    public static Path pathA = new Path("A16.15.25.24.33.43.42.32.22.21.20");
    public static Path pathB = new Path("B01.11.12.13.23.33.32.42.43.44.54");
    public static Path pathC = new Path("C55.45.35.34.23.13.12.11.21.31.30");
    public static Path pathT = new Path("T51.41.42.52");

    [SerializeField]
    private PathName pathName;
    public Path trialPath;
    public Path remainingPath;
    // Advice
    public enum AdviceName
    {
        ARROW,
        LIGHT,
        PEANUT,
    };

    public enum AdviceConfigName
    {
        ARROW_AIR,
        ARROW_GROUND,
        LIGHT,
        PEANUT
    }

    [SerializeField]
    private AdviceName advice;

    [SerializeField]
    private AdviceConfigName adviceConfigName;
    public AdviceConfig AdviceConfig
    {
        get
        {
            switch (adviceConfigName)
            {
                case (AdviceConfigName.ARROW_AIR):
                    return ARROW_AIR;
                case (AdviceConfigName.ARROW_GROUND):
                    return ARROW_GROUND;
                case (AdviceConfigName.LIGHT):
                    return LIGHT;
                case (AdviceConfigName.PEANUT):
                    return PEANUT;
                default: return ARROW_AIR;
            };
        }
    }

    public AdviceConfig ARROW_AIR, ARROW_GROUND, LIGHT, PEANUT;

    [SerializeField]
    public GameObject arrowPrefab, lightPrefab, peanutPrefab;
    [SerializeField]
    public GameObject arrowWrongWayPrefab, lightWrongWayPrefab, peanutWrongWayPrefab;

    public Material lightPathMaterial;
    public Material lightPathWrongWayMaterial;

    private LineRenderer lightPathLineRenderer;
    private LineRenderer lightPathWrongWayLineRenderer;

    List<Advice> visibleAdvice;

    public float AdviceBaseOffset
    {
        get { return AdviceConfig.AdviceBaseOffsetCoef * AreaDetectorSize; }
    }

    // Data
    private string dataFileName = "SimulationData";

    // GUI
    [SerializeField]
    private GameObject canvasGUI;
    [SerializeField]
    private GameObject participantGUI, pathGUI, adviceGUI, timeGUI, dataGUI, qualisysGUI, modeGUI, trialStateGUI;
    private TextMeshProUGUI participantText, pathText, adviceText, timeText, dataText, qualisysText, modeText, trialStateText;

    // QTM
    public float qtmPeriod = 0.1f;
    RTClient client;
    DiscoveryResponse? dr;
    private bool connectedToQTM;

    // Tracking
    public float trackingPeriod = 0.01f;
    private bool isTracking = false;

    [SerializeField]
    private GameObject feet;
    private FeetTracker feetTracker;
    [SerializeField]
    private GameObject hololens;
    private HololensTracker hololensTracker;

    // Area detector
    [SerializeField]
    private GameObject areaDetectorPrefab;
    public float AreaDetectorSize { get; set; }

    public GameObject peanut;

    void Start()
    {
        InitiateAdviceConfig();
        InitiateVariables();
        InitiateComponents();
        InitiateGUI();

        foreach (Area area in Area.BigAreaAreas())
        {
            if (!trialPath.Contains(area))
            {
                DisableAreaDetection(area);
            }
        }
        
        SetTrialState(TRIAL_NOT_STARTED);

        if (mode == Mode.XP)
        {
            foreach (GameObject o in invisibleObjects)
            {
                o.SetActive(false);
                SetObscurable(o);
            }
        } else
        {
            if (setOcclusion)
            {
                foreach (GameObject o in invisibleObjects)
                {
                    SetInvisible(o);
                }
                foreach (GameObject o in obscurableObjects)
                {
                    SetObscurable(o);
                }
            }
            if (drawLines)
            {
                DrawTrialPath();
            }
            canvasGUI.SetActive(drawGUI);
            if (manualController)
            {
                HololensController hc = hololens.AddComponent<HololensController>();
                hc.movementSpeed = debugMovementSpeed;
                hc.mouseSensitivity = debugMouseSensitivity;
            }
            else
            {
                //InitQTMServer();
                //StartCoroutine(nameof(CheckQTMConnection));
            }

        }
       
        string prefix = "Assets/Landmarks/" + pathName + "/";
        string suffix = ".png";
        bool bigAreaIsDone = false;
        int currentTexture = 1;
        for (int i = 1; i < trialPath.Count() - 2; i++)
        {
            Area a = trialPath.Get(i);
            if (!a.InBigArea())
            {
                AreaDetector ad = a.GetAreaDetector();
                Texture t = (Texture)AssetDatabase.LoadAssetAtPath(prefix + "R" + currentTexture + suffix, typeof(Texture));
                ad.Texture = t;
                currentTexture++;
            } else if (!bigAreaIsDone)
            {
                foreach (Area bigAreaArea in Area.BigAreaAreas())
                {
                    AreaDetector ad = bigAreaArea.GetAreaDetector();
                    Texture t = (Texture)AssetDatabase.LoadAssetAtPath(prefix + "R" + currentTexture + suffix, typeof(Texture));
                    ad.Texture = t;
                }
                bigAreaIsDone = true;
                currentTexture++;
            }
        }

        Camera.main.GetComponent<TrackedPoseDriver>().enabled = false;
    }

    private void DisableAreaDetection(Area area)
    {
        AreaDetector areaDetector = area.GetAreaDetector();
        if (areaDetector != null)
        {
            areaDetector.gameObject.GetComponent<Collider>().enabled = false;
        }
    }

    private void InitiateAdviceConfig()
    {
        /* 
         * h : base height
         * c : base offset coef
         * rY : Y rotation
         * 0 = LEFT (should not change)
         * 1 = RIGHT (should not change)
         * 2 = DOWN (should not change)
         * 3 = TURN_LEFT
         * 4 = TURN_RIGHT
         * 5 = GO_FORWARD
         * 6 = GO_BACKWARD
         */
        float h = 0.8f;
        float c = 1.0f;
        List<float> rY = new List<float>() { -90f, +90f, +180f, -100f, +100f, +30f, +180f };
        float rX = 0f;
        ARROW_AIR = new AdviceConfig(arrowPrefab, arrowWrongWayPrefab, h, c, rY, rX);

        h = 0.3f;
        c = 0.3f;
        rY = new List<float>() { -90f, +90f, +180f, -90f, +90f, +0f, +180f };
        rX = 90f;
        ARROW_GROUND = new AdviceConfig(arrowPrefab, arrowWrongWayPrefab, h, c, rY, rX);

        h = 0.3f;
        c = 0.8f;
        rY = new List<float>() { -90f, +90f, +180f, -90f, +90f, +30f, +180f };
        rX = 0f;
        LIGHT = new AdviceConfig(null, lightWrongWayPrefab, h, c, rY, rX);

        h = 0f;
        c = 0.4f;
        rY = new List<float>() { -90f, +90f, +180f, 0f, +0f, -50f, +180f };
        rX = 0f;
        PEANUT = new AdviceConfig(peanutPrefab, peanutWrongWayPrefab, h, c, rY, rX);
    }

    private void InitiateVariables()
    {
        SimTime = 0f;
        switch (pathName)
        {
            case PathName.A: trialPath = pathA; break;
            case PathName.B: trialPath = pathB; break;
            case PathName.C: trialPath = pathC; break;
            case PathName.T: trialPath = pathT; break;
        }
        navConfig = new NavConfig(participantName, trialPath, advice);
        client = RTClient.GetInstance();
        remainingPath = trialPath;
        visibleAdvice = new List<Advice>();

        AreaDetectorSize = areaDetectorPrefab.GetComponent<BoxCollider>().size.x;
        

    }

    private void InitiateGUI()
    {
        qualisysText.text = "Qualisys: Disconnected";
        pathText.text = "Path " + trialPath.Name;
        adviceText.text = "Advice: " + advice.ToString();
        participantText.text = "Participant: " + participantName;
        modeText.text = (mode == Mode.XP) ? "XP Mode" : "Test Mode";
    }

    private void InitiateComponents()
    {
        feetTracker = feet.GetComponent<FeetTracker>();
        hololensTracker = hololens.GetComponent<HololensTracker>();
        qualisysText = qualisysGUI.GetComponent<TextMeshProUGUI>();
        dataText = dataGUI.GetComponent<TextMeshProUGUI>();
        timeText = timeGUI.GetComponent<TextMeshProUGUI>();
        pathText = pathGUI.GetComponent<TextMeshProUGUI>();
        adviceText = adviceGUI.GetComponent<TextMeshProUGUI>();
        participantText = participantGUI.GetComponent<TextMeshProUGUI>();
        modeText = modeGUI.GetComponent<TextMeshProUGUI>();
        trialStateText = trialStateGUI.GetComponent<TextMeshProUGUI>();
    }

    private void SetTrialState(int state)
    {
        trialState = state;
        switch (state)
        {
            case TRIAL_NOT_STARTED:
                trialStateText.text = "Trial not started"; break;
            case TRIAL_ONGOING:
                trialStateText.text = "Trial ongoing"; break;
            case TRIAL_ENDED:
                trialStateText.text = "Trial ended"; break;
        }
    }

    void Update()
    {
        if (isTracking)
        {
            SimTime += Time.deltaTime;
        } else if (trialState == TRIAL_ONGOING) {
            StartCoroutine(nameof(Track));
            isTracking = true;
        }
    }

    // Every time the coroutine starts, the trackers are told to acquire data and update text files
    private IEnumerator Track()
    {
        while (true)
        {
            feetTracker.Track(SimTime);
            hololensTracker.Track(SimTime);
            UpdateDataText();
            yield return new WaitForSeconds(trackingPeriod);
        }
    }

    private void OnApplicationQuit()
    {
        WriteData();
    }

    // Updates debug mode GUI
    private void UpdateDataText()
    {
        float prevAcc = Converter.Round(hololensTracker.PreviousAcceleration(), 2);
        dataText.text = "Steps= " + feetTracker.StepCount + 
            "\nDistance travelled= " + Converter.Round(hololensTracker.DistanceTravelled(), 2) + 
            "\nSpeed= " + Converter.Round(hololensTracker.CurrentSpeed(), 2) + 
            "\nPrev_Acceleration= " + prevAcc + " (" + (prevAcc >= 0 ? "+" : "-")  + ")";

        TimeSpan timeSpan = TimeSpan.FromSeconds(SimTime);
        string time = string.Format("Elapsed time: {0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
        timeText.text = time;
    }

    // Checks connection with QTM localhost server
    private IEnumerator CheckQTMConnection()
    {
        while (true)
        {
            if (!connectedToQTM)
            {
                if (dr != null)
                {
                    if (connectedToQTM = ConnectToServer())
                    {
                        qualisysText.text = "Qualisys: Connected";
                        Debug.Log("Qualisys: Connected to server : " + dr.Value.HostName + " (" + dr.Value.IpAddress + ":" + dr.Value.Port + ")");
                    }
                } else
                {
                    InitQTMServer();
                }
            }
            yield return new WaitForSeconds(qtmPeriod);
        }
    }

    // Connects client to localhost server
    private bool ConnectToServer()
    {
        return client.Connect(dr.Value, dr.Value.Port, true, true, false, true, false, true);
    }

    // Looks fot the localhost discovery response
    private bool FetchLocalhostServer()
    {
        List<DiscoveryResponse> servers = client.GetServers();
        foreach (DiscoveryResponse res in servers)
        {
            if (res.HostName == "Localhost")
            {
                dr = res;
                break;
            }
        }

        return dr != null;
    }

    // Initiate QTM Server by connecting to localhost
    private void InitQTMServer()
    {
        if (FetchLocalhostServer())
        {
            if (connectedToQTM = ConnectToServer())
            {
                qualisysText.text = "Qualisys : Connected";
                Debug.Log("Qualisys: Connected to server: " + dr.Value.HostName + " (" + dr.Value.IpAddress + ":" + dr.Value.Port + ")");
            }
            else
            {
                qualisysText.text = "Qualisys : Disconnected";
                Debug.LogError("Qualisys: Failed to connect to localhost server");
            }
        }
        else
        {
            qualisysText.text = "Qualisys : Disconnected";
            Debug.Log("Qualisys: Failed to fetch localhost server");
        }
    }

    public bool IsConnectedToQTM()
    {
        return connectedToQTM;
    }

    public NavConfig GetNavConfig()
    {
        return navConfig;
    }

    public void StartTrial(Area startingArea)
    {
        while (!trialPath.Get(0).Equals(startingArea))
        {
            trialPath.Pop();
        }
        SetTrialState(TRIAL_ONGOING);
        ResetData();
    }

    private void ResetData()
    {
        feetTracker.ResetData();
        hololensTracker.ResetData();
        SimTime = 0f;
    }

    public void EndTrial()
    {
        SetTrialState(TRIAL_ENDED);
        StopCoroutine(nameof(Track));
        RemoveAllAdvice();
        isTracking = false;
    }

    // The data written in the synthetical file at the end of the trial
    public void WriteData()
    {
        DataManager.WriteDataSummary(dataFileName, navConfig, SimTime, hololensTracker, feetTracker);
    }

    public void AddAdvice(Advice advice)
    {
        visibleAdvice.Add(advice);
    }

    int vertexCount = 50;
    List<int> flags = new List<int>();
    private Vector3 start;

    public float lightPathDelayInSeconds;

    IEnumerator DrawPoints0(object[] parms)
    {
        List<Vector3> positionList = new List<Vector3>();
        Vector3 at = (Vector3)parms[0];
        Vector3 to = (Vector3)parms[1];
        Vector3 toto = (Vector3)parms[2];
        lightPathLineRenderer.positionCount = 0;
        for (float ratio = 0; ratio <= 1; ratio += 1f / vertexCount)
        {
            Vector3 tangent1 = Vector3.Lerp(start, at, ratio);
            Vector3 tangent2 = Vector3.Lerp(at, (at + to) / 2f, ratio);
            Vector3 curve = Vector3.Lerp(tangent1, tangent2, ratio);
            positionList.Add(curve);
            lightPathLineRenderer.positionCount++;
            lightPathLineRenderer.SetPosition(lightPathLineRenderer.positionCount - 1, curve);
            yield return new WaitForSeconds(lightPathDelayInSeconds);
        }
        for (float ratio = 0; ratio <= 1; ratio += 1f / vertexCount)
        {
            Vector3 tangent1 = Vector3.Lerp((at + to) / 2f, to, ratio);
            Vector3 tangent2 = Vector3.Lerp(to, (to + toto) / 2f, ratio);
            Vector3 curve = Vector3.Lerp(tangent1, tangent2, ratio);
            positionList.Add(curve);
            lightPathLineRenderer.positionCount++;
            lightPathLineRenderer.SetPosition(lightPathLineRenderer.positionCount - 1, curve);
            yield return new WaitForSeconds(lightPathDelayInSeconds);
        }

        start = (to + toto) / 2f;
        flags.Add(positionList.Count);
        yield return null;
    }

    IEnumerator DrawPoints1(object[] parms)
    {
        List<Vector3> positionList = new List<Vector3>();
        Vector3 at = (Vector3)parms[0];
        Vector3 to = (Vector3)parms[1];
        Vector3 toto = (Vector3)parms[2];
        for (float ratio = 0; ratio <= 1; ratio += 1f / vertexCount)
        {
            Vector3 tangent1 = Vector3.Lerp((at + to) / 2f, to, ratio);
            Vector3 tangent2 = Vector3.Lerp(to, (to + toto) / 2f, ratio);
            Vector3 curve = Vector3.Lerp(tangent1, tangent2, ratio);
            positionList.Add(curve);
            lightPathLineRenderer.positionCount++;
            lightPathLineRenderer.SetPosition(lightPathLineRenderer.positionCount - 1, curve);
            yield return new WaitForSeconds(lightPathDelayInSeconds);
        }

        start = (to + toto) / 2f;
        flags.Add(positionList.Count);
        yield return null;
    }
    public void DrawLightPath(Vector3 from, Vector3 at, Vector3 to, Vector3 toto)
    {
        if (lightPathLineRenderer == null)
        {
            GameObject pathLine = new GameObject();
            pathLine.name = "LightPath";
            pathLine.transform.position = from;
            lightPathLineRenderer = pathLine.AddComponent<LineRenderer>();
            lightPathLineRenderer.material = lightPathMaterial;
            SetObscurable(lightPathLineRenderer.gameObject);
            lightPathLineRenderer.startColor = Color.white;
            lightPathLineRenderer.endColor = Color.white;
            lightPathLineRenderer.startWidth = 0.08f;
            lightPathLineRenderer.endWidth = 0.08f;
            lightPathLineRenderer.numCornerVertices = 0;
            start = from;
            object[] parms = new object[3] { at, to, toto };
            StartCoroutine(nameof(DrawPoints0), parms);
            return;
        } else
        {

            List<Vector3> positionList = new List<Vector3>();
            if (lightPathLineRenderer.positionCount == 0)
            {
                start = from;
                object[] parms = new object[3] { at, to, toto };
                StartCoroutine(nameof(DrawPoints0), parms);

            } else
            {
                object[] parms = new object[3] { at, to, toto };
                StartCoroutine(nameof(DrawPoints1), parms);
            }

           
        }

    }

    private int segments = 100;

    public static void SetObscurable(GameObject o)
    {
        Component[] renderers = o.GetComponentsInChildren(typeof(Renderer));
        foreach (Component renderer in renderers)
        {
            ((Renderer)renderer).material.renderQueue = 3002;
        }
    }

    private void SetInvisible(GameObject o)
    {
        Component[] renderers = o.GetComponentsInChildren(typeof(Renderer));
        foreach (Component renderer in renderers)
        {
            ((Renderer)renderer).material = invisibleMaterial;
        }
    }

    public void DrawWrongWayLightPath(Area area, Vector3 offset, float rotationY)
    {
        if (lightPathWrongWayLineRenderer == null)
        {
            GameObject pathLine = new GameObject();
            pathLine.name = "WrongWayLightPath";
            lightPathWrongWayLineRenderer = pathLine.AddComponent<LineRenderer>();
            lightPathWrongWayLineRenderer.material = lightPathWrongWayMaterial;
            lightPathWrongWayLineRenderer.startColor = Color.red;
            lightPathWrongWayLineRenderer.endColor = Color.red;
            lightPathWrongWayLineRenderer.startWidth = 0.08f;
            lightPathWrongWayLineRenderer.endWidth = 0.08f;
            lightPathWrongWayLineRenderer.useWorldSpace = false;
        }
        lightPathWrongWayLineRenderer.gameObject.transform.eulerAngles = Vector3.zero;
        lightPathWrongWayLineRenderer.positionCount = segments + 1;
        lightPathWrongWayLineRenderer.gameObject.transform.position = Converter.AreaToVector3(area, 0.2f) + offset;
        CreatePoints(area, offset, rotationY);
    }
   
    void CreatePoints(Area area, Vector3 offset, float rotationY)
    {
        /*
        float x;
        float y = 0f;
        float z;

        float angle = 0f;

        for (int i = 0; i < (segments + 1); i++)
        {
            x = Mathf.Sin(Mathf.Deg2Rad * angle) * xradius + xradius;
            z = Mathf.Cos(Mathf.Deg2Rad * angle) * zradius;

            lightPathWrongWayLineRenderer.SetPosition(i, new Vector3(x, y, z));

            angle += (380f / segments);
        }
        lightPathWrongWayLineRenderer.gameObject.transform.Rotate(new Vector3(0, rotationY, 0));

        */

        float crossSize = 0.5f;
        lightPathWrongWayLineRenderer.positionCount = 5;
        lightPathWrongWayLineRenderer.SetPosition(0, new Vector3(-1, 0, -1) * crossSize);
        lightPathWrongWayLineRenderer.SetPosition(1, new Vector3(+1, 0, +1) * crossSize);
        lightPathWrongWayLineRenderer.SetPosition(2, new Vector3(0, 0, 0) * crossSize);
        lightPathWrongWayLineRenderer.SetPosition(3, new Vector3(1, 0, -1) * crossSize);
        lightPathWrongWayLineRenderer.SetPosition(4, new Vector3(-1, 0, 1) * crossSize);
        lightPathLineRenderer.gameObject.transform.Rotate(new Vector3(0, rotationY, 0));

        /*
        // Z(X)
        float a = -3f;
        float b = 0f;
        float c = 0f;
        float x, z;
        float z0 = 0.5f;
        z = z0;
        for (int i = 0; i < (segments + 1); i++)
        {
            x = a * z * z - b * z + c;

            lightPathLineRenderer.SetPosition(i, new Vector3(x, 0f, z));
            z -= 2f * z0 / segments;
        }
        lightPathLineRenderer.gameObject.transform.Rotate(new Vector3(0, rotationY, 0));*/
    }

    public void RemoveAdviceAtArea(Area area)
    {
        //if (advice == AdviceName.ARROW)
        //{
            foreach (Advice advice in visibleAdvice)
            {
                if (advice.Area.Equals(area))
                {
                    advice.Remove();
                }
            }
            /*
        } else if (advice == AdviceName.LIGHT)
        {
            if (lightPathLineRenderer != null)
            {
                Debug.Log("d");
                Vector3[] pos = new Vector3[lightPathLineRenderer.positionCount];
                Vector3[] newPos = new Vector3[lightPathLineRenderer.positionCount - 1];
                int offset = 0;
                lightPathLineRenderer.GetPositions(pos);
                for (int i = 0; i < pos.Length; i++)
                {
                    if (Vector3.Distance(Converter.AreaToVector3(area, 0.2f), pos[i]) < 0.01f)
                    {
                        Debug.Log("a");
                        newPos[i] = pos[i + offset];
                    }
                    else
                    {
                        Debug.Log("b");
                        offset++;
                    }
                }
                lightPathLineRenderer.positionCount--;
                lightPathLineRenderer.SetPositions(newPos);
            } else
            {
                Debug.Log("e");
            }
           
        }*/
        
    }

    public void RemoveAllAdvice()
    {
        if (advice == AdviceName.ARROW || advice == AdviceName.PEANUT)
        {
            foreach (Advice advice in visibleAdvice)
            {
                advice.Remove();
            }
        }
        else if (advice == AdviceName.LIGHT)
        {
            if (lightPathLineRenderer != null)
            {
                Vector3[] newPos = new Vector3[0];
                lightPathLineRenderer.positionCount = 0;
                lightPathLineRenderer.SetPositions(newPos);
            }
            if (lightPathWrongWayLineRenderer != null)
            {
                Vector3[] newPos = new Vector3[0];
                lightPathWrongWayLineRenderer.positionCount = 0;
                lightPathWrongWayLineRenderer.SetPositions(newPos);
            }
            foreach (Advice advice in visibleAdvice)
            {
                advice.Remove();
            }
            flags.Clear();
        }
    }

    public void RemoveWrongWayLightAdvice()
    {
        foreach (Advice advice in visibleAdvice)
        {
            advice.Remove();
        }
    }

    public void RemoveLightAdvice()
    {
        if (lightPathLineRenderer != null)
        {
            int n = flags[0];
            Vector3[] newPos = new Vector3[lightPathLineRenderer.positionCount - n];
            for (int i = n; i < lightPathLineRenderer.positionCount; i++)
            {
                newPos[i - n] = lightPathLineRenderer.GetPosition(i);
            }
            lightPathLineRenderer.positionCount -= n;
            lightPathLineRenderer.SetPositions(newPos);
            flags.RemoveAt(0);
        }
        if (lightPathWrongWayLineRenderer != null)
        {
            Vector3[] newPos = new Vector3[0];
            lightPathWrongWayLineRenderer.positionCount = 0;
            lightPathWrongWayLineRenderer.SetPositions(newPos);
        }
        foreach (Advice advice in visibleAdvice)
        {
            advice.Remove();
        }
    }

    /* Draws the theoretical path the participant has to follow during the xp */
    private void DrawTrialPath()
    {
        GameObject pathLine = new GameObject();
        pathLine.name = "PathLine";
        pathLine.transform.position = Converter.AreaToVector3(trialPath.Get(0), pathLineHeight);
        pathLine.AddComponent<LineRenderer>();
        LineRenderer lr = pathLine.GetComponent<LineRenderer>();
        lr.material = pathLineMaterial;
        lr.startColor = Color.red;
        lr.endColor = Color.red;
        lr.startWidth = pathLineWidth;
        lr.endWidth = pathLineWidth;
        lr.numCornerVertices = 0;
        lr.positionCount = trialPath.Count();
        for (int i = 0; i < trialPath.Count(); i++)
        {
            lr.SetPosition(i, Converter.AreaToVector3(trialPath.Get(i), pathLineHeight));
        }
    }

    public static int DistanceBetweenAreas(Area a1, Area a2)
    {
        return Mathf.Abs(a2.column - a1.column) + Mathf.Abs(a2.line - a1.line);
    }

    public AdviceName GetAdviceName()
    {
        return advice;
    }

    public LineRenderer GetLightPathLineRenderer()
    {
        return lightPathLineRenderer;
    }

}