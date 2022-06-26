using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Origin : MonoBehaviour
{

    string originID;  // needed? it should be the only one, unless we have implemented the "customizable group clustered"

    // hold the information of all sub origins: 
    List<SubOrigin> subOrigins;

    // all star systems, no matter which suborigin it is:
    List<PointSystem> allStarSystems;

    public List<SubOrigin> SubOrigins { get => subOrigins; set => subOrigins = value; }
    public List<PointSystem> AllStarSystems { get => allStarSystems; set => allStarSystems = value; }

    // Start is called before the first frame update
    void Start()
    {
        subOrigins = new List<SubOrigin>();
        allStarSystems = new List<PointSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
