using System;
using UnityEngine;

namespace DucMinh
{
    public abstract class BaseUI : MonoBehaviour
    {
        private AnimationAdapter animationAdapter;

        private void Awake()
        {
            animationAdapter = AnimationAdapter.Create(gameObject);
        }

        public void Show(Action onShow = null)
        {
            UpdateUI();
            gameObject.SetAsLastSibling();
            if (animationAdapter != null)
            {
                animationAdapter.PlayWithCallback(AnimationNames.SHOW, onShow);
            }
            else
            {
                onShow?.Invoke();
            }
        }
        protected abstract void UpdateUI();

        public void Hide(Action onHide = null)
        {
            if (animationAdapter != null)
            {
                animationAdapter.PlayWithCallback(AnimationNames.HIDE, onHide);
            }
            else
            {
                onHide?.Invoke();
            }
        }
    }
}