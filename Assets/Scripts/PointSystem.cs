using cakeslice;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// a nested class, most likely won't be used anywhere else for now, but can always be extracted; 
// well with mouse over and mouse down events I think it's time for extraction; 
// ray casting might work? what about performance
// okay, don't worry about performance now - until it becomes a problem - then solve it
public class PointSystem : MonoBehaviour
{
    public string pointSystemID;

    string subOriginID;

    // note that most of the times xyz should be world coordinate (?) wait no, is it local? 
    public float x;
    public float y;
    public float z;

    public Dictionary<string, float> neighbors;
    // in fact we can turn to simply metric system, it's a design decision of whether we want the map to be accurate at all
    // decision: no, distance is still customizable, in order to simulate the hyperspace jump cost; 

    public GameObject systemInstance;  // not needed anymore after converting to monobehaviour script
    
    public TextMesh systemNameTextMesh;

    public Camera mainCam;

    //private Color startcolor;

    GameObject arrowXRef;
    GameObject arrowYRef;
    GameObject arrowZRef;

    GameObject arrowXInstance;
    GameObject arrowYInstance;
    GameObject arrowZInstance;

    List<LineRenderer> allEdges;

    //private bool clicked;

    WorldSettingSceneControl sceneControl;  // a reference to the scene control in current world setting scene

    ///////////////////////////////////////////////////
    ///Property-Related Fields
    ///////////////////////////////////////////////////
    private string starSystemName;

    private StarSpectalType starType;

    private int planetCount;

    private string holder;

    // 这里可能后期改成独自自定义的形式, 往存档文件里加个表头那种, 现在就, 摆烂吧, 不存自定义了
    public enum StarSpectalType
    {
        O,
        B,
        A,
        F,
        G,
        K,
        M
    }

    public string SubOriginID { get => subOriginID; set => subOriginID = value; }
    public List<LineRenderer> AllEdges { get => allEdges; set => allEdges = value; }
    public string StarSystemName { get => starSystemName; set => starSystemName = value; }
    public StarSpectalType StarType { get => starType; set => starType = value; }
    public int PlanetCount { get => planetCount; set => planetCount = value; }
    public string Holder { get => holder; set => holder = value; }

    void Awake()
    {
        // from constructor
        pointSystemID = "_";
        x = -1f;
        y = -1f;
        z = -1f;
        neighbors = new Dictionary<string, float>();
        systemInstance = null;
        AllEdges = new List<LineRenderer>();

        sceneControl = GameObject.Find("WorldSettingSceneControl").GetComponent<WorldSettingSceneControl>();
        arrowXRef = sceneControl.ArrowX;
        arrowYRef = sceneControl.ArrowY;
        arrowZRef = sceneControl.ArrowZ;

        mainCam = GameObject.Find("Main Camera WSS").GetComponent<Camera>();

        //clicked = false;
        // to initialize property-fields
        StarConfigInitialize();

        systemNameTextMesh.text = starSystemName;
    }

    private void Update()
    {
        // rotate the star name text to look at the cam; 
        systemNameTextMesh.transform.LookAt(mainCam.transform);
        // should be the angle between origin and cam? I don't want this in-parallel feel. 

        if (starSystemName != systemNameTextMesh.text)
        {
            systemNameTextMesh.text = starSystemName;
        }
    }

    void OnMouseEnter()
    {
        //startcolor = GetComponent<Renderer>().material.color;
        //GetComponent<Renderer>().material.color = Color.yellow;
        GetComponent<Outline>().enabled = true;
    }
    void OnMouseExit()
    {
        //GetComponent<Renderer>().material.color = startcolor;
        GetComponent<Outline>().enabled = false;
    }

    void OnMouseOver()
    {
        // if clicked: show three arrows to move
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("You've clicked the star system with id: " + pointSystemID);
            sceneControl.LastClickedStarPointID = pointSystemID; // that it will leave this id alone
            sceneControl.NewStarPointClicked = true; // starts the cleaning process, needs testing on async
            // need a method to tell it not to re-generate same arrows over and over again: 
            // Destroy them now and generate again..? 
            // another good reason: if not destroyed, the variable will lose track of older arrows! 
            // try destroy
            if (arrowXInstance != null || arrowYInstance != null || arrowZInstance != null)
            {
                DestroyArrows();
            }
            // generate
            arrowXInstance = Instantiate(arrowXRef, gameObject.transform, false);
            arrowYInstance = Instantiate(arrowYRef, gameObject.transform, false);
            arrowZInstance = Instantiate(arrowZRef, gameObject.transform, false);
            // 需要一个广播器，告知其他point system：destroy掉他们的arrows？
            // added flags to WorldSettingsSceneControl.Update()
        }
    }

    // to update the localPos of the GO, however, this does not effect the real GO position!!! 
    public void SetCoordinate(float x = 0f, float y = 0f, float z = 0f)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        //return true;
    }

    public void SetCoordinate(Vector3 pos)
    {
        x = pos.x;
        y = pos.y;
        z = pos.z;
    }

    public void DestroyArrows()
    {
        Destroy(arrowXInstance);
        Destroy(arrowYInstance);
        Destroy(arrowZInstance);
    }

    private void StarConfigInitialize()
    {
        starSystemName = RandomStarSystemName();
        starType = RandomStarType();
        planetCount = RandomPlanetCount();
        holder = RandomHolder();
    }

    private string RandomStarSystemName()
    {
        return "a meaningful star system name";
    }

    private StarSpectalType RandomStarType()
    {
        System.Array types = System.Enum.GetValues(typeof(StarSpectalType));
        int index = Random.Range(0, types.Length - 1);
        return (StarSpectalType)types.GetValue(index);
    }

    private int RandomPlanetCount()
    {
        return Random.Range(1, 15);
    }

    private string RandomHolder()
    {
        return "an empire";
    }

    public override string ToString()
    {
        string info = "Printing Info of Point System: \n";
        info += "Id: " + pointSystemID + " \n";
        info += "Location: " + x + ", " + y + ", " + z + " \n";
        info += "Neighbors: " + string.Join(", ", neighbors) + " \n";
        return info;
    }

    public string ToCsvString()
    {
        string info = "";
        info += pointSystemID + "," + subOriginID + ","; // ID
        info += x + "," + y + "," + z + ","; // position
        // Property-Related Fields
        info += starSystemName + "," + starType + "," + planetCount + "," + holder;
        // neighbors
        foreach (KeyValuePair<string, float> neighbor in neighbors)
        {
            info += "," + neighbor.Key + "," + neighbor.Value;
        }
        // "\n" omitted since writer.WriteLine() automatically starts a new line
        return info;
    }

}
