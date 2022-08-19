using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{

    private Vector3 camera_pos;
    private Vector3 camera_rot;
    public bool moving;
    public float speed = 10f;

    // Start is called before the first frame update
    void Start()
    {
        camera_pos = transform.position;
        camera_rot = transform.eulerAngles;
        moving = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (moving)
        {
            if (Input.GetKey(KeyCode.W))
            {
                //camera_pos.y += 50 * Time.deltaTime;
                //Camera.transform.position = camera_pos;
                //transform.Translate(new Vector3(0, 0, 10 * Time.deltaTime));
                transform.position += transform.forward * speed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.S))
            {
                //transform.Translate(new Vector3(0, 0, -10 * Time.deltaTime));
                transform.position += (-transform.forward) * speed * Time.deltaTime;
                // ahhh the move method is not the one causing the angle problem, just made the xzy damping all to 0 from 1 in vcam1
            }
            if (Input.GetKey(KeyCode.A))
            {
                transform.Translate(new Vector3(-10 * Time.deltaTime, 0, 0));
            }
            if (Input.GetKey(KeyCode.D))
            {
                transform.Translate(new Vector3(10 * Time.deltaTime, 0, 0));
            }
            if (Input.GetKey(KeyCode.Q))
            {
                transform.Translate(new Vector3(0, -10 * Time.deltaTime, 0));
            }
            if (Input.GetKey(KeyCode.E))
            {
                transform.Translate(new Vector3(0, 10 * Time.deltaTime, 0));
            }
            if (Input.GetMouseButton(2))
            {
                float rh = Input.GetAxis("Mouse X");
                float rv = Input.GetAxis("Mouse Y");

                camera_rot.x -= rv * 5;
                camera_rot.y += rh * 5;

                transform.eulerAngles = camera_rot;
            }
        }
    }
}
