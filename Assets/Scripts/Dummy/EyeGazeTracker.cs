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
    public Camera gazeCamera;
    public RawImage gazeCursor;  // Assign in Unity Inspector
    
    private RawImage _gazeCursor;
    private ClientWebSocket ws;
    private CancellationTokenSource cancelToken;

    private ConcurrentQueue<string> messageQueue = new ConcurrentQueue<string>(); // ✅ Queue for thread-safe message handling

    public static Vector2 gazePosition;
    [SerializeField] private LayerMask worldLayers = ~0;

    [Serializable]
    private class GazeData
    {
        public float x;
        public float y;
        public float confidence;
    }

    async void Start()
    {
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
            // The current desktop/native display mode
            var cur = Screen.currentResolution;           // width, height, refreshRateRatio
            int mw = cur.width;
            int mh = cur.height;
            
            if (gazeData != null && gazeCursor != null)
            {

                float screenX = (int)gazeData.x;
                float screenY = (int)gazeData.y;

                screenX = screenX / 1920 * mw;
                screenY = screenY / 1080 * mh;
                _gazeCursor.rectTransform.anchoredPosition = new Vector2(screenX, -screenY);

                screenX = (int)gazeData.x;
                screenY = 1080 - (int)gazeData.y; // because in Eyetracker 0,0 is top left. In Unity UI, 0,0 is bottom left

                Ray ray = gazeCamera.ScreenPointToRay(new Vector3(screenX, screenY, 0f));
                if (Physics.Raycast(ray, out RaycastHit hit, 1000, worldLayers, QueryTriggerInteraction.Ignore))
                {
                    Debug.DrawRay(ray.origin, ray.direction * 10, Color.yellow);
                    Debug.Log($"World hit:  object:{hit.collider.gameObject.name} current res:{mw},{mh} screen point:{screenX},{screenY} hit 3d point: {hit.point}");
                }
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
