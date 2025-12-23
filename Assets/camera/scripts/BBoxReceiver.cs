using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Perception.GroundTruth;
using RosMessageTypes.Geometry;
using RosMessageTypes.Std;
using RosMessageTypes.Sensor;
using RosMessageTypes.BuiltinInterfaces;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;

public class BBoxReceiver : MonoBehaviour
{
    // private CameraSensorComponent sensorComponent;
    public string IdentifiedTopicName = "/detection";
    private ROSConnection ros;
    // Store already-published detection instance IDs for fast lookup
    private HashSet<string> detectionList = new HashSet<string>();

    void Start()
    {
        // sensorComponent = GetComponent<CameraSensorComponent>();

        var perceptionCamera = GetComponent<PerceptionCamera>();
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<StringMsg>(IdentifiedTopicName);
        if (perceptionCamera == null)
        {
            Debug.LogError("[BBoxReceiver] PerceptionCamera not found.");
            return;
        }

        var bboxLabeler = perceptionCamera.labelers
            .OfType<BoundingBox2DLabeler>()
            .FirstOrDefault();

        if (bboxLabeler == null)
        {
            Debug.LogError("[BBoxReceiver] No BoundingBox2DLabeler found.");
            return;
        }

        bboxLabeler.boundingBoxesCalculated += OnBoundingBoxesCalculated;
    }

    void OnBoundingBoxesCalculated(BoundingBox2DLabeler.BoundingBoxesCalculatedEventArgs args)
    {
        int count = 0;

        foreach (var bbox in args.data)
        {
            // Each bbox has the following fields : label_id, label_name, instance_id, x, y, width, height
            // Debug.Log($"[BBoxReceiver] BBox {count}: Label={bbox.instance_id}, x={bbox.x}, y={bbox.y}, w={bbox.width}, h={bbox.height}");
            count++;
            string id = bbox.instance_id.ToString();
            // Publish only the first time we see this instance id
            if (!detectionList.Contains(id))
            {
                detectionList.Add(id);
                StringMsg InstanceId = new StringMsg();
                InstanceId.data = "female" + id;
                ros.Publish(IdentifiedTopicName, InstanceId);
                UnityEngine.Debug.Log($"Published BBox Instance ID: {InstanceId.data}");
            }
        }
        
    }

    private void OnDestroy()
    {
        var perceptionCamera = GetComponent<PerceptionCamera>();
        if (perceptionCamera != null)
        {
            var bboxLabeler = perceptionCamera.labelers
                .OfType<BoundingBox2DLabeler>()
                .FirstOrDefault();

            if (bboxLabeler != null)
                bboxLabeler.boundingBoxesCalculated -= OnBoundingBoxesCalculated;
        }
    }
}