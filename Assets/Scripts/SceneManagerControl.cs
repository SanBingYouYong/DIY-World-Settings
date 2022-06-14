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

    public LoadMode Mode { get => mode; set => mode = value; }
    public string WorldSettingsFilePath { get => worldSettingsFilePath; set => worldSettingsFilePath = value; }

    // Start is called before the first frame update
    void Start()
    {
        SceneManager.LoadScene("MainStartScreen", LoadSceneMode.Additive);
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
