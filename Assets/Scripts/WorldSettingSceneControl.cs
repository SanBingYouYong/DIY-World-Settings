using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;
using Cinemachine;


public class WorldSettingSceneControl : MonoBehaviour
{
    bool singleSceneDebug = true;

    GameObject GenerateRandomClusterButton;
    GameObject starPoint;
    GameObject origin;

    /// <summary>
    /// Fake Main Cam: VCAM focusing on it, and its movement represents the actual cam movement. 
    /// </summary>
    public GameObject fakeMainCam;
    private Movement fakeMainCamScript;

    GameObject arrowX;
    GameObject arrowY;
    GameObject arrowZ;

    string lastClickedStarPointID;
    bool newStarPointClicked; // regard every click as a new click;

    Toggle moveWholeClusterToggle;
    Toggle generateNewClusterToggle;

    // The Save & Load functionalities
    GameObject saveWorldConfigButton;
    string savePath;
    GameObject loadWorldConfigButton;
    string loadPath;

    PointSystem lastOriginPS;

    /// <summary>
    /// The reference to the singleton ID Generator. 
    /// </summary>
    IdGenerator idGenerator;

    /// <summary>
    /// A record of all the line renderers, belonging to different connections. 
    /// </summary>
    List<LineRenderer> allLineRenderers;

    public GameObject infoPanel;

    public GameObject scrollview;
    public GameObject scrollviewContent;
    public GameObject overviewContentItem;

    public CinemachineVirtualCamera vcam;

    ///////////////////////////////////////////////////
    ///Property-Related Fields
    ///////////////////////////////////////////////////
    public InputField starSystemNameDynamic;
    public InputField starTypeDynamic;
    public InputField planetCountDynamic;
    public InputField heldByDynamic;

    public GameObject ArrowX { get => arrowX; set => arrowX = value; }
    public GameObject ArrowY { get => arrowY; set => arrowY = value; }
    public GameObject ArrowZ { get => arrowZ; set => arrowZ = value; }
    public string LastClickedStarPointID { get => lastClickedStarPointID; set => lastClickedStarPointID = value; }
    public bool NewStarPointClicked { get => newStarPointClicked; set => newStarPointClicked = value; }
    public Toggle MoveWholeClusterToggle { get => moveWholeClusterToggle; set => moveWholeClusterToggle = value; }



