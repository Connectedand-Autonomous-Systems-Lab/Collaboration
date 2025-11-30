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
using System.Threading.Tasks;
using UnityEditor;

public class PVGazePublisher : MonoBehaviour
{
    [Header("Camera Settings")]
    public Camera cam; // Drag and drop the camera here in the Inspector

    [Header("ROS Settings")]
    public string gazeTopicName = "/gaze/point";
    // public string hitObjectTopicName = "/gaze/hit_object";
    public float gazePublishRate = 60.0f; // Publish rate in Hz

    public bool PVPublisher = false;
    public string pvTopicName = "/pv";
    public float pvPublishRate = 5.0f; // Publish rate in Hz
    public string cameraFrameID = "human/camera";

    [Header("PV Saver Settings")]
    public bool PVSaver = false;
    public string saveFolderPath = "SavedImages";
    // public string saveFolderPath = EditorUtility.OpenFolderPanel("Select Directory", "", "");

    private ROSConnection ros;
    private float gazeTimer;
    private float pvTimer;
    Texture2D camTexture;
    private Texture2D imageTexture;
    // --- SavePV optimisation fields ---
    [Header("Save/PV settings")]
    [Tooltip("Downscale factor for saving images. Higher = smaller image and faster processing.")]
    public int saveDownscale = 4;

    [Tooltip("JPEG encoding quality used for SavePV (1-100). Lower = faster + smaller files.")]
    [Range(1, 100)]
    public int saveJpegQuality = 60;

    // Reusable RT & texture for saving - avoids creating/destroying objects every frame
    private RenderTexture saveRT = null;
    private Texture2D saveTex = null;

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
            // ensure save folder exists if we may save
            if (PVSaver && !string.IsNullOrEmpty(saveFolderPath))
            {
                try
                {
                    Directory.CreateDirectory(saveFolderPath);
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"Couldn't create save folder {saveFolderPath}: {ex.Message}");
                }
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

        // *** COMPRESS / DOWNSCALE ***
        float scale = 0.125f; // set your desired scale here
        int newWidth = Mathf.RoundToInt(camTexture.width * scale);
        int newHeight = Mathf.RoundToInt(camTexture.height * scale);
        Texture2D scaledTex = new Texture2D(newWidth, newHeight, TextureFormat.RGB24, false);
        for (int y = 0; y < newHeight; y++)
        {
            for (int x = 0; x < newWidth; x++)
            {
                Color c = camTexture.GetPixelBilinear(
                    (float)x / newWidth,
                    (float)y / newHeight
                );
                scaledTex.SetPixel(x, y, c);
            }
        }
        scaledTex.Apply();
    
        // Convert to byte[] (RGB24)
        byte[] imageBytes = scaledTex.GetRawTextureData();

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
            height = (uint)newHeight,
            width = (uint)newWidth,
            encoding = "rgb8",
            is_bigendian = 0,
            step = (uint)(newWidth * 3),
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
        if (cam == null)
        {
            Debug.LogWarning("SavePV: camera is null, skipping save.");
            return;
        }

        // desired smaller size (downscale factor)
        int width = Mathf.Max(1, cam.pixelWidth / Mathf.Max(1, saveDownscale));
        int height = Mathf.Max(1, cam.pixelHeight / Mathf.Max(1, saveDownscale));

        // Reuse or create RT/Texture for saving to avoid allocations every call
        if (saveRT == null || saveRT.width != width || saveRT.height != height)
        {
            if (saveRT != null)
            {
                saveRT.Release();
                Destroy(saveRT);
                saveRT = null;
            }
            if (saveTex != null)
            {
                Destroy(saveTex);
                saveTex = null;
            }

            saveRT = new RenderTexture(width, height, 24);
            saveTex = new Texture2D(width, height, TextureFormat.RGB24, false);
        }

        // Render to smaller RT directly (faster + less memory)
        cam.targetTexture = saveRT;
        cam.Render();
        RenderTexture.active = saveRT;

        // Read pixels quickly into the small texture
        saveTex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        saveTex.Apply(false, false);

        // Cleanup render targets
        cam.targetTexture = null;
        RenderTexture.active = null;

        // Encode to JPG (faster and smaller than PNG) and write to disk on a background thread
        byte[] jpgBytes = saveTex.EncodeToJPG(saveJpegQuality);

        var timestamp = new TimeStamp(Clock.time);
        string filename = $"{timestamp.Seconds}_{timestamp.NanoSeconds}.jpg";
        string savePath = Path.Combine(saveFolderPath, filename);

        // Ensure folder exists
        try
        {
            Directory.CreateDirectory(saveFolderPath);
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"SavePV: couldn't create folder '{saveFolderPath}': {ex.Message}");
        }

        // Write file asynchronously to avoid blocking main thread
        Task.Run(() =>
        {
            try
            {
                File.WriteAllBytes(savePath, jpgBytes);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"SavePV: failed to write '{savePath}': {ex.Message}");
            }
        });
    }

    void OnApplicationQuit()
    {
        // Clean up the server thread on application quit
        
    }
}
