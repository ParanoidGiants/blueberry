using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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
        [SerializeField] private bool _isDelivering;
        [SerializeField] private bool _isDelivered;
        [SerializeField] private bool _isAllDelivered;
        public bool IsDelivering => _isDelivering;
        public bool IsDelivered => _isDelivered;
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
            Debug.Log("DeliverFertilizer");
            if (_isDelivering || _isAllDelivered) yield break;
            _isDelivering = true;
            
            var deliveryfertilizers = _fertilizers.Where(x => x.IsCollected && !x.IsDelivered).ToList();
            if (deliveryfertilizers.Count == 0) yield break;

            foreach (var deliveryFertilizer in deliveryfertilizers)
            {
                deliveryFertilizer.OnDeliver(_destination.position);
                _count--;
                _totalAmount--;
                UpdateUI();
                yield return new WaitForSeconds(0.2f);
            }

            var isCollectedDelivered = false;
            while (!isCollectedDelivered)
            {
                isCollectedDelivered = deliveryfertilizers.All(x => x.IsDelivered);
                yield return null;
            }

            _isAllDelivered = _fertilizers.All(x => x.IsDelivered);
            _isDelivering = false;
        }

        private void UpdateUI(bool pulse = true)
        {
            _fertilizerText.UpdateText(_count, _totalAmount, pulse);
        }
    }
}
