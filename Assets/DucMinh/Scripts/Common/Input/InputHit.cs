using UnityEngine;

namespace DucMinh
{
    public struct InputHit2D
    {
        public bool HasHit;
        public RaycastHit2D Hit;
        public Vector2 WorldPosition;
        public Collider2D Collider => Hit.collider;
        public GameObject GameObject => Hit.collider ? Hit.collider.gameObject : null;
        public Vector2 Normal => Hit.normal;
        
        public override string ToString()
        {
            if (HasHit)
            {
                return $"Has hit: {HasHit}, World position: {WorldPosition}, GameObject: {GameObject.name}";
            }
            return $"Has hit: {HasHit}, World position: {WorldPosition}, GameObject: null";
        }
    }
    
    public struct InputHit3D
    {
        public bool HasHit;
        public RaycastHit Hit;
        public Vector3 Position;
        public Collider Collider => HasHit ? Hit.collider : null;
        public GameObject GameObject => Collider ? Collider.gameObject : null;
        public Vector3 Normal => HasHit ? Hit.normal : Vector3.forward;

        public override string ToString()
        {
            if (HasHit)
            {
                return $"Has hit: {HasHit}, World position: {Position}, GameObject: {GameObject.name}";
            }
            return $"Has hit: {HasHit}, World position: {Position}, GameObject: null";
        }
    }
}