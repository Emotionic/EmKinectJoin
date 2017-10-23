using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetLaunchPadPosition : MonoBehaviour
{

    // Use this for initialization
    void Update()
    {
        this.transform.position = new Vector3(Camera.main.transform.position.x, -Camera.main.orthographicSize, -3);
    }

}
