using UnityEngine;

/// <summary>Three production camera modes: driver cockpit, rear chase and front exterior.</summary>
public sealed class CameraFollow : MonoBehaviour
{
    public enum ViewMode { Cockpit = 0, RearChase = 1, FrontExterior = 2 }

    [SerializeField] private Vector3 chaseOffset = new Vector3(0f, 4.2f, -10.5f);
    [SerializeField, Range(1f, 20f)] private float followSpeed = 7f;
    [SerializeField, Range(1f, 20f)] private float lookSpeed = 8f;
    [SerializeField, Range(45f, 85f)] private float exteriorFieldOfView = 66f;
    private Transform target;
    private ViewMode currentMode = ViewMode.Cockpit;

    public bool IsCockpitView => currentMode == ViewMode.Cockpit;
    public ViewMode CurrentMode => currentMode;

    public void SetTarget(Transform value) => target = value;
    public void SetCockpitView(bool enabled) => currentMode = enabled ? ViewMode.Cockpit : ViewMode.RearChase;
    public void CycleView() => currentMode = (ViewMode)(((int)currentMode + 1) % 3);

    private void LateUpdate()
    {
        if (target == null) return;

        if (currentMode == ViewMode.Cockpit)
        {
            Vector3 position = target.TransformPoint(new Vector3(-.52f, 1.92f, 3.62f));
            Vector3 look = target.TransformPoint(new Vector3(-.52f, 1.88f, 15f));
            transform.position = position;
            transform.rotation = Quaternion.LookRotation(look - position, target.up);
            ApplyFieldOfView(66f);
            return;
        }

        Vector3 offset = currentMode == ViewMode.RearChase
            ? chaseOffset
            : new Vector3(0f, 3.2f, 10.5f);
        Vector3 desired = target.TransformPoint(offset);
        transform.position = Vector3.Lerp(transform.position, desired, followSpeed * Time.deltaTime);

        Vector3 lookPoint = currentMode == ViewMode.RearChase
            ? target.position + Vector3.up * 1.55f + target.forward * 3f
            : target.position + Vector3.up * 1.65f;
        transform.rotation = Quaternion.Slerp(transform.rotation,
            Quaternion.LookRotation(lookPoint - transform.position, Vector3.up), lookSpeed * Time.deltaTime);
        ApplyFieldOfView(exteriorFieldOfView);
    }

    private void ApplyFieldOfView(float value)
    {
        Camera camera = GetComponent<Camera>();
        if (camera != null) camera.fieldOfView = value;
    }
}