    // Start is called before the first frame update
    void Start()
    {
        // following can be changed to editor-oriented drag and drops: 
        GenerateRandomClusterButton = GameObject.Find("GenerateRandomClusterButton");
        GenerateRandomClusterButton.GetComponent<Button>().onClick.AddListener(GenerateRandomCluster);

        starPoint = Resources.Load("StarPoint") as GameObject;

        origin = GameObject.Find("origin");

        ArrowX = Resources.Load("Arrow_X") as GameObject;
        ArrowY = Resources.Load("Arrow_Y") as GameObject;
        ArrowZ = Resources.Load("Arrow_Z") as GameObject;

        fakeMainCamScript = fakeMainCam.GetComponent<Movement>();

        newStarPointClicked = false;

        MoveWholeClusterToggle = GameObject.Find("MoveWholeClusterToggle").GetComponent<Toggle>();
        generateNewClusterToggle = GameObject.Find("GenerateNewClusterToggle").GetComponent<Toggle>();

        saveWorldConfigButton = GameObject.Find("SaveWorldConfigButton");
        saveWorldConfigButton.GetComponent<Button>().onClick.AddListener(SaveWorldConfigToCsv);

        loadWorldConfigButton = GameObject.Find("LoadWorldConfigButton");
        loadWorldConfigButton.GetComponent<Button>().onClick.AddListener(LoadWorldConfigFromCsv);

        idGenerator = IdGenerator.getIdGenerator();

        allLineRenderers = new List<LineRenderer>();

        // TODO: reconsider the modes design; works for now actually
        if (!singleSceneDebug)
        {
            // disable loading while working on independent functionalities
            LoadWorld();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (newStarPointClicked)
        {
            if (!infoPanel.activeSelf)
            {
                infoPanel.SetActive(true); // make visible the info panel
            }
            PointSystem clickedPS = lastOriginPS; //TODO:  won't avoid null exception tho? needs testing
            foreach (PointSystem PS in origin.GetComponent<Origin>().AllStarSystems)
            {
                // if not left alone, this will destroy the new arrows as well... 
                if (PS.pointSystemID != lastClickedStarPointID)
                {
                    PS.DestroyArrows();
                }
                else
                {
                    clickedPS = PS;
                }
            }
            newStarPointClicked = false;
            // after the arrow cleaning and rendering process,
            //     following is updating the info panel on the new clicked/chosen point system
            starSystemNameDynamic.text = clickedPS.StarSystemName;
            starTypeDynamic.text = clickedPS.StarType.ToString(); // TODO: may need a customized to string method
            planetCountDynamic.text = clickedPS.PlanetCount.ToString();
            heldByDynamic.text = clickedPS.Holder;
            // to unlock cam when first clicked on a new ps: 
            fakeMainCamScript.moving = true;
        }
    }

    void LoadWorld()
    {
        if (GameObject.Find("SceneManagerControl").GetComponent<SceneManagerControl>().Mode == 
            SceneManagerControl.LoadMode.LoadSavedWorld)
        {
            LoadWorldConfigFromCsv();
        }
        else
        {
            // TODO: create a blank file for autosaves; the autosave functionality isn't very prioritized
            Debug.Log("Building on Brand New World");
        }
    }

    /// <summary>
    /// The manual update method of the overview panel. 
    /// It depopulates the current contents first, and then rebuild the contents with info from origin.AllStarSystems
    /// </summary>
    void PopulateOverviewScroll()
    {
        DepopulateOverviewScroll();
        scrollview.SetActive(true);
        foreach (PointSystem ps in origin.GetComponent<Origin>().AllStarSystems)
        {
            var item = Instantiate(overviewContentItem);
            item.transform.SetParent(scrollviewContent.transform, false);
            item.GetComponent<OverviewItem>().UpdateStats(ps);
            item.GetComponent<OverviewItem>().ps = ps;
        }
    }

    /// <summary>
    /// Depopulates by destroying all the children GameObject under scrollview's "Content"
    /// </summary>
    void DepopulateOverviewScroll()
    {
        foreach (Transform child in scrollviewContent.transform)
        {
            Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// Make the camera focus to a new GameObject. 
    /// Involves the movement of fake camera and the setting of Follow and LookAt of vcam. 
    /// </summary>
    /// <param name="tr">The target GameObject's Transform. </param>
    public void FocusToGO(Transform tr)
    {
        // TODO: use a Coroutine to make the transition smooth; 
        // tried several approaches for smooth transitions: 
        //fakeMainCam.transform.position = Vector3.Lerp(fakeMainCam.transform.position, tr.position, Time.deltaTime * fakeMainCamScript.speed);
        //fakeMainCam.transform.position = Vector3.MoveTowards(fakeMainCam.transform.position, tr.position, )
        vcam.Follow = tr;
        vcam.LookAt = tr;
        fakeMainCam.transform.position = tr.position;
        vcam.Follow = fakeMainCam.transform;
        vcam.LookAt = fakeMainCam.transform;
    }

    /// <summary>
    /// Currently only a middleout implementation is available. 
    /// </summary>
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
        PopulateOverviewScroll();
    }

    /// <summary>
    /// Cluster generation and model instantiation. Check the doc for a detailed algorithmic description. 
    /// TODO: User can choose to limit the z-variance, if they want it in 2D. 
    /// </summary>
    /// <param name="maxNeighbors">"max" but not absolute max</param>
    /// <param name="maxDisplacement"></param>
    /// <param name="meanRandomDisturb"></param>
    /// <param name="maxCost"></param>
    /// <param name="totalSystems"></param>
    private void middleOut( int maxNeighbors = 5,
                            float maxDisplacement = 10f,
                            float meanRandomDisturb = 0.0f,
                            float maxCost = 10f,
                            int totalSystems = 10)
    {
        // contours: acts like a queue: new systems that need to be expanded on get added to it,
        //     and in the while loop, the first element is in use and then gets poped out, until the counter stops the process. 
        List<PointSystem> contours = new List<PointSystem>();
        // Determine whether to generate new origin, or to use the last origin as current origin;
        // note: id == 0 doesn't mean it's the current origin
        // note: origin is not touched. SubOrigins, the children of origin, are in charge of the job. 
        GameObject activeSubOriginGO;
        if (generateNewClusterToggle.isOn)
        {
            SubOrigin newSubOriginSO;
            InitializeNewSubOrigin(out activeSubOriginGO, out newSubOriginSO);
            InitializeNewFirstSystem(contours, activeSubOriginGO, newSubOriginSO);
        }
        else
        {
            contours.Add(lastOriginPS);
            // find the correct sub origin; 
            activeSubOriginGO = origin.GetComponent<Origin>().SubOrigins.Find(so => so.SubOriginID == lastOriginPS.SubOriginID).gameObject;
        }
        // TODO: give user the choice to determine which sub origin they want to add to exactly. 

        int counter = 1; // first system has been ini'd, so 1 instead of 0
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
            // pop: 
            contours.RemoveAt(0);
            // Generate neighbors for the current PS: 
            for (int i = 0; i < num_neighbors; i++)
            {
                GameObject newNeighbor;
                PointSystem newNeighborPS;
                InitializeNewNeighbor(activeSubOriginGO, currentOrigin, out newNeighbor, out newNeighborPS);
                AssignStatsToNewNeighbor(maxDisplacement, meanRandomDisturb, maxCost, currentOrigin, 
                                            originX, originY, originZ, newNeighbor, newNeighborPS);
                contours.Add(newNeighborPS);
                // a forced update on origin pos
                // to sync the actual position and the recorded position. 
                currentOrigin.SetCoordinate(currentOrigin.gameObject.transform.position);
                newNeighborPS.SetCoordinate(newNeighborPS.gameObject.transform.position);
                // above fix is ugly: still needs to figure out a way to update on all PS if moving all; 
                // update: check TODO in Arrow.cs: OnMouseUp needs to notify all if moving all together! 
                // update: now the sub origin is moved, so its children get moved as a whole too. 
                InstantiateHyperspaceConnection(currentOrigin, newNeighborPS);
            }
            counter += num_neighbors;
        }
    }

    /// <summary>
    /// Given two connected and initiated point system, render the connection between them. 
    /// Add line renderer, set position and add to allLineRenderers;
    /// </summary>
    /// <param name="currentOrigin">from ps</param>
    /// <param name="newNeighborPS">to ps</param>
    private void InstantiateHyperspaceConnection(PointSystem currentOrigin, PointSystem newNeighborPS)
    {
        string lineRendererName = currentOrigin.pointSystemID + " to " + newNeighborPS.pointSystemID;
        // prepare the line renderer for hyperspace channels: 
        // add as the child of the outgoing star point, the "current origin"
        LineRenderer lineRenderer = new GameObject(lineRendererName).AddComponent<LineRenderer>();
        lineRenderer.transform.SetParent(currentOrigin.gameObject.transform, true);
        lineRenderer.startWidth = 0.01f;
        lineRenderer.endWidth = 0.01f;
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = false; // doubtful: if use worldPos: whole move x; if localPos: single move x
        Vector3 startingSysLoc = new Vector3(currentOrigin.x, currentOrigin.y, currentOrigin.z); // maybe add a forced update on xyz, where? middleout..?
        Vector3 endingSysLoc = new Vector3(newNeighborPS.x, newNeighborPS.y, newNeighborPS.z);
        lineRenderer.SetPosition(0, startingSysLoc);
        lineRenderer.SetPosition(1, endingSysLoc);
        allLineRenderers.Add(lineRenderer); 
        currentOrigin.AllEdges.Add(lineRenderer);
        newNeighborPS.AllEdges.Add(lineRenderer);
        // the allLineRenders is still reserved, as this is the actual "whole list" - those from PS will have overlap
    }

    /// <summary>
    /// Given several parameters, generate and set property of a new neighboring PS
    /// </summary>
    /// <param name="maxDisplacement"></param>
    /// <param name="meanRandomDisturb"></param>
    /// <param name="maxCost"></param>
    /// <param name="currentOrigin">the ps that generated this new neighbor</param>
    /// <param name="originX"></param>
    /// <param name="originY"></param>
    /// <param name="originZ"></param>
    /// <param name="newNeighbor"></param>
    /// <param name="newNeighborPS"></param>
    private void AssignStatsToNewNeighbor(float maxDisplacement, float meanRandomDisturb, float maxCost, 
                                            PointSystem currentOrigin, float originX, float originY, float originZ, 
                                            GameObject newNeighbor, PointSystem newNeighborPS)
    {
        // calculate displacement from current origin
        newNeighborPS.x = originX + Random.Range(-1f, 1f) * maxDisplacement + Random.Range(-1f, 1f) * meanRandomDisturb;
        newNeighborPS.y = originY + Random.Range(-1f, 1f) * maxDisplacement + Random.Range(-1f, 1f) * meanRandomDisturb;
        newNeighborPS.z = originZ + Random.Range(-1f, 1f) * maxDisplacement + Random.Range(-1f, 1f) * meanRandomDisturb;
        // apply to GO;
        newNeighbor.transform.localPosition = new Vector3(newNeighborPS.x, newNeighborPS.y, newNeighborPS.z);
        // define distance
        float distance = Random.Range(0f, 1f) * maxCost + Random.Range(0f, 1f) * meanRandomDisturb;
        // add neighbors
        newNeighborPS.neighbors.Add(currentOrigin.pointSystemID, distance);
        currentOrigin.neighbors.Add(newNeighborPS.pointSystemID, distance); // may add support for inconsistent cost when direction is reversed; 
    }

    /// <summary>
    /// Given an initialized ps with its sub origin, instantiate a new neighbor and 
    ///     add it to origin and sub origin's list of all star systems
    /// </summary>
    /// <param name="activeSubOriginGO"></param>
    /// <param name="currentOrigin">the ps that this new neighbor belongs to</param>
    /// <param name="newNeighbor"></param>
    /// <param name="newNeighborPS"></param>
    private void InitializeNewNeighbor(GameObject activeSubOriginGO, PointSystem currentOrigin, out GameObject newNeighbor, out PointSystem newNeighborPS)
    {
        newNeighbor = Instantiate(starPoint, activeSubOriginGO.transform, false);
        newNeighborPS = newNeighbor.GetComponent<PointSystem>();
        newNeighborPS.pointSystemID = idGenerator.getNextPointSystemID();
        newNeighborPS.SubOriginID = currentOrigin.SubOriginID;
        origin.GetComponent<Origin>().AllStarSystems.Add(newNeighborPS);
        activeSubOriginGO.GetComponent<SubOrigin>().ClusterSystems.Add(newNeighborPS);
    }

    /// <summary>
    /// Given a sub origin, generate the first system in the cluster and adds it the contour for neighbor generation. 
    /// </summary>
    /// <param name="contours">the object ref to the list of contours</param>
    /// <param name="activeSubOriginGO"></param>
    /// <param name="newSubOriginSO">This param can be replaced with a manual extraction from the soGO but anyway</param>
    private void InitializeNewFirstSystem(List<PointSystem> contours, GameObject activeSubOriginGO, SubOrigin newSubOriginSO)
    {
        GameObject firstSystem = Instantiate(starPoint, activeSubOriginGO.transform, false);
        PointSystem firstSystemPS = firstSystem.GetComponent<PointSystem>();
        firstSystemPS.pointSystemID = idGenerator.getNextPointSystemID();
        firstSystemPS.SubOriginID = newSubOriginSO.SubOriginID;
        firstSystemPS.SetCoordinate();  // note that the default is 000 in this method, but the default in PS ini is -1;
        origin.GetComponent<Origin>().AllStarSystems.Add(firstSystemPS);
        newSubOriginSO.ClusterSystems.Add(firstSystemPS);
        contours.Add(firstSystemPS);
        lastOriginPS = firstSystemPS;
    }

    private void InitializeNewStarSystem(SubOrigin newSubOriginSO)
    {
        GameObject firstSystem = Instantiate(starPoint, newSubOriginSO.transform, false);
        PointSystem firstSystemPS = firstSystem.GetComponent<PointSystem>();
        firstSystemPS.pointSystemID = idGenerator.getNextPointSystemID();
        firstSystemPS.SubOriginID = newSubOriginSO.SubOriginID;
        firstSystemPS.SetCoordinate();  // note that the default is 000 in this method, but the default in PS ini is -1;
        origin.GetComponent<Origin>().AllStarSystems.Add(firstSystemPS);
        newSubOriginSO.ClusterSystems.Add(firstSystemPS);
        lastOriginPS = firstSystemPS;
    }

    /// <summary>
    /// Initialize the sub origin for the cluster to be generated. 
    /// </summary>
    /// <param name="activeSubOriginGO"></param>
    /// <param name="newSubOriginSO"></param>
    private void InitializeNewSubOrigin(out GameObject activeSubOriginGO, out SubOrigin newSubOriginSO)
    {
        string newSubOriginID = idGenerator.getNextSubOriginID();
        activeSubOriginGO = new GameObject("SubOrigin" + newSubOriginID);
        newSubOriginSO = activeSubOriginGO.AddComponent<SubOrigin>();
        newSubOriginSO.SubOriginID = newSubOriginID;
        activeSubOriginGO.transform.SetParent(origin.transform, false);
        origin.GetComponent<Origin>().SubOrigins.Add(newSubOriginSO);
    }

    /// <summary>
    /// Add one single point system to the map. 
    /// </summary>
    public void AddStarSystem()
    {
        // try to find a valid sub origin to be spawned into, if no, ini a new SO
        GameObject intendedSubOriginGO;
        SubOrigin intendedSubOrigin;
        if (lastOriginPS == null)
        {
            InitializeNewSubOrigin(out intendedSubOriginGO, out intendedSubOrigin);
        }
        else
        {
            intendedSubOrigin = origin.GetComponent<Origin>().SubOrigins.Find(so => so.SubOriginID == lastOriginPS.SubOriginID);
        }
        InitializeNewStarSystem(intendedSubOrigin);
    }

    /// <summary>
    /// Add a connection between two PS. If a connection already exists, do nothing. 
    /// </summary>
    /// <param name="fromPS"></param>
    /// <param name="toPS"></param>
    /// <param name="dist"></param>
    public void AddConnectionBetween(PointSystem fromPS, PointSystem toPS, float dist)
    {
        // || or && both work, but in case some former logic broke, an && will be safer
        if (fromPS.neighbors.ContainsKey(toPS.pointSystemID) && toPS.neighbors.ContainsKey(fromPS.pointSystemID))
        {
            Debug.Log("Connection already present, skipping the addition");
        }
        fromPS.neighbors.Add(toPS.pointSystemID, dist);
        toPS.neighbors.Add(fromPS.pointSystemID, dist);
        InstantiateHyperspaceConnection(fromPS, toPS);
    }

    /// <summary>
    /// Update the target ps's connection (edges) after the ps has been moved to a new location. 
    /// </summary>
    /// <param name="ps"></param>
    public void UpdateConnection(PointSystem ps)
    {
        Debug.Log("Updating connection at Point System: " + ps.pointSystemID);
        foreach (LineRenderer lr in ps.AllEdges)
        {
            // this might or might not mess up the direction, but now it's undirected anyway, well the child relationship means nothing now
            // but they are now using world pos anyway
            // TODO: move all line renderers to suborigin maybe
            (string firstID, string secondID) = GetTerminalsByChannelName(lr.gameObject.name);
            // find the sub origin this is all happening
            SubOrigin terminalSO = origin.GetComponent<Origin>().SubOrigins.Find(so => so.subOriginID == ps.SubOriginID);
            if (firstID == ps.pointSystemID)
            {
                // line renderer is using local position, thus converting from world to local
                lr.SetPosition(0, lr.transform.InverseTransformPoint(ps.gameObject.transform.position));
                // find the terminal PS
                //PointSystem terminalPS = terminalSO.ClusterSystems.Find(ps => ps.pointSystemID == firstID);
                //PointSystem terminalPS = origin.GetComponent<Origin>().AllStarSystems.Find(ps => ps.pointSystemID == secondID);
                // changed from a all system search to a suborigin search
                PointSystem terminalPS = terminalSO.ClusterSystems.Find(ps => ps.pointSystemID == secondID);
                if (terminalPS == null)
                {
                    Debug.Log("Failed to find terminal PS with ID " + secondID + "; pairing with " + firstID);
                }
                lr.SetPosition(1, lr.transform.InverseTransformPoint(terminalPS.gameObject.transform.position));
            }
            else // then it's the in-going edge
            {
                // do we actually need the lr pos 0 update? anyway
                PointSystem terminalPS = terminalSO.ClusterSystems.Find(ps => ps.pointSystemID == firstID);
                if (terminalPS == null)
                {
                    Debug.Log("Terminal PS Not Found: " + firstID);
                }
                lr.SetPosition(0, lr.transform.InverseTransformPoint(terminalPS.gameObject.transform.position));
                lr.SetPosition(1, lr.transform.InverseTransformPoint(ps.gameObject.transform.position));
            }
        }
    }

    /// <summary>
    /// e.g. input: "10 to 15"; output: ("10", "15")
    /// </summary>
    /// <param name="channelName"></param>
    /// <returns></returns>
    private (string,string) GetTerminalsByChannelName(string channelName)
    {
        var splits = channelName.Split(' ');
        Debug.Log("Retrieving from " + channelName);
        Debug.Log("Got " + splits[0] + " and " + splits[splits.Length - 1]);
        return (splits[0], splits[splits.Length - 1]);
    }

    // Save and Load methods and coroutines: 

    private void SaveWorldConfigToCsv()
    {
        StartCoroutine(ShowSaveDialogCoroutine());
    }

    IEnumerator ShowSaveDialogCoroutine()
    {
        yield return FileBrowser.WaitForSaveDialog(FileBrowser.PickMode.Files);
        if (FileBrowser.Success)
        {
            savePath = FileBrowser.Result[0];
            SaveWorldConfigToCsvAfterSaveDialog();
        }
    }

    private void SaveWorldConfigToCsvAfterSaveDialog()
    {
        string tempPath = savePath;
        StreamWriter writer = new StreamWriter(tempPath, false, System.Text.Encoding.UTF8); // to the file path, overwrites, utf-8 encoding
        Debug.Log("writer initialized");
        // insert some identification data first? like name of this world blablabla
        // TODO: adds support for custom star types, like a header claiming all enum types of startype
        foreach (PointSystem ps in origin.GetComponent<Origin>().AllStarSystems)
        {
            Debug.Log("writing new ps");
            writer.WriteLine(ps.ToCsvString());
        }
        Debug.Log("writing done");
        writer.Close();
        Debug.Log("writer closed");
    }

    private void LoadWorldConfigFromCsv()
    {
        StartCoroutine(ShowLoadDialogCoroutine());

    }

    IEnumerator ShowLoadDialogCoroutine()
    {
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Files);
        if (FileBrowser.Success)
        {
            loadPath = FileBrowser.Result[0];
            LoadWorldConfigFromCsvAfterLoadDialog();
        }
    }

