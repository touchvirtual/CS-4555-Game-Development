using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 8, -7);
    public float followSpeed = 8f;
    public float lookHeight = 1.2f;

    void LateUpdate()
    {
        if (target == null) return;
        Vector3 desired = target.position + target.TransformDirection(offset);
        transform.position = Vector3.Lerp(transform.position, desired, followSpeed * Time.deltaTime);
        transform.LookAt(target.position + Vector3.up * lookHeight);
    }
}
