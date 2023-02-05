using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    public Transform Target;
    private Coroutine rotateRoutine;
    public bool IsRotating = false;

    void Update()
    {
        transform.position = Target.position;
    }

    public void InitRotate(Quaternion targetRotation)
    {
        if (rotateRoutine != null)
        {
            StopCoroutine(rotateRoutine);
        }
        rotateRoutine = StartCoroutine(Rotate(targetRotation));
    }

    public IEnumerator Rotate(Quaternion targetRotation)
    {
        IsRotating = true;
        if (targetRotation == Quaternion.identity)
        {
            targetRotation = transform.rotation;
        }
        var currentRotation = transform.rotation;
        Debug.Log("Rotate From: " + currentRotation.eulerAngles);
        Debug.Log("Rotate To: " + targetRotation.eulerAngles);
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
    }
}
