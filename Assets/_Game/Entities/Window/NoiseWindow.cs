using DarkTonic.MasterAudio;
using System;
using System.Collections;
using UnityEngine;

namespace Assets.Window
{
    [Serializable]
    public class NoiseWindow : Window
    {
        protected Noise noise;

        private bool noise_played = false;
        private void Awake()
        {
            jsonContent = jsonFile.ToString();
            noise = JsonUtility.FromJson<Noise>(jsonContent);
        }
        
        override 
        public void UpdateInteraction()
        {
            if (!interaction_ongoing)
            {
                StartInteraction();
            }
            else
            {
                StartCoroutine(ContinueInteraction());
            }
        }

        protected override IEnumerator ContinueInteraction()
        {
            if (!noise_played)
            {
                noise_played = true;
                Debug.Log(noise.text);
                yield return StartCoroutine(MasterAudio.PlaySound3DAtTransformAndWaitUntilFinished(noise.soundGroup, transform));
                noise_played = false;
            }
        }
    }
}