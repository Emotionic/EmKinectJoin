using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;

public class WSServer : MonoBehaviour
{
    public bool IsConnected
    {
        get
        {
            return ws != null && ws.IsAlive;
        }
    }

    private WebSocket ws = null;
    private Queue msgQueue;
    private string Address = null;

    private InputField _InputIP;
    private Button _BtnConnect;
    private Text _Label;

    public void BtnConnect_Clicked()
    {
        GetUIComponents();

        if (string.IsNullOrEmpty(_InputIP.text))
        {
            _Label.text = "エラー：IPアドレスが空です";
            return;
        }

        Address = _InputIP.text;
        _BtnConnect.interactable = false;
        _Label.text = "しばらくお待ち下さい...";
        Canvas.ForceUpdateCanvases();

        StartCoroutine(DelayMethod(1.0f, () =>
        {
            Connect();
            Send("KINECTJOIN", "INIT");
            _Label.text = "演技が開始するまでお待ち下さい...";
        }));
        
    }

    public void Connect()
    {
        var res = RequestHTTP(Method.GET, "emkinectjoin");
        if (res != "ok")
            throw new Exception("Cannot connect EmServerWS");

        ws = new WebSocket("ws://" + Address + "/ws");

        ws.OnOpen += (sender, e) =>
        {
            Debug.Log("WebSocket Open");
        };

        ws.OnMessage += (sender, e) =>
        {
            Debug.Log("Data: " + e.Data);
            msgQueue.Enqueue(e.Data);
        };

        ws.OnError += (sender, e) =>
        {
            Debug.Log("WebSocket Error Message: " + e.Message);
        };

        ws.OnClose += (sender, e) =>
        {
            Debug.Log("WebSocket Close");
        };

        ws.Connect();

    }

    public void Send(string _action, object _data, bool _serializeUsingJsonNET = false)
    {
        string msg = "";
        msg += "SERV\n";
        msg += _action + "\n";

        if (_data != null)
        {
            if (_data is string)
            {
                msg += _data;
            }
            else
            {
                if (_serializeUsingJsonNET)
                {
                    msg += JsonConvert.SerializeObject(_data, Formatting.None);
                }
                else
                {
                    msg += JsonUtility.ToJson(_data);
                }
            }
        }

        msg += "\n";

        ws.Send(msg);
    }

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        msgQueue = Queue.Synchronized(new Queue());
    }

    private void Start()
    {

    }

    private void Update()
    {
        if (!IsConnected)
            return;

        lock (msgQueue.SyncRoot)
        {
            try
            {
                foreach (var _msg in msgQueue)
                {
                    var msg = ((string)_msg).Split();

                    switch (msg[1])
                    {
                        /* 初期化 */
                        case "INIT":
                            SceneManager.LoadScene("MainScene");
                            break;

                        /* 再起動 */
                        case "RESTART":
                            SceneManager.LoadScene("Connection");

                            GetUIComponents();
                            _InputIP.text = Address;
                            _BtnConnect.interactable = false;
                            _Label.text = "演技が開始するまでお待ち下さい...";

                            Send("KINECTJOIN", "INIT");
                            break;

                        /* 演技の終了 */
                        case "ENDPERFORM":
                            SceneManager.LoadScene("FinishScene");
                            break;

                        default:
                            break;
                    }

                }
            }
            finally
            {
                msgQueue.Clear();
            }
        }

    }

    private void GetUIComponents()
    {
        var canvas = GameObject.Find("Canvas").transform;
        _InputIP = canvas.Find("InputIP").GetComponent<InputField>();
        _BtnConnect = canvas.Find("BtnConnect").GetComponent<Button>();
        _Label = canvas.Find("Label").GetComponent<Text>();
    }

    private string RequestHTTP(Method method, string action, string value = null)
    {
        string url = "http://" + Address + "/" + action;

        try
        {
            var wc = new System.Net.WebClient();
            string resText = "";

            switch (method)
            {
                case Method.GET:
                    {
                        byte[] resData = wc.DownloadData(url);
                        resText = System.Text.Encoding.UTF8.GetString(resData);
                    }
                    break;

                case Method.POST:
                    {
                        if (value == null)
                            throw new ArgumentException();

                        var ps = new System.Collections.Specialized.NameValueCollection();
                        ps.Add("value", value);
                        byte[] resData = wc.UploadValues(url, ps);
                        resText = System.Text.Encoding.UTF8.GetString(resData);
                    }
                    break;

                default:
                    throw new ArgumentException();

            }

            wc.Dispose();

            return resText;
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// 渡された処理を指定時間後に実行する
    /// </summary>
    /// <param name="waitTime">遅延時間[秒]</param>
    /// <param name="action">実行したい処理</param>
    /// <returns></returns>
    private IEnumerator DelayMethod(float waitTime, Action action)
    {
        yield return new WaitForSeconds(waitTime);
        action();
    }

}

public enum Method
{
    GET,
    POST
}
