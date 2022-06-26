using cakeslice;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Arrow : MonoBehaviour
{

    Vector3 movingVec;
    float speed;
    Vector3 targetPos;

    UnityEngine.UI.Toggle moveWholeCluster;  // if yes, then manipulate the empty object "origin" instead of single star system;

    GameObject origin;

    SubOrigin subOrigin;

    WorldSettingSceneControl sceneControl;

    // Start is called before the first frame update
    void Start()
    {
        //speed = 0.1f;
        // this is ugly I know
        //if (gameObject.name.Contains("_X"))
        //{
        //    movingVec = new Vector3(1f, 0f, 0f);
        //}
        //else if (gameObject.name.Contains("_Y"))
        //{
        //    movingVec = new Vector3(0f, 1f, 0f);
        //}
        //else
        //{
        //    movingVec = new Vector3(0f, 0f, 1f);
        //}
        //movingVec = Vector3.forward;
        moveWholeCluster = GameObject.Find("MoveWholeClusterToggle").GetComponent<UnityEngine.UI.Toggle>();
        origin = GameObject.Find("origin");
        sceneControl = GameObject.Find("WorldSettingSceneControl").GetComponent<WorldSettingSceneControl>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnMouseEnter()
    {
        //Debug.Log("Mouse Entered the Arrow");
        GetComponent<cakeslice.Outline>().enabled = true;
    }

    void OnMouseExit()
    {
        GetComponent<cakeslice.Outline>().enabled = false;
    }

    void OnMouseDrag()
    {
        float distance_to_screen = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;
        ////movingVec = movingVec * distance_to_screen;
        //transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance_to_screen));
        Vector3 newPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance_to_screen));
        Vector3 starPos = transform.parent.transform.position;
        subOrigin = null;

        if (moveWholeCluster.isOn)
        {
            //starPos = origin.transform.position;  // old origin version, now changed to sub origin
            string subOriginID = transform.parent.GetComponent<PointSystem>().SubOriginID;
            subOrigin = origin.GetComponent<Origin>().SubOrigins.Find(so => so.subOriginID == subOriginID);
            starPos = subOrigin.gameObject.transform.position;
        }

        // interesting to think about reference vs. copying here with vectors
        // transform.position is an accessor - a "property" in C# - thus the field can't be passed but only its value can be changed. 
        // hmmm

        //newPos.y = transform.position.y;
        //newPos.z = transform.position.z;
        //transform.position = newPos;
        if (gameObject.name.Contains("_X"))
        {
            starPos.x = newPos.x;
            //transform.parent.transform.position = transform.parent.transform.TransformPoint(starPos);
            
        }
        else if (gameObject.name.Contains("_Y"))
        {
            starPos.y = newPos.y;
            //transform.parent.transform.position = starPos;
        }
        else if (gameObject.name.Contains("_Z"))  // needed? 
        {
            starPos.z = newPos.z;
            //transform.parent.transform.position = starPos;
        }
        if (moveWholeCluster.isOn)
        {
            // TODO: need to know which subcluster it is within
            //origin.transform.position = starPos;
            subOrigin.gameObject.transform.position = starPos;
        }
        else
        {
            transform.parent.transform.position = starPos; // actually no need to convert localPos of arrow to pos of star manually ->
                                                           // above Camera.main.blablabla already does the work? kinda
        }

        //transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, 0f, distance_to_screen));
        ////transform.position = Camera.main.ScreenToWorldPoint(movingVec);
        ////transform.Translate(movingVec);
        //transform.parent.transform.Translate(movingVec * Time.deltaTime);
    }

    void OnMouseUp()
    {
        // refresh the line renderers maybe
        // TODO: line renderers... 
        // update the PointSystem xyz value
        transform.parent.GetComponent<PointSystem>().SetCoordinate(transform.position);
        // TODO: if moving whole, update on all PSs!!! 
        Debug.Log("Updated on Point System coordinates");
        if (moveWholeCluster.isOn)
        {
            foreach (PointSystem ps in subOrigin.ClusterSystems)
            {
                // it's an update of the script according to its gameobject
                ps.SetCoordinate(ps.gameObject.transform.position);  // world pos, and it's used in line renderer
            }
        }
        else
        {
            // call the update line renderer function in control
            sceneControl.UpdateConnection(transform.parent.GetComponent<PointSystem>());
        }
    }
}
