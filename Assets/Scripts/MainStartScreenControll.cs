using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainStartScreenControll : MonoBehaviour
{

    GameObject NewWorldButton;
    GameObject LoadWorldButton;

    // Start is called before the first frame update
    void Start()
    {
        // instantiate game objects: background, menu buttons... 

        // what's the difference of assigning listener here and in the unity gui on click? latter requires method to be public?
        // latter seems no difference, it's adding another listener as well, dunno why it isn't showing up in runtime properties. 
        NewWorldButton = GameObject.Find("NewWorldButton");
        NewWorldButton.GetComponent<Button>().onClick.AddListener(NewWorld);
        LoadWorldButton = GameObject.Find("LoadWorldButton");
        LoadWorldButton.GetComponent<Button>().onClick.AddListener(LoadWorld);
    }

    // Update is called once per frame
    void Update()
    {
        //if (NewWorldButton.GetComponent<Button>().onClick)
    }

    public void NewWorld()
    {
        Debug.Log("New World Button Clicked");
        // enter world setting scene or make a new blank world setting file? 
        // this communication method is ugly: 
        GameObject.Find("SceneManagerControl").GetComponent<SceneManagerControl>().EnterWorldSettingScene(SceneManagerControl.LoadMode.NewWorld);
    }

    void LoadWorld()
    {
        Debug.Log("Load World Button Clicked");
        // load a saved WorldSettings file and enter world setting scene
        // should open a file choosing window and then pass the param to scene manager. 
        GameObject.Find("SceneManagerControl").GetComponent<SceneManagerControl>().WorldSettingsFilePath = "Temp File Path: World Loading WIP";
        GameObject.Find("SceneManagerControl").GetComponent<SceneManagerControl>().EnterWorldSettingScene(SceneManagerControl.LoadMode.LoadSavedWorld);
    }

}
