using UnityEngine;

namespace DucMinh.MultiProperties
{
    [System.Serializable]
    [InlineLabels("Top", "Right", "Bottom", "Left")]
    public class Padding : FourProperties
    {
        [SerializeField] private float top;
        [SerializeField] private float right;
        [SerializeField] private float bottom;
        [SerializeField] private float left;

        public float Top => top;

        public float Right => right;

        public float Bottom => bottom;

        public float Left => left;
    }
}