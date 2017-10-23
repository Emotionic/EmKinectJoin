using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;

public class ColorBodySourceView : MonoBehaviour 
{
    /// <summary>
    /// 認識した関節を表示するか
    /// </summary>
    public bool IsBodyView;
    
    /// <summary>
    /// 関節表示用マテリアル
    /// </summary>
    public Material BoneMaterial;

    /// <summary>
    /// BodySourceManagerクラス取得用オブジェクト
    /// </summary>
    public GameObject BodySourceManager;

    /// <summary>
    /// 視点変換用カメラ
    /// </summary>
    public Camera ConvertCamera;

    public bool IsMirror;

    /// <summary>
    /// Kinectで取得したBodyデータを渡す
    /// </summary>
    /// <returns>取得したBodyデータ</returns>
    public GameObject[] GetBodies()
    {
        // DictionaryからBodyデータのみを渡す
        GameObject[] bodies = new GameObject[_Bodies.Count];
        
        _Bodies.Values.CopyTo(bodies, 0);

        return bodies;
    }

    /// <summary>
    /// IDを指定して一つのBodyデータを渡す
    /// </summary>
    /// <param name="id">BodyのID</param>
    /// <returns>idに紐付けられたBodyデータ</returns>
    public GameObject GetBody(ulong id)
    {
        if (_Bodies.ContainsKey(id))
            return _Bodies[id];
        else
            return null;
    }
    
    /// <summary>
    /// idとBodyオブジェクトの辞書
    /// </summary>
    private Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>();

    /// <summary>
    /// idとJointTypeからJointのオブジェクトを得る辞書
    /// </summary>
    public Dictionary<ulong, Dictionary<Kinect.JointType, GameObject>> JointsFromBodies { get; private set; }

    /// <summary>
    /// インスタンス化されたBodyManager
    /// </summary>
    private BodySourceManager _BodyManager;

    /// <summary>
    /// 各座標系を対応付けるためのクラス
    /// </summary>
    private Kinect.CoordinateMapper _CoordinateMapper;

    /// <summary>
    /// Kinectから取得できる画像の幅
    /// </summary>
    private const int _KinectWidth = 1920;

    /// <summary>
    /// Kinectから取得できる画像の高さ
    /// </summary>
    private const int _KinectHeight = 1080;
    
    /// <summary>
    /// 各関節のHead向きで隣にある関節を取得するための辞書
    /// </summary>
    private Dictionary<Kinect.JointType, Kinect.JointType> _BoneMap = new Dictionary<Kinect.JointType, Kinect.JointType>()
    {
        { Kinect.JointType.FootLeft, Kinect.JointType.AnkleLeft },
        { Kinect.JointType.AnkleLeft, Kinect.JointType.KneeLeft },
        { Kinect.JointType.KneeLeft, Kinect.JointType.HipLeft },
        { Kinect.JointType.HipLeft, Kinect.JointType.SpineBase },
        
        { Kinect.JointType.FootRight, Kinect.JointType.AnkleRight },
        { Kinect.JointType.AnkleRight, Kinect.JointType.KneeRight },
        { Kinect.JointType.KneeRight, Kinect.JointType.HipRight },
        { Kinect.JointType.HipRight, Kinect.JointType.SpineBase },
        
        { Kinect.JointType.HandTipLeft, Kinect.JointType.HandLeft },
        { Kinect.JointType.ThumbLeft, Kinect.JointType.HandLeft },
        { Kinect.JointType.HandLeft, Kinect.JointType.WristLeft },
        { Kinect.JointType.WristLeft, Kinect.JointType.ElbowLeft },
        { Kinect.JointType.ElbowLeft, Kinect.JointType.ShoulderLeft },
        { Kinect.JointType.ShoulderLeft, Kinect.JointType.SpineShoulder },
        
        { Kinect.JointType.HandTipRight, Kinect.JointType.HandRight },
        { Kinect.JointType.ThumbRight, Kinect.JointType.HandRight },
        { Kinect.JointType.HandRight, Kinect.JointType.WristRight },
        { Kinect.JointType.WristRight, Kinect.JointType.ElbowRight },
        { Kinect.JointType.ElbowRight, Kinect.JointType.ShoulderRight },
        { Kinect.JointType.ShoulderRight, Kinect.JointType.SpineShoulder },
        
        { Kinect.JointType.SpineBase, Kinect.JointType.SpineMid },
        { Kinect.JointType.SpineMid, Kinect.JointType.SpineShoulder },
        { Kinect.JointType.SpineShoulder, Kinect.JointType.Neck },
        { Kinect.JointType.Neck, Kinect.JointType.Head },
    };

    private void Start()
    {
        JointsFromBodies = new Dictionary<ulong, Dictionary<Kinect.JointType, GameObject>>();
    }

    /// <summary>
    /// Unityのアップデートメソッド
    /// </summary>
    void Update () 
    {
        if (BodySourceManager == null)
        {
            return;
        }
        
        _BodyManager = BodySourceManager.GetComponent<BodySourceManager>();
        if (_BodyManager == null)
        {
            return;
        }
        
        Kinect.Body[] data = _BodyManager.GetData();
        if (data == null)
        {
            return;
        }
        
        List<ulong> trackedIds = new List<ulong>();
        foreach(var body in data)
        {
            if (body == null)
            {
                continue;
            }
                
            if(body.IsTracked)
            {
                trackedIds.Add (body.TrackingId);
            }
        }
        
        List<ulong> knownIds = new List<ulong>(_Bodies.Keys);
        
        // First delete untracked bodies
        foreach(ulong trackingId in knownIds)
        {
            if(!trackedIds.Contains(trackingId))
            {
                Destroy(_Bodies[trackingId]);
                _Bodies.Remove(trackingId);
                JointsFromBodies.Remove(trackingId);
            }
        }

        foreach(var body in data)
        {
            if (body == null)
            {
                continue;
            }
            
            if(body.IsTracked)
            {
                if(!_Bodies.ContainsKey(body.TrackingId))
                {
                    _Bodies[body.TrackingId] = CreateBodyObject(body.TrackingId);
                }

                RefreshBodyObject(body);
            }
        }

        if(_CoordinateMapper == null)
        {
            _CoordinateMapper = _BodyManager.Sensor.CoordinateMapper;
        }
    }
    
