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

    private void InstantiateHyperspaceConnection(PointSystem currentOrigin, PointSystem newNeighborPS)
    {
        // Generate Connection : add line renderer, set position and add to allLineRenderers;
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
        allLineRenderers.Add(lineRenderer);  // how to locate the actual line renderer? 
                                             // updating connection will now take O(n) time if Find() is used? 
                                             // I'll leave optimisation to later
                                             // update: will have a field in PS, holding its outgoing edges
                                             // but they are already moving with the star (parent/child)
                                             // 
                                             // update: now every PS holds the list of all edges that has something to do with it; 
                                             // thus when this PS is moved alone, all edges can be updated
        currentOrigin.AllEdges.Add(lineRenderer);
        newNeighborPS.AllEdges.Add(lineRenderer);
        // the allLineRenders is still reserved, as this is the actual "whole list" - those from PS will have overlap
    }

    private void AssignStatsToNewNeighbor(float maxDisplacement, float meanRandomDisturb, float maxCost, PointSystem currentOrigin, float originX, float originY, float originZ, GameObject newNeighbor, PointSystem newNeighborPS)
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

    private void InitializeNewNeighbor(GameObject activeSubOriginGO, PointSystem currentOrigin, out GameObject newNeighbor, out PointSystem newNeighborPS)
    {
        newNeighbor = Instantiate(starPoint, activeSubOriginGO.transform, false);
        //newNeighborPS = newNeighbor.AddComponent<PointSystem>();
        newNeighborPS = newNeighbor.GetComponent<PointSystem>();
        newNeighborPS.pointSystemID = idGenerator.getNextPointSystemID();
        newNeighborPS.SubOriginID = currentOrigin.SubOriginID;
        // better add them after all operations complete, above
        // but above does not have reference to them... 
        // add to origin and suborigin
        origin.GetComponent<Origin>().AllStarSystems.Add(newNeighborPS);
        activeSubOriginGO.GetComponent<SubOrigin>().ClusterSystems.Add(newNeighborPS);
    }

    private void InitializeNewFirstSystem(List<PointSystem> contours, GameObject activeSubOriginGO, SubOrigin newSubOriginSO)
    {
        GameObject firstSystem = Instantiate(starPoint, activeSubOriginGO.transform, false);
        //PointSystem firstSystemPS = firstSystem.AddComponent<PointSystem>();
        PointSystem firstSystemPS = firstSystem.GetComponent<PointSystem>();
        firstSystemPS.pointSystemID = idGenerator.getNextPointSystemID();
        firstSystemPS.SubOriginID = newSubOriginSO.SubOriginID;  // to make it consistent, I know it's the same
                                                                 // remember to apply the xyz in PS to GO!!!
        firstSystemPS.SetCoordinate();  // note that the default is 000 in this method, but the default in PS ini is -1;

        // TODO: move allSystems from here to Origin., well and subOrigin
        //allSystems.Add(firstSystemPS);
        origin.GetComponent<Origin>().AllStarSystems.Add(firstSystemPS);
        newSubOriginSO.ClusterSystems.Add(firstSystemPS);
        contours.Add(firstSystemPS);
        lastOriginPS = firstSystemPS;
    }

    private void InitializeNewSubOrigin(out GameObject activeSubOriginGO, out SubOrigin newSubOriginSO)
    {
        string newSubOriginID = idGenerator.getNextSubOriginID();
        activeSubOriginGO = new GameObject("SubOrigin" + newSubOriginID);
        newSubOriginSO = activeSubOriginGO.AddComponent<SubOrigin>();
        newSubOriginSO.SubOriginID = newSubOriginID;
        activeSubOriginGO.transform.SetParent(origin.transform, false);
        origin.GetComponent<Origin>().SubOrigins.Add(newSubOriginSO);
    }

    //// originally to be used with middleout, now latter is in charge of instantiation, former is changed to
    //// update the connections when user edits the hyperspace channel connectivities
    //// I'll archieve this, and add a new function UpdateConnection()
    //private void instantiateStarSystems()
    //{
        
    //    // generate star systems and connections
    //    List<(string, string)> generatedConnection = new List<(string, string)>();
    //    foreach (PointSystem pointSystem in allSystems)
    //    {
    //        // 加入随机生成不同的星系贴图/模型功能
    //        // 看看unity怎么实现类似群星的发光小球代表星系
    //        // 我tm直接sphere+emission
    //        //GameObject newSystem = Instantiate(starPoint, origin.transform, false) as GameObject;
    //        //newSystem.transform.localPosition = new Vector3(pointSystem.x, pointSystem.y, pointSystem.z);
    //        //pointSystem.systemInstance = newSystem; // assign GameObject reference; 
    //        //starSystems.Add(newSystem);
    //        // generate connections and add to the list:
    //        foreach (string neighborID in pointSystem.neighbors.Keys)
    //        {
    //            string lineRendererName = pointSystem.pointSystemID + " to " + neighborID;
    //            // prepare the line renderer for hyperspace channels: 
    //            LineRenderer lineRenderer = new GameObject(lineRendererName).AddComponent<LineRenderer>();
    //            lineRenderer.startColor = Color.black;
    //            lineRenderer.endColor = Color.black;
    //            lineRenderer.startWidth = 0.01f;
    //            lineRenderer.endWidth = 0.01f;
    //            lineRenderer.positionCount = 2;
    //            lineRenderer.useWorldSpace = true; // doubtful
    //            PointSystem neighborSys = allSystems.Find(sys => sys.pointSystemID == neighborID);  // maybe change allSys to Dict for faster lookup?
    //            Vector3 startingSysLoc = new Vector3(pointSystem.x, pointSystem.y, pointSystem.z);
    //            Vector3 endingSysLoc = new Vector3(neighborSys.x, neighborSys.y, neighborSys.z);
    //            lineRenderer.SetPosition(0, startingSysLoc);
    //            lineRenderer.SetPosition(1, endingSysLoc);
    //        }
    //    }

    //}

    // default is to render the hyperspace channels
    // forget that
    public void UpdateConnection(PointSystem ps)
    {
        //ps.transform.FindChild
        Debug.Log("Updating connection at Point System: " + ps.pointSystemID);
        //foreach (Transform child in ps.gameObject.transform) // 看起来需要遍历全部？如果能作为field存起来似乎可以避免
        //{
        //    if (child.gameObject.name.Contains(" to "))
        //    {
        //        // find the terminal star system
        //        string terminalID = child.gameObject.name[child.gameObject.name.Length - 1].ToString();
        //        Debug.Log("Updating to terminal ID: " + terminalID);
        //        // locate the sub origin, then locate the terminal; should be faster than finding in all sys in origin? 
        //        SubOrigin terminalSO = origin.GetComponent<Origin>().SubOrigins.Find(so => so.subOriginID == ps.SubOriginID);
        //        PointSystem terminalPS = terminalSO.ClusterSystems.Find(ps => ps.pointSystemID == terminalID);
        //        child.GetComponent<LineRenderer>().SetPosition(0, ps.transform.position);
        //        child.GetComponent<LineRenderer>().SetPosition(1, terminalPS.gameObject.transform.position);
        //        // 更新失败，因为line renderer用的是世界坐标？
        //        // 考虑把line renderer放到上一级sub origin里了，不作为当前星系的child存在
        //        // 成功了，让line renderer使用了世界坐标，现在发现另一端也需要更新
        //    }
        //}
        foreach (LineRenderer lr in ps.AllEdges)
        {
            // this will mess up the direction, but now it's undirected anyway, well the child relationship means nothing now
            // but they are now using world pos anyway
            // TODO: move all line renderers to suborigin maybe
            // find the terminals
            // TODO: change all these name based ID finding method! this will cause NTR since 1 and 13 now are the same !!!!! 
            //string terminalID = lr.gameObject.name[0].ToString();
            (string firstID, string secondID) = GetTerminalsByChannelName(lr.gameObject.name);
            // find the sub origin this is all happening: well after we blur the gap between sub origins, this might need refactoring, fuck
            SubOrigin terminalSO = origin.GetComponent<Origin>().SubOrigins.Find(so => so.subOriginID == ps.SubOriginID);
            if (firstID == ps.pointSystemID)
            {
                // then it's the right order
                //lr.SetPosition(0, ps.gameObject.transform.localPosition);
                // this is from world to local: line renderer is using local position, thus converting from world to local! 
                lr.SetPosition(0, lr.transform.InverseTransformPoint(ps.gameObject.transform.position));
                //firstID = lr.gameObject.name[lr.gameObject.name.Length - 1].ToString(); // fuck this... why did it work??? 
                // find the terminal PS
                //PointSystem terminalPS = terminalSO.ClusterSystems.Find(ps => ps.pointSystemID == firstID);
                PointSystem terminalPS = origin.GetComponent<Origin>().AllStarSystems.Find(ps => ps.pointSystemID == secondID); // changed from firstID above to now
                if (terminalPS == null)
                {
                    Debug.Log("Failed to find terminal PS with ID " + secondID + "; pairing with " + firstID);
                }
                //lr.SetPosition(1, terminalPS.gameObject.transform.localPosition);
                lr.SetPosition(1, lr.transform.InverseTransformPoint(terminalPS.gameObject.transform.position));
            }
            else
            {
                // then it's the in-going edge
                // do we actually need the 0 pos update? anyway
                PointSystem terminalPS = terminalSO.ClusterSystems.Find(ps => ps.pointSystemID == firstID);
                if (terminalPS == null)
                {
                    Debug.Log("Terminal PS Not Found: " + firstID);
                }
                //lr.SetPosition(0, terminalPS.gameObject.transform.localPosition);
                lr.SetPosition(0, lr.transform.InverseTransformPoint(terminalPS.gameObject.transform.position));
                //lr.SetPosition(1, ps.gameObject.transform.localPosition);
                lr.SetPosition(1, lr.transform.InverseTransformPoint(ps.gameObject.transform.position));
            }
            // well this maybe does not mess up the direction
        }
    }

    private (string,string) GetTerminalsByChannelName(string channelName)
    {
        var splits = channelName.Split(' ');
        Debug.Log("Retrieving from " + channelName);
        Debug.Log("Got " + splits[0] + " and " + splits[splits.Length - 1]);
        return (splits[0], splits[splits.Length - 1]);
    }

    IEnumerator ShowSaveDialogCoroutine()
    {
        yield return FileBrowser.WaitForSaveDialog(FileBrowser.PickMode.Files);
        if (FileBrowser.Success)
        {
            savePath = FileBrowser.Result[0];
            //Debug.Log(FileBrowser.Result);
            SaveWorldConfigToCsvAfterSaveDialog();
        }
    }

    // TODO: read world setting from such a csv file
    private void SaveWorldConfigToCsv()
    {
        StartCoroutine(ShowSaveDialogCoroutine());
        // only save the list allStarSystems for now
        //string tempPath = "Assets/Resources/tempSave.csv";
        //string tempPath = savePath;
        //StreamWriter writer = new StreamWriter(tempPath, false, System.Text.Encoding.UTF8); // to the file path, overwrites, utf-8 encoding
        //Debug.Log("writer initialized");
        //// insert some identification data first? like name of this world blablabla
        //foreach (PointSystem ps in origin.GetComponent<Origin>().AllStarSystems)
        //{
        //    Debug.Log("writing new ps");
        //    writer.WriteLine(ps.ToCsvString());
        //}
        //Debug.Log("writing done");
        //writer.Close();
        //Debug.Log("writer closed");
    }

    private void SaveWorldConfigToCsvAfterSaveDialog()
    {
        string tempPath = savePath;
        StreamWriter writer = new StreamWriter(tempPath, false, System.Text.Encoding.UTF8); // to the file path, overwrites, utf-8 encoding
        Debug.Log("writer initialized");
        // insert some identification data first? like name of this world blablabla
        foreach (PointSystem ps in origin.GetComponent<Origin>().AllStarSystems)
        {
            Debug.Log("writing new ps");
            writer.WriteLine(ps.ToCsvString());
        }
        Debug.Log("writing done");
        writer.Close();
        Debug.Log("writer closed");
    }

    IEnumerator ShowLoadDialogCoroutine()
    {
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Files);
        if (FileBrowser.Success)
        {
            loadPath = FileBrowser.Result[0];
            //Debug.Log(FileBrowser.Result);
            LoadWorldConfigFromCsvAfterLoadDialog();
        }
    }

    private void LoadWorldConfigFromCsv()
    {
        StartCoroutine(ShowLoadDialogCoroutine());

    }

    private void LoadWorldConfigFromCsvAfterLoadDialog()
    {

        DestroyCurrentWorldSetting();


        // using fixed tempSave to test on functionalities now
        // TODO: add a backup functionality, since now the whole save will be overwritten
        //string tempPath = "Assets/Resources/tempSave.csv";
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
            //Dictionary<string, float> neighbors = neighborsLine.ToDictionary<string, float>(x => neighborsLine.Dequeue(), y => float.Parse(neighborsLine.Dequeue()));
            Dictionary<string, float> neighbors = new Dictionary<string, float>();
            while (neighborsLine.Count >= 2)
            {
                neighbors.Add(neighborsLine.Dequeue(), float.Parse(neighborsLine.Dequeue()));
                //neighbors.Add(neighborsLine[0], float.Parse(neighborsLine[1]));
                //neighborsLine.RemoveRange(0, 2);
            }

            GameObject subOriginGO;
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
            GameObject psGO = Instantiate(starPoint, subOriginGO.transform, false);
            //PointSystem ps = psGO.AddComponent<PointSystem>(); // now the prefab has the script on it itself; 
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
            //// its neighboring connections will be forloop-updated after all ps generated
            //// here we need to pre-generate the line renderers and add to ps.AllEdges to be updated later
            //// TODO: The connection now will be generated twice: 0-1 and 1-0, thus the update function won't function normally; it actually will, but it's fixed anyway
            //// TODO: the place where a connection is recorded under which ps might change after save&load
            //foreach (KeyValuePair<string, float> neighbor in neighbors)
            //{
            //    // wait, seems like the line renderer function can be reused? 
            //    // nvm the neighbors ain't ini'd yet

            //    // trying to fix the line render update bug
            //    // load后会有连接线在原地是因为同一个链接被生成了两次
            //    // 这时如果说"已经有了就不再生成", 就会导致部分链接不被update, 因为update的时候遵循了父子关系
            //    // 并不, 见下: update fail是因为neighbor.AllEdges会缺失定义. 
            //    // 考虑先生成一遍所有星系而不管connection, 然后再来一遍, 就可以直接call InstantiateHyperspaceConnection了
            //    string check_for_generated = neighbor.Key + " to " + ps.pointSystemID;
            //    if (GameObject.Find(check_for_generated) != null)
            //    {
            //        continue;
            //    }
            //    string lineRendererName = ps.pointSystemID + " to " + neighbor.Key;
            //    LineRenderer lineRenderer = new GameObject(lineRendererName).AddComponent<LineRenderer>();
            //    lineRenderer.transform.SetParent(ps.gameObject.transform, true);
            //    lineRenderer.startWidth = 0.01f;
            //    lineRenderer.endWidth = 0.01f;
            //    lineRenderer.positionCount = 2;
            //    lineRenderer.useWorldSpace = false;
            //    // SetPosition not called here
            //    allLineRenderers.Add(lineRenderer);
            //    ps.AllEdges.Add(lineRenderer);
            //    // neighborPS.AllEdges cannot be added here... should not be a problem?
            //    // it is a problem, it will cause the update to fail
            //}
            origin.GetComponent<Origin>().AllStarSystems.Add(ps);
            subOriginGO.GetComponent<SubOrigin>().ClusterSystems.Add(ps);
            //Debug.Log(line);
        }
        // 我 tm n^3
        foreach (PointSystem ps in origin.GetComponent<Origin>().AllStarSystems)
        {
            foreach (KeyValuePair<string, float> neighbor in ps.neighbors)
            {
                PointSystem neighborPS = origin.GetComponent<Origin>().AllStarSystems.Find(p => p.pointSystemID == neighbor.Key);
                // 还是会ini两遍同一个connection, 可以在方法里加入一个不重复的判定, 但同样会导致不稳定: 从谁到谁这个记录在存档的时候可能会改变
                InstantiateHyperspaceConnection(ps, neighborPS);
                // 这样就不用Update Connection了? 
            }
        }

        //// 是否可以加入到这里 算了估计会出bug
        //foreach (PointSystem ps in origin.GetComponent<Origin>().AllStarSystems)
        //{
        //    UpdateConnection(ps); // now all edges will be updated, the terminalPS should be able to be assigned the found ps now
        //}
        PopulateOverviewScroll();
    }

    // disable all in-memory stats currently in Origin and others
    private void DestroyCurrentWorldSetting()
    {
        Debug.Log("Clearing Stats stored now in origins");
        // TODO: maybe suborigins need to be saved as well? their positions to origin
        // their positions are not recorded now anyways, it's strange
        // TODO: idGenerator... alas
        origin.GetComponent<Origin>().SubOrigins.Clear();
        origin.GetComponent<Origin>().AllStarSystems.Clear();
        foreach (Transform childTransform in origin.transform)
        {
            Destroy(childTransform.gameObject);
        }
    }

    public void ChangeStarSysName()
    {

    }

    // User edit of selected point system's info

    // stop camera movement
    public void EditStart()
    {

    }

    // since no InputField.OnEditStart is available: 
    public void EditDuring()
    {
        if (fakeMainCamScript.moving)
        {
            fakeMainCamScript.moving = false;
        }
    }

    // update star system config
    public void EditEnd()
    {
        fakeMainCamScript.moving = true;
        var curPS = origin.GetComponent<Origin>().AllStarSystems.Find(ps => ps.pointSystemID == lastClickedStarPointID);
        EditPSSaveConfig(curPS);
    }

    private void EditPSSaveConfig(PointSystem curPS)
    {
        curPS.StarSystemName = starSystemNameDynamic.text;
        //curPS.systemNameTextMesh.text = curPS.StarSystemName;
        //curPS.StarType = starTypeDynamic.text; // f it's gotta be a drop down
        curPS.PlanetCount = int.Parse(planetCountDynamic.text);
        curPS.Holder = heldByDynamic.text;
    }
}
