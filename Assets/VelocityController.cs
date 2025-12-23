using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;
using Unity.Robotics.UrdfImporter.Control;

public class VelocityController : MonoBehaviour
{
    public GameObject robotBase;
    public float maxLinearSpeed = 2f;        // m/s
    public float maxRotationalSpeed = 1f;    // rad/s
    public float forceLimit = 10f;
    public float damping = 10f;

    public float ROSTimeout = 0.5f;
    private float lastCmdReceived = 0f;

    ROSConnection ros;
    private float rosLinear = 0f;   // m/s
    private float rosAngular = 0f;  // rad/s

    private Rigidbody rb;

    void Start()
    {
        // ROS setup
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<TwistMsg>("cmd_vel", ReceiveROSCmd);

        // Physics setup
        if (robotBase == null)
        {
            Debug.LogError("VelocityController: robotBase is not assigned!");
            return;
        }

        rb = robotBase.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = robotBase.AddComponent<Rigidbody>();
        }

        // Usually you want the robot controlled by physics
        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationZ;
    }

    void ReceiveROSCmd(TwistMsg cmdVel)
    {
        rosLinear  = (float)cmdVel.linear.x;   // forward m/s
        rosAngular = (float)cmdVel.angular.z;  // yaw rad/s
        lastCmdReceived = Time.time;
    }

    void FixedUpdate()
    {
        ROSUpdate();
    }

    private void ROSUpdate()
    {
        if (Time.time - lastCmdReceived > ROSTimeout)
        {
            rosLinear = 0f;
            rosAngular = 0f;
        }

        // If your coordinate frame is flipped, keep the minus here:
        RobotInput(rosLinear, -rosAngular);
    }

    private void RobotInput(float speed, float rotSpeed) // m/s and rad/s
    {
        if (rb == null) return;

        // Clamp speeds symmetrically
        speed    = Mathf.Clamp(speed,    -maxLinearSpeed,     maxLinearSpeed);
        rotSpeed = Mathf.Clamp(rotSpeed, -maxRotationalSpeed, maxRotationalSpeed);

        // Debug to verify we’re actually getting a non-zero speed from ROS
        // Debug.Log($"cmd_vel linear: {speed}, angular: {rotSpeed}");

        // Linear velocity in world space, along robot's forward
        rb.velocity = robotBase.transform.forward * speed;          // m/s

        // Angular velocity around Y axis (Unity expects radians/sec)
        rb.angularVelocity = new Vector3(0f, rotSpeed, 0f);         // rad/s
    }
}
