using UnityEngine;

/// <summary>Animated paired windshield wipers with a single, reliable toggle state.</summary>
public sealed class CoachWiperSystem : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float sweepSpeed = 2.2f;
    [SerializeField, Range(10f, 65f)] private float sweepAngle = 42f;
    private Transform leftWiper;
    private Transform rightWiper;
    private Quaternion leftRest;
    private Quaternion rightRest;
    private bool enabledByDriver;

    public bool IsEnabled => enabledByDriver;

    public void Configure(Transform left, Transform right)
    {
        leftWiper = left;
        rightWiper = right;
        if (leftWiper != null) leftRest = leftWiper.localRotation;
        if (rightWiper != null) rightRest = rightWiper.localRotation;
        ApplyRestPose();
    }

    public void Toggle()
    {
        enabledByDriver = !enabledByDriver;
        if (!enabledByDriver) ApplyRestPose();
    }

    private void Update()
    {
        if (!enabledByDriver) return;
        float sweep = Mathf.Sin(Time.time * sweepSpeed * Mathf.PI * 2f) * sweepAngle;
        if (leftWiper != null)
            leftWiper.localRotation = leftRest * Quaternion.Euler(0f, 0f, sweep);
        if (rightWiper != null)
            rightWiper.localRotation = rightRest * Quaternion.Euler(0f, 0f, -sweep);
    }

    private void ApplyRestPose()
    {
        if (leftWiper != null) leftWiper.localRotation = leftRest;
        if (rightWiper != null) rightWiper.localRotation = rightRest;
    }
}