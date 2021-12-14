using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControls : MonoBehaviour
{
    [SerializeField] private float speed;

    // Update is called once per frame
    void Update()
    {
        if(Input.GetAxis("Vertical") != 0)
        {
            transform.Translate(new Vector3(0, (Input.GetAxis("Vertical") * speed * Time.deltaTime), 0), Space.World);
        }
        
        if(Input.GetAxis("Horizontal") != 0)
        {
            transform.Translate(new Vector3((Input.GetAxis("Horizontal") * speed * Time.deltaTime), 0, 0), Space.World);
        }
    }
}
