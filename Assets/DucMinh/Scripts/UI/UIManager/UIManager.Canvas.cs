using UnityEngine;

namespace DucMinh
{
    public static partial class UIManager
    {
        private static Transform _staticCanvas;
        public static Transform StaticCanvas
        {
            get
            {
                if (_staticCanvas.IsNullObject())
                {
                    _staticCanvas = CreateCanvas("Static Canvas");
                }
                return _staticCanvas;
            }
        }
        
        private static Transform _dynamicCanvas;
        public static Transform DynamicCanvas
        {
            get
            {
                if (_dynamicCanvas.IsNullObject())
                {
                    _dynamicCanvas = CreateCanvas("Dynamic Canvas");
                }
                return _dynamicCanvas;
            }
        }

        private static Transform CreateCanvas(string canvasName)
        {
            var canvas = canvasName.CreateCanvas();
            canvas.planeDistance = 10;
            return canvas.transform;
        }
    }
}