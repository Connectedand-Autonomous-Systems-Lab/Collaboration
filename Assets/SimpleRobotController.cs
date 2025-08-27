using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;
using System.Collections;

public class SimpleRobotController : MonoBehaviour
{
    public GameObject targetObject; // The GameObject you want to move
    private Rigidbody rb;

    public float maxLinearSpeed = 2.0f;
    public float maxAngularSpeed = 1.0f;
    public float rosTimeout = 0.5f;

    private float linearSpeed = 0f;
    private float angularSpeed = 0f;
    private float lastCmdTime = 0f;

    private ROSConnection ros;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<TwistMsg>("cmd_vel", OnCmdVelReceived);
        ArticulationBody rb = GetComponent<ArticulationBody>();
    }

    void OnCmdVelReceived(TwistMsg msg)
    {
        linearSpeed = Mathf.Clamp((float)msg.linear.x, -maxLinearSpeed, maxLinearSpeed);
        angularSpeed = Mathf.Clamp((float)msg.angular.z, -maxAngularSpeed, maxAngularSpeed);
        lastCmdTime = Time.time;
    }

    void FixedUpdate()
    {
        // Transform base_footprint = transform.Find("base_footprint");
        // base_footprint.localPosition = new Vector3(0, 0, 0);
        if (Time.time - lastCmdTime > rosTimeout)
        {
            linearSpeed = 0f;
            angularSpeed = 0f;
        }

        if (targetObject != null)
        {
            // Move forward/backward
            // Debug.Log($"linear: {linearSpeed} Angular: {angularSpeed}");
            // targetObject.transform.Translate(Vector3.forward * linearSpeed * Time.fixedDeltaTime, Space.Self);
            // targetObject.transform.Rotate(Vector3.up, angularSpeed * Mathf.Rad2Deg * Time.fixedDeltaTime, Space.Self);

            Vector3 vel = rb.velocity;
            vel.x = linearSpeed;
            vel.z = angularSpeed;
            rb.velocity = vel;
        }
    }
}
