using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class EncoderReceiver : MonoBehaviour
{
    public int port = 5005;
    private UdpClient client;
    private Thread receiveThread;

    public static float angle = 0f;
    private float targetAngle = 0f;
    private bool running = true;

    // メインスレッドで処理するためのフラグ
    private bool rightTurnReceived = false;
    private bool leftTurnReceived = false;

    void Start()
    {
        client = new UdpClient(port);
        client.Client.ReceiveTimeout = 100; // タイムアウトでスレッド停止防止
        receiveThread = new Thread(ReceiveData);
        receiveThread.IsBackground = true;
        receiveThread.Start();
        Debug.Log("UDP receiver started on port " + port);
    }

    void ReceiveData()
    {
        IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
        while (running)
        {
            try
            {
                byte[] data = client.Receive(ref anyIP);
                string text = Encoding.UTF8.GetString(data).Trim();

                Debug.Log("[UDP Received] " + text);
                
                if (text.StartsWith("Angle:"))
                {
                    if (float.TryParse(text.Split(':')[1], out float newAngle))
                    {
                        targetAngle = newAngle;
                    }
                }
                else if (text == "Rr")
                {
                    targetAngle = 0f;  // 角度リセット
                    rightTurnReceived = true;  // メインスレッドで処理
                }
                else if (text == "Lr")
                {
                    targetAngle = 0f;  // 角度リセット
                    leftTurnReceived = true;  // メインスレッドで処理
                }
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode != SocketError.TimedOut)
                {
                    Debug.LogError("Socket error: " + ex);
                }
            }
        }
    }

    void Update()
    {
        // 角度補正
        float delta = targetAngle - angle;
        if (delta > 180f) delta -= 360f;
        if (delta < -180f) delta += 360f;
        angle += delta;

        // 回転の向き調整ここ
        transform.rotation = Quaternion.Euler(0, -angle, 0);

        // メインスレッドで回転イベント処理
        if (rightTurnReceived)
        {
            GameManager.Instance?.OnRightTurn();
            rightTurnReceived = false;
        }

        if (leftTurnReceived)
        {
            GameManager.Instance?.OnLeftTurn();
            leftTurnReceived = false;
        }
    }

    void OnApplicationQuit()
    {
        running = false;
        receiveThread.Join();
        client.Close();
    }
}
