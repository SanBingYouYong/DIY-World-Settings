using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerControl : MonoBehaviour
{

    public enum LoadMode
    {
        NewWorld, 
        LoadSavedWorld
    }

    //delegate void enterWorldSettingScene(LoadMode mode);
    //enterWorldSettingScene enterWsDelegate;

    string worldSettingsFilePath;
    LoadMode mode;

    Camera mainCamSM; // Scene Manager Cam: should not be enabled anyway?
    Camera mainCamMSS; // Main Start Screen Cam: disable on entering of WSS
    Camera mainCamWSS; // World Setting Scene Cam: enable only when at WSS;

    public LoadMode Mode { get => mode; set => mode = value; }
    public string WorldSettingsFilePath { get => worldSettingsFilePath; set => worldSettingsFilePath = value; }

    // Start is called before the first frame update
    void Start()
    {
        mainCamSM = GameObject.Find("Main Camera SM").GetComponent<Camera>();
        mainCamSM.enabled = false;
        SceneManager.LoadScene("MainStartScreen", LoadSceneMode.Additive);
        // to avoid null exception on cam when the scene isn't loaded fully
        if (SceneManager.GetSceneByName("MainStartScreen").isLoaded)
        {
            mainCamMSS = GameObject.Find("Main Camera MSS").GetComponent<Camera>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void EnterWorldSettingScene(LoadMode mode)
    {
        this.Mode = mode;
        SceneManager.UnloadSceneAsync("MainStartScreen"); // been asked to use UnloadSceneAsync instead of UnloadScene (latter not safe)
        SceneManager.LoadScene("WorldSettingScene", LoadSceneMode.Additive);
        if (SceneManager.GetSceneByName("WorldSettingScene").isLoaded)
        {
            mainCamWSS = GameObject.Find("Main Camera WSS").GetComponent<Camera>();
            // TODO: find out how to change light setting skyboxes between scenes
            // Window->Rendering->Lighting->Environment->SkyboxTexture: Skybox
            //mainCamWSS.enabled = true;
            //RenderSettings.skybox = (Material)Resources.Load("Skybox");
            
        }


        //LoadWorld();
    }

    //public string GetWorldSettingsFilePath()
    //{
    //    return WorldSettingsFilePath;
    //}

    //public void SetWorldSettingsFilePath(string newPath)
    //{
    //    WorldSettingsFilePath = newPath;
    //    Debug.Log("new WorldSettings file path set");
    //}
}
