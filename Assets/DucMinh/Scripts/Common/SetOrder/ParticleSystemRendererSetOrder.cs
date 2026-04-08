using UnityEngine;

namespace DucMinh
{
    [RequireComponent(typeof(ParticleSystemRenderer))]
    public class ParticleSystemRendererSetOrder : MonoBehaviour, ISetOrder
    {
        private ParticleSystemRenderer _particleRenderer;

        private void Awake()
        {
            _particleRenderer = GetComponent<ParticleSystemRenderer>();
        }

        public void SetOrder(string sortingLayer, int orderInLayer)
        {
            if (_particleRenderer == null)
            {
                _particleRenderer = GetComponent<ParticleSystemRenderer>();
            }

            if (_particleRenderer != null)
            {
                _particleRenderer.sortingLayerName = sortingLayer;
                _particleRenderer.sortingOrder = orderInLayer;
            }
        }
    }
}
