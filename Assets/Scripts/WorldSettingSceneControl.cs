using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldSettingSceneControl : MonoBehaviour
{
    bool singleSceneDebug = true;

    string WorldSettingFilePath;

    GameObject GenerateRandomClusterButton;
    GameObject starPoint;
    GameObject origin;
    GameObject hyperspaceChannel;

    IdGenerator idGenerator;

    List<PointSystem> allSystems;
    //List<GameObject> starSystems;
    List<LineRenderer> allLineRenderers;



    // Start is called before the first frame update
    void Start()
    {
        GenerateRandomClusterButton = GameObject.Find("GenerateRandomClusterButton");
        GenerateRandomClusterButton.GetComponent<Button>().onClick.AddListener(GenerateRandomCluster);

        starPoint = Resources.Load("StarPoint") as GameObject; // cause I'm too lazy to drag and drop

        origin = GameObject.Find("origin");

        hyperspaceChannel = Resources.Load("HyperspaceChannel") as GameObject;

        idGenerator = IdGenerator.getIdGenerator();

        allSystems = new List<PointSystem>();
        //starSystems = new List<GameObject>();
        allLineRenderers = new List<LineRenderer>();

        if (!singleSceneDebug)
        {
            // disable loading while working on independent functionalities
            LoadWorld();
        }
    }

    // Update is called once per frame
    void Update()
    {
        // TODO: in Update: check if any starpoint's position changed; if so, update connection - the line renderer
    }

    void LoadWorld()
    {
        if (GameObject.Find("SceneManagerControl").GetComponent<SceneManagerControl>().Mode == SceneManagerControl.LoadMode.LoadSavedWorld)
        {
            this.WorldSettingFilePath = GameObject.Find("SceneManagerControl").GetComponent<SceneManagerControl>().WorldSettingsFilePath;
            Debug.Log("Loading Specified World Setting: " + WorldSettingFilePath);
            // load the specified world setting, so other functionality can build on it
        }
        else
        {
            // create a blank file? 
            Debug.Log("Building on Brand New World");
        }
    }

    void GenerateRandomCluster()
    {
        Debug.Log("Generating Random Cluster");
        // start from one node, use a seed to generate {several} neighbors with random relative displacements, connect them with the first node; 
        // choose only a random portion of them to add more neighbors and connect to themselves respectively. 
        // second method: Generate random points, KNN, choose one of them to be the new "cluster"; 
        // anyway I'll implement as many as I can and leave the option to the user; 
        // np.random.uniform(-20, 20, size=(n,2))
        // now implementing: check doc
        middleOut();
        //instantiateStarSystems();
        UpdateConnection();
    }

    // originally only a generation method, now comes with model instantiation
    private void middleOut( int maxNeighbors = 5, // well it's more like "max" but not absolute max
                            float maxDisplacement = 10f,
                            float meanRandomDisturb = 0.0f,
                            float maxCost = 10f,
                            int totalSystems = 10)
    {
        // user can choose to limit the z-variance? so that if they want it in 2D or 3D
        //GameObject newSystem = Instantiate(starSystem, origin.transform, false) as GameObject;
        //newSystem.transform.localPosition = new Vector3(0f, 0f, 0f);
        //var x = new PointSystem();
        List<PointSystem> contours = new List<PointSystem>();
        //PointSystem firstSystem = new PointSystem();  // changed to below
        GameObject firstSystem = Instantiate(starPoint, origin.transform, false);
        PointSystem firstSystemPS = firstSystem.AddComponent<PointSystem>();
        firstSystemPS.id = idGenerator.getNextID();
        // remember to apply the xyz in PS to GO
        firstSystemPS.SetCoordinate();  // note that the default is 000 in this method, but the default in PS ini is -1;

        allSystems.Add(firstSystemPS);
        contours.Add(firstSystemPS);

        int counter = 1;
        while (counter < totalSystems)
        {
            PointSystem currentOrigin = contours[0];
            int num_neighbors = (int)(maxNeighbors * Random.Range(0f, 1f) + meanRandomDisturb * Random.Range(0f, 1f));
            // make sure there's always one neighbor, or the contours indexing will bug out
            if (num_neighbors < 1)
            {
                num_neighbors = 1;
            }
            
            float originX = currentOrigin.x;
            float originY = currentOrigin.y;
            float originZ = currentOrigin.z;

            contours.RemoveAt(0);
            for (int i = 0; i < num_neighbors; i++)
            {
                //PointSystem newNeighbor = new PointSystem();

                GameObject newNeighbor = Instantiate(starPoint, origin.transform, false);
                PointSystem newNeighborPS = newNeighbor.AddComponent<PointSystem>();
                newNeighborPS.id = idGenerator.getNextID();
                // calculate displacement from current origin
                newNeighborPS.x = originX + Random.Range(-1f, 1f) * maxDisplacement + Random.Range(-1f, 1f) * meanRandomDisturb;
                newNeighborPS.y = originY + Random.Range(-1f, 1f) * maxDisplacement + Random.Range(-1f, 1f) * meanRandomDisturb;
                newNeighborPS.z = originZ + Random.Range(-1f, 1f) * maxDisplacement + Random.Range(-1f, 1f) * meanRandomDisturb;
                // apply to GO;
                newNeighbor.transform.localPosition = new Vector3(newNeighborPS.x, newNeighborPS.y, newNeighborPS.z);
                // define distance
                float distance = Random.Range(0f, 1f) * maxCost + Random.Range(0f, 1f) * meanRandomDisturb;
                // add neighbors
                newNeighborPS.neighbors.Add(currentOrigin.id, distance);
                currentOrigin.neighbors.Add(newNeighborPS.id, distance); // may add support for inconsistent cost when direction is reversed; 
                allSystems.Add(newNeighborPS);
                contours.Add(newNeighborPS);
                //Debug.Log(newNeighbor.id);
                // Generate Connection : add line renderer, set position and add to allLineRenderers;
                string lineRendererName = currentOrigin.id + " to " + newNeighborPS.id;
                // prepare the line renderer for hyperspace channels: 
                // add as the child of the outgoing star point, the "current origin"
                LineRenderer lineRenderer = new GameObject(lineRendererName).AddComponent<LineRenderer>();
                lineRenderer.transform.SetParent(currentOrigin.gameObject.transform, true);
                lineRenderer.startWidth = 0.01f;
                lineRenderer.endWidth = 0.01f;
                lineRenderer.positionCount = 2;
                lineRenderer.useWorldSpace = false; // doubtful
                Vector3 startingSysLoc = new Vector3(currentOrigin.x, currentOrigin.y, currentOrigin.z);
                Vector3 endingSysLoc = new Vector3(newNeighborPS.x, newNeighborPS.y, newNeighborPS.z);
                lineRenderer.SetPosition(0, startingSysLoc);
                lineRenderer.SetPosition(1, endingSysLoc);
                allLineRenderers.Add(lineRenderer);  // how to locate the actual line renderer? 
                // updating connection will now take O(n) time if Find() is used? 
                // I'll leave optimisation to later
            }
            counter += num_neighbors;
        }
        //Debug.Log(allSystems.ToArray().Length);
    }

    // originally to be used with middleout, now latter is in charge of instantiation, former is changed to
    // update the connections when user edits the hyperspace channel connectivities
    // I'll archieve this, and add a new function UpdateConnection()
    private void instantiateStarSystems()
    {
        
        // generate star systems and connections
        List<(string, string)> generatedConnection = new List<(string, string)>();
        foreach (PointSystem pointSystem in allSystems)
        {
            // 加入随机生成不同的星系贴图/模型功能
            // 看看unity怎么实现类似群星的发光小球代表星系
            // 我tm直接sphere+emission
            //GameObject newSystem = Instantiate(starPoint, origin.transform, false) as GameObject;
            //newSystem.transform.localPosition = new Vector3(pointSystem.x, pointSystem.y, pointSystem.z);
            //pointSystem.systemInstance = newSystem; // assign GameObject reference; 
            //starSystems.Add(newSystem);
            // generate connections and add to the list:
            foreach (string neighborID in pointSystem.neighbors.Keys)
            {
                string lineRendererName = pointSystem.id + " to " + neighborID;
                // prepare the line renderer for hyperspace channels: 
                LineRenderer lineRenderer = new GameObject(lineRendererName).AddComponent<LineRenderer>();
                lineRenderer.startColor = Color.black;
                lineRenderer.endColor = Color.black;
                lineRenderer.startWidth = 0.01f;
                lineRenderer.endWidth = 0.01f;
                lineRenderer.positionCount = 2;
                lineRenderer.useWorldSpace = true; // doubtful
                PointSystem neighborSys = allSystems.Find(sys => sys.id == neighborID);  // maybe change allSys to Dict for faster lookup?
                Vector3 startingSysLoc = new Vector3(pointSystem.x, pointSystem.y, pointSystem.z);
                Vector3 endingSysLoc = new Vector3(neighborSys.x, neighborSys.y, neighborSys.z);
                lineRenderer.SetPosition(0, startingSysLoc);
                lineRenderer.SetPosition(1, endingSysLoc);
            }
        }

    }

    // default is to render the hyperspace channels
    private void UpdateConnection(List<PointSystem> affectedSystems = null)
    {
        if (affectedSystems == null)
        {
            Debug.Log("Null Checked, regenerating all connections");
        }
    }

    
}
