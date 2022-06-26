using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubOrigin : MonoBehaviour
{

    public string subOriginID;

    // all star systems under this sub origin;
    // maybe we can give user the choice to group some clusters? make new origins? fuck
    List<PointSystem> clusterSystems;

    public string SubOriginID { get => subOriginID; set => subOriginID = value; }
    public List<PointSystem> ClusterSystems { get => clusterSystems; set => clusterSystems = value; }

    // Start is called before the first frame update
    void Start()
    {
        //subOriginID = "";
        //clusterSystems = new List<PointSystem>();
    }

    void Awake()
    {
        clusterSystems = new List<PointSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
