using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Represents an advice : arrow, light path or PEANUT */
public class Advice
{
    public Area Area { 
        get { return area; }
    }
    // Where the advice is to spawn
    private Area area;
    // Advice prefab
    private GameObject advice;

    public Advice(Area area, GameObject advicePrefab, Vector3 position, Vector3 rotation)
    {
        this.area = new Area(area);
        this.advice = GameObject.Instantiate(advicePrefab, position, Quaternion.Euler(rotation));
    }

    public Advice(Area area, GameObject advicePrefab, Vector3 position, Vector3 rotation, int animState)
    {
        this.area = new Area(area);
        this.advice = GameObject.Instantiate(advicePrefab, position, Quaternion.Euler(rotation));
        advice.GetComponent<Animator>().SetInteger("State", animState);
    }

    // Removes the advice from scene and memory
    public void Remove()
    {
        GameObject.Destroy(advice);
    }
}
