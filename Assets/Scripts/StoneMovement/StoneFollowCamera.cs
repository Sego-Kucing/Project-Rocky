using UnityEngine;

public class StoneFollowCamera : MonoBehaviour
{
    [Tooltip("The stone to follow.")]
    public Transform target;

    [Tooltip("Fixed world-space offset from the target (not rotated by the stone's aim).")]
    public Vector3 worldOffset = new Vector3(0f, 3f, -6f);

    [Tooltip("How quickly the camera catches up to its target position. Higher = snappier.")]
    public float followSpeed = 8f;

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + worldOffset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
        transform.LookAt(target.position + Vector3.up * 1f);
    }
}