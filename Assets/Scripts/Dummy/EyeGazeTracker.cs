using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class EyeGazeTracker : MonoBehaviour
{
    private Canvas canvas;
    public RawImage gazeCursor;  // Assign in Unity Inspector
    private RawImage _gazeCursor;
    private ClientWebSocket ws;
    private CancellationTokenSource cancelToken;
    private float screenWidth;
    private float screenHeight;

    private ConcurrentQueue<string> messageQueue = new ConcurrentQueue<string>(); // ✅ Queue for thread-safe message handling

    public static Vector2 gazePosition;

    [Serializable]
    private class GazeData
    {
        public float x;
        public float y;
        public float confidence;
    }

    async void Start()
    {
        screenWidth = Screen.width;
        screenHeight = Screen.height;
        ws = new ClientWebSocket();
        cancelToken = new CancellationTokenSource();

        canvas = FindObjectOfType<Canvas>();
        if(canvas != null){
            _gazeCursor = Instantiate(gazeCursor, Vector3.zero, Quaternion.identity, canvas.transform);
        }

        await ConnectWebSocket();
    }

    private async Task ConnectWebSocket()
    {
        try
        {
            await ws.ConnectAsync(new Uri("ws://localhost:4300"), CancellationToken.None);
            Debug.Log("✅ Connected to WebSocket Server!");

            _ = Task.Run(ReceiveMessages);  // Start receiving messages asynchronously
        }
        catch (Exception ex)
        {
            Debug.LogError("❌ WebSocket connection failed: " + ex.Message);
        }
    }

    private async Task ReceiveMessages()
    {
        byte[] buffer = new byte[1024];

        while (ws.State == WebSocketState.Open)
        {
            try
            {
                WebSocketReceiveResult result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), cancelToken.Token);
                string jsonData = Encoding.UTF8.GetString(buffer, 0, result.Count);

                messageQueue.Enqueue(jsonData); //  Add message to the queue instead of processing directly
            }
            catch (Exception ex)
            {
                Debug.LogError("❌ WebSocket receive error: " + ex.Message);
                break;
            }
        }
    }

    private void Update()
    {
        while (messageQueue.TryDequeue(out string jsonData))  //  Process messages safely in Unity's main thread
        {
            ProcessGazeData(jsonData);
        }
    }

    private void ProcessGazeData(string jsonData)
    {
        try
        {
            GazeData gazeData = JsonUtility.FromJson<GazeData>(jsonData);

            if (gazeData != null && gazeCursor != null)
            {
                float screenX = gazeData.x ;
                float screenY = (-gazeData.y) ;  // Flip Y for Unity UI

                // Debug.Log("Gaze Position: x=" + screenX + ", y=" + screenY);

                
                _gazeCursor.rectTransform.anchoredPosition = new Vector2(screenX, screenY);
                gazePosition = new Vector2(screenX, 1080 + screenY);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("❌ Error parsing gaze data: " + ex.Message);
        }
    }

    private async void OnDestroy()
    {
        if (ws != null)
        {
            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            ws.Dispose();
        }
    }
}