    /// <summary>
    /// Kinectのbodyデータを元にUnityのオブジェクトを生成する
    /// </summary>
    /// <param name="id">Body固有のID</param>
    /// <returns>生成されたBodyのオブジェクト</returns>
    private GameObject CreateBodyObject(ulong id)
    {
        GameObject body = new GameObject(id.ToString());
        var floor = _BodyManager.FloorClipPlane;
        var comp = Quaternion.FromToRotation(new Vector3(floor.X, floor.Y, floor.Z), Vector3.up);

        JointsFromBodies[id] = new Dictionary<Kinect.JointType, GameObject>();
        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {
            GameObject jointObj = new GameObject();
            
            if(IsBodyView)
            {
                LineRenderer lr = jointObj.AddComponent<LineRenderer>();
                lr.positionCount = 2;
                lr.material = BoneMaterial;
                lr.startWidth = 0.05f;
                lr.endWidth = 0.05f;
            }

            jointObj.transform.rotation = _BodyManager.GetJointOrientations(id)[jt].Orientation.ToQuaternion(comp);
            jointObj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            jointObj.name = jt.ToString();
            jointObj.transform.parent = body.transform;
            
            JointsFromBodies[id][jt] = jointObj;
        }
        
        return body;
    }
    
    /// <summary>
    /// Bodyオブジェクトのデータを更新する
    /// </summary>
    /// <param name="body">更新するbodyデータ</param>
    private void RefreshBodyObject(Kinect.Body body)
    {
        var floor = _BodyManager.FloorClipPlane;
        var comp = Quaternion.FromToRotation(new Vector3(floor.X, floor.Y, floor.Z), Vector3.up);

        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {
            Kinect.Joint sourceJoint = body.Joints[jt];
            Kinect.Joint? targetJoint = null;
            
            if(_BoneMap.ContainsKey(jt))
            {
                targetJoint = body.Joints[_BoneMap[jt]];
            }
            
            Transform jointObj = JointsFromBodies[body.TrackingId][jt].transform;
            jointObj.localPosition = GetVector3FromJoint(sourceJoint);
            jointObj.localRotation = jointObj.transform.rotation = body.JointOrientations[jt].Orientation.ToQuaternion(comp);

            if (IsBodyView)
            {
                LineRenderer lr = jointObj.GetComponent<LineRenderer>();

                if (targetJoint.HasValue)
                {
                    lr.SetPosition(0, jointObj.localPosition);
                    lr.SetPosition(1, GetVector3FromJoint(targetJoint.Value));
                    lr.startColor = GetColorForState(sourceJoint.TrackingState);
                    lr.endColor = GetColorForState(targetJoint.Value.TrackingState);
                }
                else
                {
                    lr.enabled = false;
                }
            }
        }
    }
    
    /// <summary>
    /// Kinectの関節情報の確からしさを色で表す。
    /// </summary>
    /// <param name="state">関節状態</param>
    /// <returns>関節状態を表す色</returns>
    private static Color GetColorForState(Kinect.TrackingState state)
    {
        switch (state)
        {
        case Kinect.TrackingState.Tracked:
            return Color.green;

        case Kinect.TrackingState.Inferred:
            return Color.red;

        default:
            return Color.black;
        }
    }
    
    /// <summary>
    /// Kinectの関節座標をUnityの関節座標に変換する
    /// </summary>
    /// <param name="joint">Unity座標に変換する関節</param>
    /// <returns>対応するUnity座標</returns>
    private Vector3 GetVector3FromJoint(Kinect.Joint joint)
    {
        // 変換用カメラがあり、関節がトラッキング中
        if(ConvertCamera != null || (joint.TrackingState != Kinect.TrackingState.NotTracked))
        {
            // 関節座標をカラー画像座標に変換
            var kinectColorPoint = _CoordinateMapper.MapCameraPointToColorSpace(joint.Position);

            // Kinectカラー座標からUnity座標に変換
            Vector3 unityPoint = new Vector3(kinectColorPoint.X, kinectColorPoint.Y, 0);
            if((0 <= unityPoint.x) && (unityPoint.x < _KinectWidth) &&
               (0 <= unityPoint.y) && (unityPoint.y < _KinectHeight))
            {
                // スクリーンサイズを調整
                unityPoint.x = unityPoint.x * Screen.width / _KinectWidth;
                unityPoint.y = unityPoint.y * Screen.height / _KinectHeight;

                // Unityの座標に変換(同じ大きさに合わせた変換用カメラを使用)
                Vector3 colorPoint3 = ConvertCamera.ScreenToWorldPoint(unityPoint);

                // 向きを合わせる
                colorPoint3.x *= (IsMirror) ? -1 : 1;
                colorPoint3.y *= -1;
                colorPoint3.z = -1;

                return colorPoint3;
            }
        }

        // ない場合仕方なく今まで通り
        return new Vector3(joint.Position.X * 10, joint.Position.Y * 10, -1);
    }
}
