using UnityEngine;
using UnityEngine.Rendering;

namespace DucMinh
{
    [RequireComponent(typeof(SortingGroup))]
    public class SortingGroupSetOrder : MonoBehaviour, ISetOrder
    {
        private SortingGroup _sortingGroup;

        private void Awake()
        {
            _sortingGroup = GetComponent<SortingGroup>();
        }

        public void SetOrder(string sortingLayer, int orderInLayer)
        {
            if (_sortingGroup == null)
            {
                _sortingGroup = GetComponent<SortingGroup>();
            }

            if (_sortingGroup != null)
            {
                _sortingGroup.sortingLayerName = sortingLayer;
                _sortingGroup.sortingOrder = orderInLayer;
            }
        }
    }
}
