using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Testing : MonoBehaviour
{

    public CinemachineVirtualCamera vcam;
    public GameObject x;
    public GameObject y;
    public GameObject z;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            vcam.Follow = x.transform;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            vcam.Follow = y.transform;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            vcam.Follow = z.transform;
        }
    }
}
