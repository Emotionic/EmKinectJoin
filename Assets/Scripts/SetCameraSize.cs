using UnityEngine;
using System.Collections;

public class SetCameraSize : MonoBehaviour
{
    public Camera _ConvertCamera;
    public Camera _MainCamera;

    // Update is called once per frame
    void Update()
    {
        float frustumHeight = Screen.width / 100f / _MainCamera.aspect;

        float distance = frustumHeight * 0.5f;

        _ConvertCamera.orthographicSize = distance;
    }
}