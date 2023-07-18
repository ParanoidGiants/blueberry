using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

namespace CollectableFetrilizer
{
    public class FertilizerManager : MonoBehaviour
    {
        private GameUI.FertilizerText _fertilizerText;
        private Transform _destination;
        private Fertilizer[] _fertilizers;
        
        private int _totalAmount;
        private int _count;
        [SerializeField] private bool _isDelivering;
        public bool IsDelivering => _isDelivering;
        [SerializeField] private bool _isAllDelivered;
        public bool IsAllDelivered => _isAllDelivered;

        private void Awake()
        {
            _fertilizers = FindObjectsOfType<Fertilizer>();
            _fertilizerText = FindObjectOfType<GameUI.FertilizerText>();
            _destination = FindObjectOfType<Level.EndLevel>().transform;
            _totalAmount = _fertilizers.Length;
        }

        private void Start()
        {
            UpdateUI(false);
        }

        public void OnCollectFertilizer(Fertilizer fertilizer)
        {
            _count++;
            UpdateUI();
        }

        public IEnumerator DeliverFertilizer()
        {
            _isDelivering = true;
            
            var collectedToDeliver = _fertilizers.Where(x => x.IsCollected && !x.IsDelivered).ToList();
            foreach (var deliveryItem in collectedToDeliver)
            {
                StartCoroutine(deliveryItem.OnAnimateDelivery(_destination.position));
                _count--;
                _totalAmount--;
                UpdateUI();
                yield return new WaitForSeconds(0.2f);
            }

            var waitUntil = new WaitUntil(() => collectedToDeliver.All(x => x.IsDelivered));
            yield return waitUntil;

            _isAllDelivered = _fertilizers.All(x => x.IsDelivered);
            _isDelivering = false;
        }

        private void UpdateUI(bool pulse = true)
        {
            _fertilizerText.UpdateText(_count, _totalAmount, pulse);
        }

        public bool HasNewFertilizer()
        {
            return !_isDelivering && _fertilizers.Any(x => x.IsCollected && !x.IsDelivered);
        }
    }
}
