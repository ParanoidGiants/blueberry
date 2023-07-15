using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CollectableFetrilizer
{
    public class FertilizerManager : MonoBehaviour
    {
        private GameUI.FertilizerText _fertilizerText;
        private Transform _destination;
        private Fertilizer[] _fertilizers;
        
        private int _totalAmount;
        private int _count;
        private bool _isDelivering;
        private bool _isDelivered;
        public bool IsDelivering => _isDelivering;
        public bool IsDelivered => _isDelivered;

        private void Awake()
        {
            _fertilizers = FindObjectsOfType<Fertilizer>();
            _fertilizerText = FindObjectOfType<GameUI.FertilizerText>();
            _destination = FindObjectOfType<Level.End>().transform;
            _totalAmount = _fertilizers.Length;
        }

        private void Start()
        {
            UpdateUI();
        }

        public void OnCollectFertilizer(Fertilizer fertilizer)
        {
            _count++;
            UpdateUI();
        }

        public void InitDeliverFertilizer()
        {
            if (_isDelivering) return;
            StartCoroutine(DeliverFertilizer());
        }

        private IEnumerator DeliverFertilizer()
        {
            _isDelivering = true;
            var collectedFertilizers = _fertilizers.Where(x => x.IsCollected && !x.IsDelivered).ToList();
            if (collectedFertilizers.Count == 0) yield break;

            foreach (var collectedFertilizer in collectedFertilizers)
            {
                collectedFertilizer.OnDeliver(_destination.position);
                _count--;
                _totalAmount--;
                UpdateUI();
                yield return new WaitForSeconds(0.2f);
            }
            yield return new WaitUntil(() => AreAllFertilizersDelivered(collectedFertilizers));
            
            _isDelivered = _totalAmount == 0;
            _isDelivering = false;
        }

        private bool AreAllFertilizersDelivered(List<Fertilizer> collected)
        {
            foreach (var fertilizer in collected)
            {
                if (!fertilizer.IsDelivered) return false;
            }
            return true;
        }

        private void UpdateUI()
        {
            _fertilizerText.UpdateText(_count, _totalAmount);
        }
    }
}
