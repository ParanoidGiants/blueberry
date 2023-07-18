using System.Collections;
using CollectableFetrilizer;
using UnityEngine;
using UnityEngine.Playables;

namespace Level
{
    public class EndLevel : MonoBehaviour
    {
        [SerializeField] private PlayableDirector _endDirector;
        private FertilizerManager _manager;
        private Animator _animator;

        private bool _isDelivering = false;

        private void Awake()
        {
            _endDirector = FindObjectOfType<GameCamera.CameraController>().EndDirector;
            _manager = FindObjectOfType<FertilizerManager>();
            _animator = GetComponent<Animator>();
            _animator.enabled = false;
        }

        private IEnumerator OnTriggerEnter(Collider other)
        {
            Debug.Log("OnTriggerEnter");
            if (_isDelivering || _manager.IsAllDelivered) yield break;
            _isDelivering = true;
            
            yield return _manager.DeliverFertilizer();
            
            _isDelivering = false;
            if (!_manager.IsAllDelivered) yield break;
            
            _endDirector.Play();
            _animator.enabled = true;
        }
    }
}