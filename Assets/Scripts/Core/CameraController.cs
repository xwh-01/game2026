using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Transform target;
    private Vector3 velocity;
    private float shakeTime;
    private float shakeStrength;

    private const float FollowSmoothTime = 0.12f;

    public void Initialize(Transform followTarget)
    {
        target = followTarget;
        if (target != null)
        {
            Vector3 targetPosition = target.position;
            transform.position = new Vector3(targetPosition.x, targetPosition.y, -10f);
        }
    }

    public void Shake(float strength, float duration)
    {
        shakeStrength = Mathf.Max(shakeStrength, strength);
        shakeTime = Mathf.Max(shakeTime, duration);
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 targetPosition = new Vector3(target.position.x, target.position.y, -10f);
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, FollowSmoothTime);

        if (shakeTime > 0f)
        {
            shakeTime -= Time.deltaTime;
            float shakeFactor = shakeTime > 0f ? shakeTime : 0f;
            Vector2 offset = Random.insideUnitCircle * shakeStrength * shakeFactor;
            smoothedPosition += new Vector3(offset.x, offset.y, 0f);
        }

        transform.position = smoothedPosition;
    }
}
