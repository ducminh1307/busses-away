using UnityEngine;
using UnityEngine.SceneManagement;

namespace DucMinh
{
    public static class Context
    {
        private static Camera _mainCamera;

        public static Camera MainCamera
        {
            get
            {
                if (_mainCamera.IsNullObject())
                {
                    _mainCamera = Camera.main;
                }

                return _mainCamera;
            }
        }
        
        private static Camera _selectedCamera;

        public static Camera SelectedCamera
        {
            get
            {
                if (_selectedCamera.IsNullObject())
                {
                    GameObject.Find("Selected Camera")?.TryGetComponent(out _selectedCamera);
                }
                if (_selectedCamera.IsNullObject())
                {
                    _selectedCamera = Camera.main;
                }

                return _selectedCamera;
            }
        }
        
        private static Camera _uiCamera;

        public static Camera UICamera
        {
            get
            {
                if (_uiCamera.IsNullObject())
                {
                    GameObject.Find("UI Camera")?.TryGetComponent(out _uiCamera);
                }   
                if (_uiCamera.IsNullObject())
                {
                    _uiCamera = Camera.main;
                }

                return _uiCamera;
            }
        }
    }
}