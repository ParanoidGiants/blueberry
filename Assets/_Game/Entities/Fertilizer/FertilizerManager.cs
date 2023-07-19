using System.Collections;
using System.Linq;
using UnityEngine;

namespace CollectableFetrilizer
{
    public class FertilizerManager : MonoBehaviour
    {
        [Header("Reference")]
        public Creeper.MeshGenerator _meshGenerator;
        
        private GameUI.FertilizerText _fertilizerText;
        private Transform _destination;
        private Fertilizer[] _fertilizers;

        private int _startAmount;
        private int _totalAmount;
        private int _collectedAmount;
        private int _count;
        
        private bool _isDelivering;
        public bool IsDelivering => _isDelivering;
        private bool _isAllDelivered;
        public bool IsAllDelivered => _isAllDelivered;

        private void Awake()
        {
            _fertilizers = FindObjectsOfType<Fertilizer>();
            _fertilizerText = FindObjectOfType<GameUI.FertilizerText>();
            _destination = FindObjectOfType<Level.EndLevel>().transform;
            _startAmount = _totalAmount = _fertilizers.Length;
        }

        private void Start()
        {
            UpdateUI(false);
        }

        public void OnCollectFertilizer(Fertilizer fertilizer)
        {
            _count++;
            _collectedAmount++;
            UpdateUI();
        }

        public IEnumerator DeliverFertilizer()
        {
            _isDelivering = true;
            
            var collectedToDeliver = _fertilizers.Where(x => x.IsCollected && !x.IsDelivered).ToList();
            var startPosition = _meshGenerator.GetTipPosition();
            foreach (var deliveryItem in collectedToDeliver)
            {
                StartCoroutine(deliveryItem.OnAnimateDelivery(startPosition, _destination.position));
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

        public float GetFertilizerPercentage()
        {
            return (float) _collectedAmount / _startAmount;
        }
    }
}
