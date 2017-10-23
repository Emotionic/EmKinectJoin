using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Windows.Kinect;
using Microsoft.Kinect.VisualGestureBuilder;

namespace Assets.KinectView.Scripts
{
    public delegate void GestureHandler(KeyValuePair<Gesture, DiscreteGestureResult> result, ulong id);

    public class GestureManager : MonoBehaviour
    {
        /// <summary>
        /// Gestureが検出されたイベント
        /// </summary>
        public event GestureHandler GestureDetected;

        public List<ulong> TrackingIds
        {
            get
            {
                return _TrackedIds;
            }
        }

        /// <summary>
        /// BodySourceManagerクラス取得用オブジェクト
        /// </summary>
        public GameObject BodySourceManager;
        
        /// <summary>
        /// インスタンス化されたBodyManager
        /// </summary>
        private BodySourceManager _BodyManager;

        /// <summary>
        /// ジェスチャーのデータベース
        /// </summary>
        private VisualGestureBuilderDatabase _GestureDatabase;

        /// <summary>
        /// ジェスチャーのソースリスト
        /// </summary>
        private List<VisualGestureBuilderFrameSource> _GestureFrameSourcesList;

        /// <summary>
        /// ジェスチャーのリーダリスト
        /// </summary>
        private List<VisualGestureBuilderFrameReader> _GestureFrameReadersList;

        /// <summary>
        /// トラック中のIDリスト
        /// </summary>
        private List<ulong> _TrackedIds;

        /// <summary>
        /// 取得したGestureリザルト
        /// </summary>
        private Dictionary<Gesture, DiscreteGestureResult> _DiscreteGestureResults;

        /// <summary>
        /// 学習済みのGestureリスト
        /// </summary>
        private List<Gesture> _Gestures = new List<Gesture>();

        /// <summary>
        /// Gestureが追加されているか
        /// </summary>
        private bool _IsAddGesture = false;

        /// <summary>
        /// Gesture名
        /// </summary>
        public List<string> GestureNames { get; private set; }

        /// <summary>
        /// Kinectで取得できる人数
        /// </summary>
        private const int AcquirableBodyNumber = 6;

        private void Start()
        {
            // 各種初期化
            GestureNames = new List<string>();
            _GestureFrameSourcesList = new List<VisualGestureBuilderFrameSource>(AcquirableBodyNumber);
            _GestureFrameReadersList = new List<VisualGestureBuilderFrameReader>(AcquirableBodyNumber);
            _TrackedIds = new List<ulong>();

            // データベースを登録
            _GestureDatabase = VisualGestureBuilderDatabase.Create(@"./Gestures/Emotionic.gbd");

            // 利用可能なGestureの名前とそれ自身をリストに追加する
            foreach (var gesture in _GestureDatabase.AvailableGestures)
            {
                GestureNames.Add(gesture.Name);
                _Gestures.Add(gesture);
            }
            
        }

        private void Update()
        {
            if (BodySourceManager == null)
                return;
            
            _BodyManager = BodySourceManager.GetComponent<BodySourceManager>();
            
            if (_BodyManager == null)
                return;
            
            if (!_IsAddGesture)
            {
                Gesture[] gestures = _Gestures.ToArray();

                for (int i = 0; i < AcquirableBodyNumber; i++)
                {
                    _GestureFrameSourcesList.Add(VisualGestureBuilderFrameSource.Create(_BodyManager.Sensor, 0));

                    if (_GestureFrameSourcesList[i] == null)
                        continue;

                    _GestureFrameReadersList.Add(_GestureFrameSourcesList[i].OpenReader());

                    _GestureFrameSourcesList[i].AddGestures(gestures);

                    _GestureFrameReadersList[i].IsPaused = true;
                }

                _IsAddGesture = true;
            }
            
            FindValidBodys();
            
            for (int i = 0; i < AcquirableBodyNumber; i++)
            {
                if (_GestureFrameSourcesList[i].IsTrackingIdValid)
                {
                    DetectGestures
                        (
                        _GestureFrameReadersList[i].CalculateAndAcquireLatestFrame(),
                        _GestureFrameSourcesList[i].TrackingId
                        );
                }
            }
        }

        /// <summary>
        /// IDのBodyからGestureを検出する
        /// </summary>
        /// <param name="gestureFrame">検出に用いるGestureFrame</param>
        /// <param name="id">BodyのID</param>
        private void DetectGestures(VisualGestureBuilderFrame gestureFrame, ulong id)
        {
            if (gestureFrame == null || gestureFrame.DiscreteGestureResults == null)
                return;

            _DiscreteGestureResults = gestureFrame.DiscreteGestureResults;

            if (_DiscreteGestureResults == null)
                return;

            foreach (var result in _DiscreteGestureResults)
            {
                if (result.Value == null || !result.Value.Detected)
                    continue;

                Debug.Log("SEND EVENT : " + result.Key.Name + " : " + result.Value.Confidence);
                GestureDetected(result, id);
            }
        }


        /// <summary>
        /// 現在有効なBodyを探す
        /// </summary>
        private void FindValidBodys()
        {
            if (_BodyManager == null)
                return;

            Body[] bodies = _BodyManager.GetData();
            if (bodies == null)
                return;

            foreach (Body body in bodies)
            {
                if (!body.IsTracked)
                    continue;

                if (_TrackedIds.Contains(body.TrackingId))
                    continue;

                for (int i = 0; i < AcquirableBodyNumber; i++)
                {
                    if (_GestureFrameSourcesList[i] == null || _GestureFrameReadersList[i] == null)
                        continue;

                    if (!_GestureFrameSourcesList[i].IsTrackingIdValid)
                    {
                        SetBody(body.TrackingId, i);
                        break;
                    }
                }

            }
        }


        /// <summary>
        /// GestureへBodyを設定する
        /// </summary>
        /// <param name="id">設定するID</param>
        /// <param name="i">設定するGestureデータ番号</param>
        private void SetBody(ulong id, int i)
        {
            if (_GestureFrameSourcesList[i].TrackingId > 0)
            {
                _TrackedIds.Remove(_GestureFrameSourcesList[i].TrackingId);
                Debug.Log("Resetbody : " + i + " : " + _GestureFrameSourcesList[i].TrackingId);
                _GestureFrameSourcesList[i].TrackingId = 0;
                _GestureFrameReadersList[i].IsPaused = true;
            }

            Debug.Log("Setbody : " + i + " : " + id);
            _GestureFrameSourcesList[i].TrackingId = id;
            _GestureFrameReadersList[i].IsPaused = false;
            _TrackedIds.Add(id);
        }

    }
}
