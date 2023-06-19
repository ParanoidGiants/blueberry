using System;
using System.Collections;
using DarkTonic.MasterAudio;
using UnityEngine;

public class PigeonSFX : MonoBehaviour
{
    string soundGroup = "PigeonSFX";
    bool played = false;

    // Update is called once per frame
    void Update()
    {
        if (played == false) {
            StartCoroutine(AudioPlayer());
        }
    }

    IEnumerator AudioPlayer()
    {
        played = true;
        int seconds = (int) UnityEngine.Random.Range(0, 6);
        yield return new WaitForSeconds(seconds);
        yield return MasterAudio.PlaySound3DAtTransformAndWaitUntilFinished(soundGroup, transform);
        played = false;
    }
}
