using UnityEngine;

public class FollowParent : MonoBehaviour
{
    public Transform parentToFollow;

    private Vector3 localOffset;
    private Quaternion localRotationOffset;

    void Start()
    {
        if (parentToFollow != null)
        {
            // Calculate local offset at start
            localOffset = Quaternion.Inverse(parentToFollow.rotation) * (transform.position - parentToFollow.position);
            localRotationOffset = Quaternion.Inverse(parentToFollow.rotation) * transform.rotation;
        }
    }

    void LateUpdate()
    {
        if (parentToFollow != null)
        {
            // Maintain local offset and rotation relative to parent
            transform.position = parentToFollow.position + parentToFollow.rotation * localOffset;
            transform.rotation = parentToFollow.rotation * localRotationOffset;
        }
    }
}
