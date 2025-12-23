using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;
using RosMessageTypes.Geometry;
using RosMessageTypes.Tf2;
using RosMessageTypes.BuiltinInterfaces;
using Unity.Robotics.Core;   // Contains namespace TimeStamp

public class SimpleOdomTfPublisher : MonoBehaviour
{
    [Header("TF frames")]
    public string parentFrameId = "odom";
    public string childFrameId  = "base_footprint";

    [Header("ROS")]
    public string tfTopic = "/tf";
    public float publishHz = 30f;

    ROSConnection ros;
    float timeElapsed = 0f;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<TFMessageMsg>(tfTopic);
    }

    void FixedUpdate()
    {
        timeElapsed += Time.fixedDeltaTime;
        if (timeElapsed < 1f / publishHz)
            return;
        timeElapsed = 0f;

        // --- 1) Get pose of this object in Unity world ---
        Vector3 pos = transform.position;
        Quaternion rot = transform.rotation;

        // --- 2) (Simple version) Use Unity pose directly as ROS pose ---
        // If you want *correct* ROS vs Unity axes later, we can add ROSGeometry,
        // but this keeps it dead simple for now.

        // Header: timestamp + parent frame
        // For a simple setup, a zero timestamp is usually fine.
        var timestamp = new TimeStamp(Clock.time);

        var header = new HeaderMsg
        {
            frame_id = parentFrameId,
            stamp = new TimeMsg
            {
                sec = timestamp.Seconds,
                nanosec = timestamp.NanoSeconds,
            }
        };

        // Transform (translation + rotation)
        var transformMsg = new TransformMsg(
            translation: new Vector3Msg(pos.x,pos.z,pos.y),
            rotation:    new QuaternionMsg(rot.x, rot.y, rot.z, rot.w)
        );

        var tfStamped = new TransformStampedMsg(
            header: header,
            child_frame_id: childFrameId,
            transform: transformMsg
        );

        // TFMessage is just an array of TransformStamped
        var tfMessage = new TFMessageMsg(new TransformStampedMsg[] { tfStamped });

        ros.Publish(tfTopic, tfMessage);
    }
}
