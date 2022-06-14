using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldSettingSceneCamera : MonoBehaviour
{

    private GameObject Camera;
    private Vector3 camera_pos;
    private Vector3 camera_rot;

    // Start is called before the first frame update
    void Start()
    {
        Camera = GameObject.Find("Main Camera");
        camera_pos = Camera.transform.position;
        camera_rot = Camera.transform.eulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            //camera_pos.y += 50 * Time.deltaTime;
            //Camera.transform.position = camera_pos;
            Camera.transform.Translate(new Vector3(0, 0, 10 * Time.deltaTime));
        }
        if (Input.GetKey(KeyCode.S))
        {
            Camera.transform.Translate(new Vector3(0, 0, -10 * Time.deltaTime));
        }
        if (Input.GetKey(KeyCode.A))
        {
            Camera.transform.Translate(new Vector3(-10 * Time.deltaTime, 0, 0));
        }
        if (Input.GetKey(KeyCode.D))
        {
            Camera.transform.Translate(new Vector3(10 * Time.deltaTime, 0, 0));
        }
        if (Input.GetKey(KeyCode.Q))
        {
            Camera.transform.Translate(new Vector3(0, -10 * Time.deltaTime, 0));
        }
        if (Input.GetKey(KeyCode.E))
        {
            Camera.transform.Translate(new Vector3(0, 10 * Time.deltaTime, 0));
        }
        if (Input.GetMouseButton(2))
        {
            float rh = Input.GetAxis("Mouse X");
            float rv = Input.GetAxis("Mouse Y");

            camera_rot.x -= rv * 5;
            camera_rot.y += rh * 5;

            Camera.transform.eulerAngles = camera_rot;
        }
        // if no input: slowly slow down to 0? : 
        // want to add slow random regular turbulence too
    }
}
