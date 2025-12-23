using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;

[RequireComponent(typeof(Rigidbody))]
public class CmdVelMover : MonoBehaviour
{
    [Header("ROS")]
    public string topicName = "/cmd_vel";

    [Header("Tuning")]
    public float linearScale = 1.0f;   // tweak to match your units
    public float angularScale = 1.0f;  // tweak to match your units

    private ROSConnection ros;
    private Rigidbody rb;

    // values from last cmd_vel
    private float targetLinearX = 0f;   // forward/back (m/s)
    private float targetAngularZ = 0f;  // yaw (rad/s)

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        rb = GetComponent<Rigidbody>();

        // Subscribe to cmd_vel (geometry_msgs/msg/Twist)
        ros.Subscribe<TwistMsg>(topicName, CmdVelCallback);
    }

    void CmdVelCallback(TwistMsg msg)
    {
        // Twist: linear.x, angular.z for differential drive-style robots
        targetLinearX = (float)msg.linear.x;
        targetAngularZ = (float)msg.angular.z;
    }

    void FixedUpdate()
    {
        // Forward/backward velocity in Unity world
        Vector3 forwardVel = transform.forward * (targetLinearX * linearScale);

        // Angular velocity around Y axis
        Vector3 angularVel = new Vector3(0f, targetAngularZ * angularScale, 0f);

        rb.velocity = forwardVel;
        rb.angularVelocity = -angularVel;
    }
}