    /// <summary>
    /// Destroys the current world and load the selected new one. 
    /// </summary>
    private void LoadWorldConfigFromCsvAfterLoadDialog()
    {

        DestroyCurrentWorldSetting();
        // TODO: add a backup functionality, since now the whole save will be overwritten
        string tempPath = loadPath;
        StreamReader reader = new StreamReader(tempPath, true);
        while (!reader.EndOfStream)
        {
            List<string> line = reader.ReadLine().Split(',').ToList();
            string psID = line[0];
            string soID = line[1];
            float x = float.Parse(line[2]);
            float y = float.Parse(line[3]);
            float z = float.Parse(line[4]);
            string starSysName = line[5];
            PointSystem.StarSpectalType starType = (PointSystem.StarSpectalType)System.Enum.Parse(typeof(PointSystem.StarSpectalType), line[6]);
            int planetCount = int.Parse(line[7]);
            string holder = line[8];
            var skimForNeighbors = line.Skip(9); // 8 + 1 for indexing from 0 // always cast invalid: IEnumerable to any, idk why, left it as var
            Queue<string> neighborsLine = new Queue<string>(skimForNeighbors);
            Dictionary<string, float> neighbors = new Dictionary<string, float>();
            while (neighborsLine.Count >= 2)
            {
                neighbors.Add(neighborsLine.Dequeue(), float.Parse(neighborsLine.Dequeue()));
            }

            GameObject subOriginGO;
            // get the sub origin, if there isn't one matching the id, create a new one: 
            if (origin.GetComponent<Origin>().SubOrigins.Any(so => so.subOriginID == soID))
            {
                subOriginGO = origin.GetComponent<Origin>().SubOrigins.Find(so => so.subOriginID == soID).gameObject;
            }
            else
            {
                subOriginGO = new GameObject("SubOrigin" + soID);
                SubOrigin subOrigin = subOriginGO.AddComponent<SubOrigin>();
                subOrigin.SubOriginID = soID;
                subOriginGO.transform.SetParent(origin.transform, false);
                origin.GetComponent<Origin>().SubOrigins.Add(subOrigin);
            }
            // create a corresponding ps
            GameObject psGO = Instantiate(starPoint, subOriginGO.transform, false);
            PointSystem ps = psGO.GetComponent<PointSystem>();
            ps.pointSystemID = psID;
            ps.SubOriginID = soID;
            ps.SetCoordinate(x, y, z);
            ps.StarSystemName = starSysName;
            ps.StarType = starType;
            ps.PlanetCount = planetCount;
            ps.Holder = holder;
            ps.neighbors = neighbors; // TODO: maybe a deep copy? 
            psGO.transform.localPosition = new Vector3(x, y, z);
            origin.GetComponent<Origin>().AllStarSystems.Add(ps);
            subOriginGO.GetComponent<SubOrigin>().ClusterSystems.Add(ps);
        }
        // 我 tm n^3
        foreach (PointSystem ps in origin.GetComponent<Origin>().AllStarSystems)
        {
            foreach (KeyValuePair<string, float> neighbor in ps.neighbors)
            {
                PointSystem neighborPS = origin.GetComponent<Origin>().AllStarSystems.Find(p => p.pointSystemID == neighbor.Key);
                // 还是会ini两遍同一个connection, 可以在方法里加入一个不重复的判定, 但同样会导致不稳定: 从谁到谁这个记录在存档的时候可能会改变
                // 那么到底是不稳定记录还是重复记录呢 好问题 我不知道
                // re-use of the method
                InstantiateHyperspaceConnection(ps, neighborPS);
            }
        }
        // update overview panel
        PopulateOverviewScroll();
    }

