using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Windows.Kinect;
using Microsoft.Kinect.VisualGestureBuilder;
using Effekseer;
using Kinect = Windows.Kinect;
using System;
using System.IO;

namespace Assets.KinectView.Scripts
{
    public class EffectsFromGesture : MonoBehaviour
    {
        public GameObject GestureManager;

        public GameObject BodySourceManager;

        public GameObject LaunchPad;

        public UnityEngine.AudioSource Audio;

        public AudioClip Clip;

        private WSServer _WSServer;

        private Camera _MainCamera;

        private EffectPrefabs _Prefabs;

        /// <summary>
        /// エフェクト名
        /// </summary>
        private readonly string[] _EffectNames = { "StairBroken", "punch", "laser" };

        private Dictionary<Emotionic.Effect, GameObject> _EffectPrefabs;

        private GestureManager _GestureManager;

        private bool _IsRegMethod = false;

        /// <summary>
        /// Kinect画像と取得した関節情報を表示する
        /// </summary>
        private ColorBodySourceView _ColorBodyView;

        private BodySourceManager _BodyManager;

        private Dictionary<ulong, Dictionary<JointType, GameObject>> _Joints;

        private Dictionary<string, EffectAttributes> _GestureFromEffectAttributes;

        private RainbowColor _RbColor;

        private int _bodyCount = 0;

        // Use this for initialization
        void Start()
        {
            // loadEffect
            foreach (var efkName in _EffectNames)
                EffekseerSystem.LoadEffect(efkName);

            _Prefabs = this.GetComponent<EffectPrefabs>();

            _EffectPrefabs = new Dictionary<Emotionic.Effect, GameObject>()
            {
                {Emotionic.Effect.Beam, _Prefabs.Kamehameha },
                {Emotionic.Effect.Clap, _Prefabs.Clap},
                {Emotionic.Effect.Ripple, _Prefabs.Punch}
            };

            _MainCamera = GameObject.Find("ConvertCamera").GetComponent<Camera>();

            if (GameObject.Find("WSServer") != null)
            {
                _WSServer = GameObject.Find("WSServer").GetComponent<WSServer>();
            }

            _GestureManager = GestureManager.GetComponent<GestureManager>();
            _BodyManager = BodySourceManager.GetComponent<BodySourceManager>();
            _ColorBodyView = BodySourceManager.GetComponent<ColorBodySourceView>();

            // Add effect attributes
            _GestureFromEffectAttributes = new Dictionary<string, EffectAttributes>();
            _GestureFromEffectAttributes["Jump"] = new EffectAttributes(0.3, JointType.SpineMid, 1, _EffectNames[0]);
            _GestureFromEffectAttributes["Punch"] = new EffectAttributes(0.1, JointType.HandRight, 1, Emotionic.Effect.Ripple);
            _GestureFromEffectAttributes["ChimpanzeeClap_Left"] = new EffectAttributes(0.2, JointType.HandTipLeft, 0.1f, Emotionic.Effect.Clap);
            _GestureFromEffectAttributes["ChimpanzeeClap_Right"] = new EffectAttributes(0.2, JointType.HandTipRight, 0.1f, Emotionic.Effect.Clap);
            _GestureFromEffectAttributes["Daisuke"] = new EffectAttributes(0.3, JointType.Head, 3, _EffectNames[0]);
            _GestureFromEffectAttributes["Kamehameha"] = new EffectAttributes(0.15, JointType.HandLeft, 1, Emotionic.Effect.Beam);

            _RbColor = new RainbowColor(0, 0.001f);

        }

        // Update is called once per frame
        void Update()
        {
            if (_GestureManager == null || _ColorBodyView == null || _BodyManager == null)
            {
                _GestureManager = GestureManager.GetComponent<GestureManager>();
                _BodyManager = BodySourceManager.GetComponent<BodySourceManager>();
                _ColorBodyView = BodySourceManager.GetComponent<ColorBodySourceView>();
            }

            if (_MainCamera == null)
                _MainCamera = GameObject.Find("ConvertCamera").GetComponent<Camera>();

            if (!_IsRegMethod)
            {
                _GestureManager.GestureDetected += _GestureManager_GestureDetected;
                _IsRegMethod = true;
            }

            _Joints = _ColorBodyView.JointsFromBodies;

            if (_Joints.Count > _bodyCount)
            {
                Audio.pitch = 1.0f;
                Audio.PlayOneShot(Clip);
            }
            _bodyCount = _Joints.Count;

            foreach (GameObject body in _ColorBodyView.GetBodies())
            {
                AddingTrailRendererToBody(body);
            }

            _RbColor.Update();

        }

