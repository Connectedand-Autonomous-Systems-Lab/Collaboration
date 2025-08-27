using System;
using System.Collections.Generic;
using RosMessageTypes.Sensor;
using RosMessageTypes.Std;
using RosMessageTypes.Nav;
using RosMessageTypes.Geometry;
using RosMessageTypes.BuiltinInterfaces;
using Unity.Robotics.Core;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine.Serialization;

public class OdomMYR : MonoBehaviour
{
    public string topic;
    [FormerlySerializedAs("TimeBetweenScansSeconds")]
    public double PublishPeriodSeconds = 0.1;

    // Change the scan start and end by this amount after every publish
    public float TimeBetweenMeasurementsSeconds = 0.01f;
    public string LayerMaskName = "TurtleBot3Manual";
    public string FrameId = "tb3_0/odom";

    ROSConnection m_Ros;

    double m_TimeNextScanSeconds = -1;

    protected virtual void Start()
    {
        m_Ros = ROSConnection.GetOrCreateInstance();
        m_Ros.RegisterPublisher<OdometryMsg>(topic);

        m_TimeNextScanSeconds = Clock.Now + PublishPeriodSeconds;
    }

    public void Update()
    {
        if (Clock.NowTimeInSeconds < m_TimeNextScanSeconds)
        {
            return;
        }

        m_TimeNextScanSeconds = Clock.Now + PublishPeriodSeconds;
        var timestamp = new TimeStamp(Clock.time);

        var msg = new OdometryMsg
        {
            header = new HeaderMsg
            {
                frame_id = FrameId,
                stamp = new TimeMsg
                {
                    sec = timestamp.Seconds,
                    nanosec = timestamp.NanoSeconds,
                }
            },
            child_frame_id = "tb3_0/base_footprint",
            pose = new PoseWithCovarianceMsg
            {
                pose = new PoseMsg
                {
                    position = new PointMsg
                    {
                        x = transform.position.x,
                        y = transform.position.z
                    }
                }
            },
        };
        
        // Debug.Log("x: " + transform.position.x + " | y: " + transform.position.y + " | z: " + transform.position.z );
        m_Ros.Publish(topic, msg);


    }
}