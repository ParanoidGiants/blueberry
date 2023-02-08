using System;
using System.Collections;
using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    public Transform Target;
    private Coroutine rotateRoutine;
    public bool IsRotating;

    void Update()
    {
        transform.position = Target.position;
    }

    public void InitRotate(Quaternion targetRotation, Action callback)
    {
        if (rotateRoutine != null)
        {
            StopCoroutine(rotateRoutine);
        }
        IsRotating = true;
        rotateRoutine = StartCoroutine(Rotate(targetRotation, callback));
    }

    private IEnumerator Rotate(Quaternion targetRotation, Action callback)
    {
        var currentRotation = transform.rotation;
        float rotateTime = 0f;
        float rotateIn = 1f;

        while (rotateTime < rotateIn)
        {
            transform.rotation = Quaternion.Lerp(currentRotation, targetRotation, rotateTime/rotateIn);
            rotateTime += Time.deltaTime;
            yield return null;
        }
        transform.rotation = targetRotation;
        IsRotating = false;
        callback();
    }
}
