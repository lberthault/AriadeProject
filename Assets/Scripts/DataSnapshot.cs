using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Represents the data stored at a specific time by the trackers */
public class DataSnapshot
{
    public float Time { get; }
    public Vector3 Position { get; }
    public Vector3 Rotation { get; }

    public DataSnapshot(float time, Vector3 position, Vector3 rotation)
    {
        this.Time = time;
        this.Position = position;
        this.Rotation = rotation;
    }
}
