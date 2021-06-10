using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path
{
    public string Name { get; set; }
    private List<Area> areas;

    public Path()
    {
        areas = new List<Area>();
    }

    public Path(List<Area> areas)
    {
        this.areas = areas;
    }

    public Path(string strPath)
    {
        areas = new List<Area>();
        Name = strPath.Substring(0, 1);
        string[] strCoords = strPath.Substring(1, strPath.Length - 1).Split('.');
        int line, column;
        foreach (string strCoord in strCoords)
        {
            line = int.Parse(strCoord.Substring(0, 1));
            column = int.Parse(strCoord.Substring(1, 1));
            areas.Add(new Area(line, column));
        }
    }

    public int CountArea(Area area)
    {
        int res = 0;
        foreach (Area a in areas)
        {
            if (a.Equals(area))
            {
                res++;
            }
        }
        return res;
    }

    public int FirstOccurrence(Area area)
    {
        int res = -1;
        for (int i = 0; i < areas.Count; i++)
        {
            if (areas[i].Equals(area))
            {
                res = i;
                break;
            }
        }
        return res;
    }
    public bool Contains(Area area)
    {
        return areas.Contains(area);
    }

    public Area Get(int i)
    {
        return areas[i];
    }

    public Area GetLast()
    {
        if (areas.Count > 0)
        {
            return areas[areas.Count - 1];
        }
        else
        {
            return null;
        }
    }

    public int Count()
    {
        return areas.Count;
    }

    public void Add(Area area)
    {
        areas.Add(area);
    }

    public void RemoveLast()
    {
        if (areas.Count > 0)
        {
            areas.RemoveAt(areas.Count - 1);
        } else
        {
            Debug.LogError("Attempting to remove area from trialPath but trialPath is empty");
        }
    }

    public void Remove(Area area)
    {
        areas.Remove(area);
    }

    public void Clear()
    {
        areas.Clear();
    }

    override public string ToString()
    {
        string res = "";
        if (Name != null)
        {
            res += Name;
        }
        for (int i = 0; i < areas.Count; i++)
        {
            res += areas[i].line + "" + areas[i].column;
            if (i != areas.Count - 1)
            {
                res += ".";
            }
        }
        return res;
    }

    public Path Join(Path p)
    {
        List<Area> joinedAreas = new List<Area>();
        joinedAreas.AddRange(areas);
        joinedAreas.AddRange(p.areas);
        return new Path(joinedAreas);
    }

    public Area Pop()
    {
        Area area = areas[0];
        areas.RemoveAt(0);
        return area;
    }

    public void Push(Area area)
    {
        areas.Insert(0, area);
    }

    public List<Area> AreasInBigArea()
    {
        List<Area> res = new List<Area>();
        foreach (Area area in areas)
        {
            if (area.InBigArea())
            {
                res.Add(area);
            }
        }
        return res;
    }
}
