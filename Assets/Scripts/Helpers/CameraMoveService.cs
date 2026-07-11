using System.Collections;
using UnityEngine;

public sealed class CameraMoveService : MonoBehaviour
{
    private Coroutine moveRoutine;
    private Vector3 defaultPos;
    private Quaternion defaultRot;

    private void Awake()
    {
        defaultPos = transform.position;
        defaultRot = transform.rotation;
    }

    public void PushForward(float distance, float duration)
    {
        StopMove();

        Vector3 start = transform.position;
        Vector3 target = start + transform.forward * distance;

        moveRoutine = StartCoroutine(MoveRoutine(start, target, duration));
    }

    public void StopMove()
    {
        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        moveRoutine = null;
    }

    public void ResetToDefault(float duration)
    {
        StopMove();

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        moveRoutine = StartCoroutine(ResetRoutine(startPos, startRot, duration));
    }

    private IEnumerator MoveRoutine(Vector3 start, Vector3 target, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            transform.position = Vector3.Lerp(start, target, k);
            yield return null;
        }
        transform.position = target;
        moveRoutine = null;
    }

    private IEnumerator ResetRoutine(Vector3 startPos, Quaternion startRot, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            transform.position = Vector3.Lerp(startPos, defaultPos, k);
            transform.rotation = Quaternion.Slerp(startRot, defaultRot, k);
            yield return null;
        }

        transform.position = defaultPos;
        transform.rotation = defaultRot;
        moveRoutine = null;
    }
    public void PushForwardAndUp(float forwardDistance, float upDistance, float duration)
    {
        StopMove();

        Vector3 start = transform.position;
        Vector3 target =
            start
            + transform.forward * forwardDistance
            + transform.up * upDistance;

        moveRoutine = StartCoroutine(MoveRoutine(start, target, duration));
    }
    public void MoveTo(Vector3 worldPos, Vector3 worldEuler, float duration)
    {
        StopMove();

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        Vector3 targetPos = worldPos;
        Quaternion targetRot = Quaternion.Euler(worldEuler);

        moveRoutine = StartCoroutine(MoveToRoutine(startPos, startRot, targetPos, targetRot, duration));
    }
    public void MoveToDelayed(Vector3 pos, Vector3 rot, float duration, float delay)
    {
        StopMove();
        StartCoroutine(MoveToDelayedRoutine(pos, rot, duration, delay));
    }

    private IEnumerator MoveToDelayedRoutine(
        Vector3 pos,
        Vector3 rot,
        float duration,
        float delay)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        yield return MoveToRoutine(
            transform.position,
            transform.rotation,
            pos,
            Quaternion.Euler(rot),
            duration
        );
    }

    private IEnumerator MoveToRoutine(Vector3 startPos, Quaternion startRot, Vector3 targetPos, Quaternion targetRot, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            transform.position = Vector3.Lerp(startPos, targetPos, k);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, k);
            yield return null;
        }

        transform.position = targetPos;
        transform.rotation = targetRot;
        moveRoutine = null;
    }


}
