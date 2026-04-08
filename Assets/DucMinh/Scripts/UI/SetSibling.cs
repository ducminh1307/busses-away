using System;
using UnityEngine;

namespace DucMinh
{
    public class SetSibling : MonoBehaviour
    {
        enum SiblingOption
        {
            FirstSibling,
            CustomSibling,
            LastSibling,
        }
        [SerializeField] private SiblingOption siblingOption = SiblingOption.FirstSibling;
        [SerializeField] private int targetSiblingIndex = 0;

        private void Start()
        {
            switch (siblingOption)
            {
                case SiblingOption.FirstSibling:
                    transform.SetAsFirstSibling();
                    break;
                case SiblingOption.CustomSibling:
                    transform.SetSiblingIndex(targetSiblingIndex);
                    break;
                case SiblingOption.LastSibling:
                    transform.SetAsLastSibling();
                    break;
            }
            
        }
    }
}