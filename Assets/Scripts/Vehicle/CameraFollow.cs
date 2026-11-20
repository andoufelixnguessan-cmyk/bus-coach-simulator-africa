using UnityEngine;

public sealed class CameraFollow : MonoBehaviour
{
    [SerializeField] private Vector3 offset = new Vector3(0f, 4.2f, -10.5f);
    [SerializeField, Range(1f, 20f)] private float followSpeed = 7f;
    [SerializeField, Range(1f, 20f)] private float lookSpeed = 8f;
    private Transform target;

    public void SetTarget(Transform value) => target = value;

    private void LateUpdate()
    {
        if (target == null) return;
        Vector3 desiredPosition = target.TransformPoint(offset);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
        Vector3 lookPoint = target.position + Vector3.up * 1.55f + target.forward * 3f;
        Quaternion desiredRotation = Quaternion.LookRotation(lookPoint - transform.position, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, lookSpeed * Time.deltaTime);
    }
}
