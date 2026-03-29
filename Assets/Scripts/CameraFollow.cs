using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;

    public Vector3 offset = new Vector3(0, 5, -7);
    public float smoothTime = 0.2f;

    public float followDelay = 0.1f;

    public float recoilDistance = 2f;
    public float recoilDuration = 0.1f;

    private Vector3 velocity = Vector3.zero;
    private float delayTimer = 0f;

    private bool isRecoiling = false;
    private float recoilTimer = 0f;
    private Vector3 recoilStartPos;
    private Vector3 recoilTargetPos;

    void LateUpdate()
    {
        // Phase recul caméra
        if (isRecoiling)
        {
            recoilTimer -= Time.deltaTime;

            float t = 1f - (recoilTimer / recoilDuration);

            transform.position = Vector3.Lerp(recoilStartPos, recoilTargetPos, t);

            if (recoilTimer <= 0f)
            {
                isRecoiling = false;
                delayTimer = followDelay;
            }

            return;
        }

        // Phase délai
        if (delayTimer > 0f)
        {
            delayTimer -= Time.deltaTime;
            return;
        }

        // Phase follow smooth
        Vector3 targetPosition = target.position + offset;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            smoothTime
        );

        transform.LookAt(target);
    }

    public void TriggerDashEffect()
    {
        isRecoiling = true;
        recoilTimer = recoilDuration;

        recoilStartPos = transform.position;

        // recul dans la direction opposée au regard
        Vector3 backward = -transform.forward;
        recoilTargetPos = transform.position + backward * recoilDistance;
    }
}