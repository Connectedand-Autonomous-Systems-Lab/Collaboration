using UnityEngine;
using RosMessageTypes.Geometry;
using RosMessageTypes.Std;
using RosMessageTypes.Sensor;
using RosMessageTypes.BuiltinInterfaces;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using System;
using System.Text;
using System.Collections.Generic;
using Unity.Robotics.Core;
using UnityEngine.Serialization;
using System.IO;

public class PVGazePublisher : MonoBehaviour
{
    [Header("Camera Settings")]
    public Camera cam; // Drag and drop the camera here in the Inspector

    [Header("ROS Settings")]
    public string gazeTopicName = "/gaze/point";
    // public string hitObjectTopicName = "/gaze/hit_object";
    public bool PVPublisher = false;
    public bool PVSaver = false;
    public string pvTopicName = "/pv";
    public float gazePublishRate = 60.0f; // Publish rate in Hz
    public float pvPublishRate = 5.0f; // Publish rate in Hz
    public string cameraFrameID = "human/camera";

    private ROSConnection ros;
    private float gazeTimer;
    private float pvTimer;
    Texture2D camTexture;
    private Texture2D imageTexture;

    void Start()
    {
        // Initialize ROS Connection
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<PointStampedMsg>(gazeTopicName);
        camTexture = new Texture2D(cam.pixelWidth, cam.pixelHeight, TextureFormat.RGB24, false);
        if (PVPublisher){
            ros.RegisterPublisher<ImageMsg>(pvTopicName);
            pvTimer = 0f;
            
            // Fallback if the camera is not assigned
            if (cam == null)
            {
                Debug.LogError("No camera found. Please assign a camera to the script.");
            }
            // load sample image
            // Texture2D loadedTexture = Resources.Load<Texture2D>("sample");
            // imageTexture = new Texture2D(loadedTexture.width, loadedTexture.height, TextureFormat.RGB24, false);
            // imageTexture.SetPixels(loadedTexture.GetPixels());
            // imageTexture.Apply();
            // Debug.Log("Image successfully loaded. Width: " + imageTexture.width + ", Height: " + imageTexture.height);
        }
        
        // Initialize Timer
        gazeTimer = 0f;

    }

    void Update()
    {
        gazeTimer += Time.deltaTime;
        
        
        // publish gaze
        if (gazeTimer >= 1.0f / gazePublishRate)
        {
            gazeTimer = 0f;
            PublishStampedGaze();
        }

        // publish pv
        if (PVPublisher || PVSaver){
            pvTimer += Time.deltaTime;
            if (pvTimer >= 1.0f / pvPublishRate)
            {
                pvTimer = 0f;
                if (cam != null)
                {   
                    if (PVPublisher){PublishPV();}
                    else if (PVSaver){SavePV();}
                    // PublishSample();
                } 
            }
        }
        
    }

