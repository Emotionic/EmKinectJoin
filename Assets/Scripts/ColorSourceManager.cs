using UnityEngine;
using System.Collections;
using Windows.Kinect;

public class ColorSourceManager : MonoBehaviour 
{
    public int ColorWidth { get; private set; }
    public int ColorHeight { get; private set; }
    public Texture2D Texture { get; private set; }

    private byte[] Data;

    private KinectSensor _Sensor;
    private ColorFrameReader _Reader;
    
    void Start()
    {
        _Sensor = KinectSensor.GetDefault();
        
        if (_Sensor != null) 
        {
            _Reader = _Sensor.ColorFrameSource.OpenReader();
            
            var frameDesc = _Sensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Rgba);
            ColorWidth = frameDesc.Width;
            ColorHeight = frameDesc.Height;
            
            Texture = new Texture2D(frameDesc.Width, frameDesc.Height, TextureFormat.RGBA32, false);
            Data = new byte[frameDesc.BytesPerPixel * frameDesc.LengthInPixels];
            
            if (!_Sensor.IsOpen)
            {
                _Sensor.Open();
            }
        }
    }
    
    void Update () 
    {
        if (_Reader != null) 
        {
            var frame = _Reader.AcquireLatestFrame();
            
            if (frame != null)
            {
                frame.CopyConvertedFrameDataToArray(Data, ColorImageFormat.Rgba);
                Texture.LoadRawTextureData(Data);
                Texture.Apply();
                
                frame.Dispose();
                frame = null;
            }
        }
    }

    void OnApplicationQuit()
    {
        if (_Reader != null) 
        {
            _Reader.Dispose();
            _Reader = null;
        }
        
        if (_Sensor != null) 
        {
            if (_Sensor.IsOpen)
            {
                _Sensor.Close();
            }
            
            _Sensor = null;
        }
    }
}
