using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Step
{
    public Foot foot;

    private bool finished;
    public float Start { get; }
    public float End { get; set; }
    public float Duration
    {
        get { return End - Start; }
    }

    public Step(Foot foot, float start, float end)
    {
        this.foot = foot;
        this.Start = start;
        this.End = end;
        this.finished = start < end;
    }

    public Step(Foot foot, float start, float end, bool finished)
    {
        this.foot = foot;
        this.Start = start;
        this.End = end;
        this.finished = finished;
    }

    public bool isFinished()
    {
        return finished;
    }

    public void setFinished(bool finished)
    {
        this.finished = finished;
    }

}