    void FlipTextureVertically(Texture2D tex)
    {
        Color[] pixels = tex.GetPixels();
        int width = tex.width;
        int height = tex.height;

        for (int y = 0; y < height / 2; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int top = y * width + x;
                int bottom = (height - y - 1) * width + x;

                Color temp = pixels[top];
                pixels[top] = pixels[bottom];
                pixels[bottom] = temp;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
    }

    void PublishPV()
    {
        // Create a RenderTexture and copy camera image to it
        RenderTexture rt = new RenderTexture(cam.pixelWidth, cam.pixelHeight, 24);
        cam.targetTexture = rt;
        cam.Render();
        RenderTexture.active = rt;

        // Read pixels into texture
        camTexture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        camTexture.Apply();
        FlipTextureVertically(camTexture);

        // Cleanup
        cam.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        // Convert to byte[] (RGB24)
        byte[] imageBytes = camTexture.GetRawTextureData();

        var timestamp = new TimeStamp(Clock.time);

        // Create ROS Image message
        ImageMsg imgMsg = new ImageMsg
        {
            header = new HeaderMsg
            {
                stamp = new TimeMsg
                {
                    sec = timestamp.Seconds,
                    nanosec = timestamp.NanoSeconds,
                },
                frame_id = cameraFrameID
            },
            height = (uint)camTexture.height,
            width = (uint)camTexture.width,
            encoding = "rgb8",
            is_bigendian = 0,
            step = (uint)(camTexture.width * 3),
            data = imageBytes
        };

        // Publish to ROS
        ros.Publish(pvTopicName, imgMsg);
    
    }

    void PublishSample()
    {
        // Convert to raw RGB24
        Texture2D rgbTex = new Texture2D(imageTexture.width, imageTexture.height, TextureFormat.RGB24, false);

        rgbTex.SetPixels(imageTexture.GetPixels());
        rgbTex.Apply();
        byte[] imageBytes = rgbTex.GetRawTextureData();
        Destroy(rgbTex); // free memory

        // Create timestamp
        TimeStamp timestamp = new TimeStamp(Clock.time);

        ImageMsg imageMsg = new ImageMsg
        {
            header = new HeaderMsg
            {
                stamp = new TimeMsg
                {
                    sec = timestamp.Seconds,
                    nanosec = timestamp.NanoSeconds
                },
                frame_id = cameraFrameID
            },
            height = (uint)imageTexture.height,
            width = (uint)imageTexture.width,
            encoding = "rgb8",
            is_bigendian = 0,
            step = (uint)(imageTexture.width * 3),
            data = imageBytes
        };

        ros.Publish(pvTopicName, imageMsg);
        // Debug.Log("Published saved image to ROS2.");
    }


    void PublishGaze()
    {   
        // Debug.Log(EyeGazeTracker.gazePosition);
        // Debug.Log("Gaze Position: x = " + EyeGazeTracker.gazePosition.x + ", y = " + EyeGazeTracker.gazePosition.y);
        // Create a new Point message
        double x = (double)EyeGazeTracker.gazePosition.x;
        double y = (double)EyeGazeTracker.gazePosition.y;

        PointMsg point = new PointMsg
        {
            x = x,
            y = y,
            z = 0f
        };
        
        ros.Publish(gazeTopicName, point);
        
    }

    void PublishStampedGaze()
    {
        // Debug.Log(EyeGazeTracker.gazePosition);
        // Debug.Log("Gaze Position: x = " + EyeGazeTracker.gazePosition.x + ", y = " + EyeGazeTracker.gazePosition.y);
        // Convert gaze position to double
        double x = (double)EyeGazeTracker.gazePosition.x;
        double y = (double)EyeGazeTracker.gazePosition.y;

        // Create a new PointStamped message
        var timestamp = new TimeStamp(Clock.time);

        PointStampedMsg stampedGaze = new PointStampedMsg
        {
            header = new HeaderMsg
            {
                stamp = new TimeMsg
                {
                    sec = timestamp.Seconds,
                    nanosec = timestamp.NanoSeconds,
                },
                frame_id = cameraFrameID
            },
            point = new PointMsg
            {
                x = x,
                y = y,
                z = 0.0
            }
        };

        // Publish the stamped gaze message
        ros.Publish(gazeTopicName, stampedGaze);
    }

    void SavePV()
    {
        int width = cam.pixelWidth;
        int height = cam.pixelHeight;

        // Create a RenderTexture and copy camera image to it
        RenderTexture rt = new RenderTexture(width, height, 24);
        cam.targetTexture = rt;
        cam.Render();
        RenderTexture.active = rt;

        // Read pixels into texture
        camTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        camTexture.Apply();
        // FlipTextureVertically(camTexture); // Fix upside-down image

        // Cleanup
        cam.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        // Encode to PNG (or JPG if you prefer)
        byte[] pngBytes = camTexture.EncodeToPNG(); // or EncodeToJPG()

        // Timestamp for filename
        var timestamp = new TimeStamp(Clock.time);
        string filename = $"{timestamp.Seconds}_{timestamp.NanoSeconds}.png";

        // Save path (change as needed)
        string savePath = Path.Combine(@"C:\Users\mayoo\Documents\unity_projects\Unity_saved_pv_human_behaviour\maze1", filename);
        // string savePath = Path.Combine(Application.dataPath, "saved_images", filename);
        File.WriteAllBytes(savePath, pngBytes);

        // Debug.Log($"Saved image to: {savePath}");
    }

    void OnApplicationQuit()
    {
        // Clean up the server thread on application quit
        
    }
}
