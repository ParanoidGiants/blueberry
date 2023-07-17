using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

namespace Level
{
    public class End : MonoBehaviour
    {
        private PlayableDirector _endDirector;
        private Animator _animator;
        private bool _isAllDelivered;

        private void Awake()
        {
            _endDirector = FindObjectOfType<GameCamera.CameraController>().EndDirector;
            _animator = GetComponent<Animator>();
            _animator.enabled = false;
            _isAllDelivered = false;
        }

        private IEnumerator OnTriggerEnter(Collider other)
        {
            if (_isAllDelivered) yield break;
            
            var manager = other.GetComponent<CollectableFetrilizer.FertilizerManager>();
            if (manager == null) yield break;

            manager.InitDeliverFertilizer();
            yield return new WaitUntil(() => !manager.IsDelivering);
            
            if (!manager.IsDelivered) yield break;
            
            _isAllDelivered = true;
            _endDirector.Play();
            _animator.enabled = true;
        }
    }
}
