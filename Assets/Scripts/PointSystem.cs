using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// a nested class, most likely won't be used anywhere else for now, but can always be extracted; 
// well with mouse over and mouse down events I think it's time for extraction; 
// ray casting might work? what about performance
// okay, don't worry about performance now - until it becomes a problem - then solve it
public class PointSystem
{
    public string id;

    public float x;
    public float y;
    public float z;

    public Dictionary<string, float> neighbors;
    // in fact we can turn to simply metric system, it's a design decision of whether we want the map to be accurate at all
    // decision: no, distance is still customizable, in order to simulate the hyperspace jump cost; 

    public GameObject systemInstance;

    public PointSystem()
    {
        id = "_";
        x = -1f;
        y = -1f;
        z = -1f;
        neighbors = new Dictionary<string, float>();
        systemInstance = null;
    }

    public override string ToString()
    {
        string info = "Printing Info of Point System: \n";
        info += "Id: " + id + " \n";
        info += "Location: " + x + ", " + y + ", " + z + " \n";
        info += "Neighbors: " + string.Join(", ", neighbors) + " \n";
        return info;
    }

}
