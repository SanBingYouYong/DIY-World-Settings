using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldSettingSceneControl : MonoBehaviour
{
    bool singleSceneDebug = true;

    string WorldSettingFilePath;

    GameObject GenerateRandomClusterButton;
    GameObject starSystem;
    GameObject origin;
    GameObject hyperspaceChannel;

    IdGenerator idGenerator;

    List<PointSystem> allSystems;
    List<GameObject> starSystems;


    // Start is called before the first frame update
    void Start()
    {
        GenerateRandomClusterButton = GameObject.Find("GenerateRandomClusterButton");
        GenerateRandomClusterButton.GetComponent<Button>().onClick.AddListener(GenerateRandomCluster);

        starSystem = Resources.Load("StarPoint") as GameObject; // cause I'm too lazy to drag and drop

        origin = GameObject.Find("origin");

        hyperspaceChannel = Resources.Load("HyperspaceChannel") as GameObject;

        idGenerator = IdGenerator.getIdGenerator();

        allSystems = new List<PointSystem>();
        starSystems = new List<GameObject>();

        if (!singleSceneDebug)
        {
            // disable loading while working on independent functionalities
            LoadWorld();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
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
        instantiateStarSystems();
    }

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
        PointSystem firstSystem = new PointSystem();
        firstSystem.id = idGenerator.getNextID();
        allSystems.Add(firstSystem);
        contours.Add(firstSystem);
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
                PointSystem newNeighbor = new PointSystem();
                newNeighbor.id = idGenerator.getNextID();
                // calculate displacement from current origin
                newNeighbor.x = originX + Random.Range(-1f, 1f) * maxDisplacement + Random.Range(-1f, 1f) * meanRandomDisturb;
                newNeighbor.y = originY + Random.Range(-1f, 1f) * maxDisplacement + Random.Range(-1f, 1f) * meanRandomDisturb;
                newNeighbor.z = originZ + Random.Range(-1f, 1f) * maxDisplacement + Random.Range(-1f, 1f) * meanRandomDisturb;
                // define distance
                float distance = Random.Range(0f, 1f) * maxCost + Random.Range(0f, 1f) * meanRandomDisturb;
                // add neighbors
                newNeighbor.neighbors.Add(currentOrigin.id, distance);
                currentOrigin.neighbors.Add(newNeighbor.id, distance); // may add support for inconsistent cost when direction is reversed; 
                allSystems.Add(newNeighbor);
                contours.Add(newNeighbor);
                //Debug.Log(newNeighbor.id);
            }
            counter += num_neighbors;
        }
        //Debug.Log(allSystems.ToArray().Length);
    }

    private void instantiateStarSystems()
    {
        
        // generate star systems and connections
        List<(string, string)> generatedConnection = new List<(string, string)>();
        foreach (PointSystem pointSystem in allSystems)
        {
            // 加入随机生成不同的星系贴图/模型功能
            // 看看unity怎么实现类似群星的发光小球代表星系
            // 我tm直接sphere+emission
            GameObject newSystem = Instantiate(starSystem, origin.transform, false) as GameObject;
            newSystem.transform.localPosition = new Vector3(pointSystem.x, pointSystem.y, pointSystem.z);
            pointSystem.systemInstance = newSystem; // assign GameObject reference; 
            starSystems.Add(newSystem);
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
}
