/* An area represents the square-shaped space between four landmarks */
using System.Collections.Generic;
using UnityEngine;

public class Area
{
   
    /* An area is DEFINED by its line and column, its coordinates on the scene where (0,0) is the top left area */
    public int line, column;
    /* The time at which the participant enters and leaves the area */
    public float inTime, outTime;

    public Area(int line, int column)
    {
        this.line = line;
        this.column = column;
        inTime = -1f;
        outTime = -1f;
    }

    public Area(int line, int column, float inTime)
    {
        this.line = line;
        this.column = column;
        this.inTime = inTime;
        this.outTime = -1f;
    }

    public Area(Area area, float inTime)
    {
        this.line = area.line;
        this.column = area.column;
        this.inTime = inTime;
        this.outTime = -1f;
    }

    public Area(Area area)
    {
        this.line = area.line;
        this.column = area.column;
        this.inTime = -1f;
        this.outTime = -1f;
    }

    override public string ToString()
    {
        return "(" + line + "," + column + ")";
    }

    /* Two areas are equal iff they have the same coordinates */
    public override bool Equals(object obj)
    {
        return (obj.GetType() == typeof(Area))
            && (obj != null)
            && (((Area)obj).line == line)
            && (((Area)obj).column == column);
    }

    public override int GetHashCode() { return 100*line + column; }

    /* The total time the participant spent in the area */
    public float Time 
    {
        get { return outTime - inTime; }
    }

    public bool InBigArea()
    {
        return (line == 2 || line == 3) && (column == 3 || column == 4);
    }

    public static List<Area> BigAreaAreas()
    {
        List<Area> res = new List<Area>();
        res.Add(new Area(2, 3));
        res.Add(new Area(3, 3));
        res.Add(new Area(2, 4));
        res.Add(new Area(3, 4));
        return res;
    }

    /* The external border is the set of areas outside the trial area */
    public bool IsOnExternalBorder()
    {
        return line == 0 || line == 5 || column == 0 || column == 6;
    }

    /* The internal border is the set of the first areas behind the outermost landmarks */
    public bool IsOnInternalBorder()
    {
        return !IsOnExternalBorder() && (line == 1 || line == 4 || column == 1 || column == 5);
    }

    /* An area is in corner iff it is in a corner of the internal border */
    public bool IsInCorner()
    {
        return (line == 1 && column == 1) || (line == 1 && column == 5) || (line == 4 && column == 1) || (line == 4 && column == 5);
    }

    /* The number of decisions the participant can take at the intersection */
    public int NumberOfDecisions()
    {
        bool inBigArea = InBigArea();
        bool onExternalBorder = IsOnExternalBorder();
        bool onInternalBorder = IsOnInternalBorder();
        bool inCorner = IsInCorner();


        if (onExternalBorder)
        {
            return 0;
        }

        if (inCorner)
        {
            return 2;
        }

        if (onInternalBorder && !inCorner)
        {
            return 3;
        }

        if (!inBigArea)
        {
            return 4;
        } else
        {
            return 8;
        }
    }

    public AreaDetector GetAreaDetector()
    {
        GameObject parent = GameObject.Find("AreaDetectors");
        foreach (Transform transform in parent.transform)
        {
            AreaDetector areaDetector = transform.gameObject.GetComponent<AreaDetector>();
            if (areaDetector.Area.Equals(this))
            {
                return areaDetector;
            }
        }
        return null;
    }

}