    /// <summary>
    /// Disables all in-memory stats currently in Origin and others
    /// </summary>
    private void DestroyCurrentWorldSetting()
    {
        Debug.Log("Clearing Stats stored now in origins");
        // TODO: maybe suborigins need to be saved as well? their positions to origin
        // their positions are not recorded now anyways, hmm it's strange
        // TODO: idGenerator... alas, what do I do with the singleton,
        // I mean, id starting from non-zero is better than repeated ID... 
        origin.GetComponent<Origin>().SubOrigins.Clear();
        origin.GetComponent<Origin>().AllStarSystems.Clear();
        foreach (Transform childTransform in origin.transform)
        {
            Destroy(childTransform.gameObject);
        }
    }

    /// <summary>
    /// Stops the camera movements while editing the info (typing). 
    /// Since no InputField.OnEditStart is available, it sets it again and again. 
    /// </summary>
    public void EditDuring()
    {
        if (fakeMainCamScript.moving)
        {
            fakeMainCamScript.moving = false;
        }
    }

    /// <summary>
    /// Update star system config after editing. Unlocks the camera. 
    /// </summary>
    public void EditEnd()
    {
        fakeMainCamScript.moving = true;
        var curPS = origin.GetComponent<Origin>().AllStarSystems.Find(ps => ps.pointSystemID == lastClickedStarPointID);
        EditPSSaveConfig(curPS);
    }

    /// <summary>
    /// Save the information to the edited PS. The weird name is to follow the same Editxxx convention.
    /// </summary>
    /// <param name="curPS"></param>
    private void EditPSSaveConfig(PointSystem curPS)
    {
        curPS.StarSystemName = starSystemNameDynamic.text; // no need to update text since input field does that for us
        //curPS.StarType = starTypeDynamic.text; // f it's gotta be a drop down, check the TODOs
        curPS.PlanetCount = int.Parse(planetCountDynamic.text);
        curPS.Holder = heldByDynamic.text;
    }
}
