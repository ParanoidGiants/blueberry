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

        private void Awake()
        {
            _endDirector = FindObjectOfType<GameCamera.CameraController>().EndDirector;
            _manager = FindObjectOfType<FertilizerManager>();
            _animator = GetComponent<Animator>();
            _animator.enabled = false;
        }

        private IEnumerator OnTriggerEnter(Collider other)
        {
            if (_manager.IsDelivering || !_manager.HasNewFertilizer()) yield break;
            
            yield return StartCoroutine(_manager.DeliverFertilizer());

            if (_manager.IsAllDelivered)
            {
                _endDirector.Play();
                _animator.enabled = true;
            }
        }
    }
}
