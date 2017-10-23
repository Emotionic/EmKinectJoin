using UnityEngine;
using System.Collections;
using Windows.Kinect;

public class ColorSourceView : MonoBehaviour
{
    public GameObject ColorSourceManager;
    public bool IsMirror;
    private ColorSourceManager _ColorManager;
    
    void Start ()
    {
        gameObject.GetComponent<Renderer>().material.SetTextureScale("_MainTex", 
            new Vector2((IsMirror) ? 1 : -1, 1));

        // this.transform.localScale = new Vector3(Screen.width / 100f, Screen.height / 100f, 1);
    }
    
    void Update()
    {
        if (ColorSourceManager == null)
        {
            return;
        }
        
        _ColorManager = ColorSourceManager.GetComponent<ColorSourceManager>();
        if (_ColorManager == null)
        {
            return;
        }
        
        gameObject.GetComponent<Renderer>().material.mainTexture = _ColorManager.Texture;
    }
}