        private void _GestureManager_GestureDetected(KeyValuePair<Gesture, DiscreteGestureResult> result, ulong id)
        {
            if (!_GestureFromEffectAttributes.ContainsKey(result.Key.Name))
                return;

            EffectAttributes ea = _GestureFromEffectAttributes[result.Key.Name];
            if (result.Value.Confidence < ea.Threshold)
                return;

            switch (ea.Type)
            {
                case EffectAttributes.EffectType.Effekseer:
                    var h = EffekseerSystem.PlayEffect(ea.EffectName, _Joints[id][ea.AttachPosition].transform.position);
                    h.SetScale(ea.Scale);
                    h.SetRotation(_Joints[id][ea.AttachPosition].transform.rotation);
                    break;

                case EffectAttributes.EffectType.ParticleSystem:
                    var effe = Instantiate(_EffectPrefabs[ea.EffectKey]);
                    effe.transform.position = _Joints[id][ea.AttachPosition].transform.position;

                    // rotate
                    switch (ea.EffectKey)
                    {
                        case Emotionic.Effect.Beam:
                            if (_Joints[id][ea.AttachPosition].transform.position.x < _Joints[id][JointType.SpineMid].transform.position.x)
                            {
                                effe.transform.Rotate(new Vector3(0, 180, 0));
                            }
                            break;
                        case Emotionic.Effect.Ripple:
                            if (_Joints[id][ea.AttachPosition].transform.position.x < _Joints[id][JointType.SpineMid].transform.position.x)
                            {
                                if (_Joints[id][JointType.HandLeft].transform.position.x < _Joints[id][JointType.HandRight].transform.position.x)
                                {
                                    effe.transform.position = _Joints[id][JointType.HandRight].transform.position;
                                }
                                else
                                {
                                    effe.transform.position = _Joints[id][JointType.HandLeft].transform.position;
                                }
                            }
                            else
                            {
                                if (_Joints[id][JointType.HandLeft].transform.position.x < _Joints[id][JointType.HandRight].transform.position.x)
                                {
                                    effe.transform.position = _Joints[id][JointType.HandLeft].transform.position;
                                }
                                else
                                {
                                    effe.transform.position = _Joints[id][JointType.HandRight].transform.position;
                                }
                            }
                            break;
                    }

                    effe.GetComponent<ParticleSystem>().Play(true);
                    Destroy(effe.gameObject, 10);

                    // Send effect to EmServerWS
                    if (IsConnected)
                    {
                        _WSServer.Send("KINECTJOIN", result.Key.Name);
                    }

                    Debug.Log(_Joints[id][ea.AttachPosition].transform.rotation.eulerAngles);
                    break;

            }

        }

        /// <summary>
        /// 両手足にTrailRendererを付ける
        /// </summary>
        /// <param name="body">エフェクトを付けるBody</param>
        private void AddingTrailRendererToBody(GameObject body)
        {
            GameObject[] joints =
            {
                _Joints[ulong.Parse(body.name)][JointType.HandTipRight],
                _Joints[ulong.Parse(body.name)][JointType.HandTipLeft],
                _Joints[ulong.Parse(body.name)][JointType.FootRight],
                _Joints[ulong.Parse(body.name)][JointType.FootLeft],
                _Joints[ulong.Parse(body.name)][JointType.Head],
                _Joints[ulong.Parse(body.name)][JointType.SpineBase]
            };

            for (int i = 0; i < joints.Length; i++)
            {
                Transform trail;
                if (!joints[i].transform.Find(_Prefabs.Trail.name))
                {
                    Instantiate(_Prefabs.Trail, joints[i].transform).name = "Trail";
                }

                trail = joints[i].transform.Find(_Prefabs.Trail.name);

                TrailRenderer tr = trail.GetComponent<TrailRenderer>();

                ParticleSystem[] pss =
                    {
                    trail.Find("Hand Particle").GetComponent<ParticleSystem>(),
                    trail.Find("NG Hand Particle").GetComponent<ParticleSystem>()
                    };

                tr.startColor = _RbColor.Rainbow;
                foreach (ParticleSystem ps in pss)
                {
                    ps.startColor = _RbColor.Rainbow;
                }

            }

        }

        private bool IsConnected
        {
            get { return _WSServer != null && _WSServer.IsConnected; }
        }

    }
}