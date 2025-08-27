using UnityEngine;

public class CarWaypointFollower : MonoBehaviour
{
    public Transform[] waypoints;
    public float speed = 5f;
    public float reachThreshold = 0.5f;

    private int currentWaypointIndex = 0;

    void Update()
    {
        if (waypoints.Length == 0) return;

        Transform target = waypoints[currentWaypointIndex];
        Vector3 direction = (target.position - transform.position).normalized;

        // Move forward
        transform.position += direction * speed * Time.deltaTime;

        // Rotate smoothly towards the waypoint
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 2f);

        // Check if the car has reached the waypoint
        if (Vector3.Distance(transform.position, target.position) < reachThreshold)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
    }
}
