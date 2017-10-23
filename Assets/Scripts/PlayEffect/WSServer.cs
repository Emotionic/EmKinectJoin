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
    public delegate void EndPerformHandler();
    public event EndPerformHandler EndPerform;

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

    public void BtnConnect_Clicked()
    {
        Address = GameObject.Find("InputIP").GetComponent<InputField>().text;
        SceneManager.LoadScene("MainScene");
    }

    public void Connect()
    {
        var res = RequestHTTP(Method.GET, "check");
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
                        /* 演技の終了 */
                        case "ENDPERFORM":
                            EndPerform();
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

}

public enum Method
{
    GET,
    POST
}
