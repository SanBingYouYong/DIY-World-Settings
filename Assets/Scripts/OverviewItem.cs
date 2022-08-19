using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OverviewItem : MonoBehaviour
{

    public Text nameText;
    public Button focusButton;

    WorldSettingSceneControl control;
    public PointSystem ps;

    // Start is called before the first frame update
    void Start()
    {
        control = GameObject.Find("WorldSettingSceneControl").GetComponent<WorldSettingSceneControl>();
        //ps = control.Sys
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateStats(PointSystem ps)
    {
        nameText.GetComponent<Text>().text = ps.StarSystemName;
    }

    public void FocusPressed()
    {
        Debug.Log(nameText.text + "'s focus button is pressed");
        control.FocusToGO(ps.gameObject.transform);
    }
}
