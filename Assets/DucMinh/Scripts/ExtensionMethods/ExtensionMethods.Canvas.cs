using System;
using UnityEngine;
using UnityEngine.UI;

namespace DucMinh
{
    public static partial class ExtensionMethods
    {
        public enum CanvasMatchType
        {
            Width,
            Middle,
            Height,
        }
        
        public static void SetCanvasSortingLayerName(this Canvas canvas, string sortingLayerName)
        {
            if (canvas == null)
            {
                Log.Debug("Canvas is null");
                return;
            }

            if (SortingLayer.NameToID(sortingLayerName) != 0)
            {
                canvas.sortingLayerName = sortingLayerName;
            }
            else
            {
                Log.Debug($"Sorting layer '{sortingLayerName}' does not exist.");
            }
        }
        
        public static void SetCanvasSortingLayerID(this Canvas canvas, int sortingLayerID)
        {
            if (canvas == null)
            {
                Log.Debug("Canvas is null");
                return;
            }

            if (SortingLayer.IsValid(sortingLayerID))
            {
                canvas.sortingLayerID = sortingLayerID;
            }
            else
            {
                Log.Debug($"Invalid sorting layer ID '{sortingLayerID}'.");
            }
        }
        
        public static void SetCanvasSortingOrder(this Canvas canvas, int sortingOrder)
        {
            if (canvas == null)
            {
                Log.Debug("Canvas is null");
                return;
            }
            
            canvas.sortingOrder = sortingOrder;
        }

        public static string GetCanvasSortingLayerName(this Canvas canvas)
        {
            return canvas?.sortingLayerName ?? StringNull;
        }

        public static int GetCanvasSortingLayerID(this Canvas canvas)
        {
            return canvas?.sortingLayerID ?? 0;
        }

        public static int GetCanvasSortingOrder(this Canvas canvas)
        {
            return canvas?.sortingOrder ?? 0;
        }

        public static Canvas CreateCanvas(this GameObject gObj, bool canvasHorizontal = false, CanvasMatchType matchType = CanvasMatchType.Width)
        {
            var canvas = gObj.GetOrAddComponent<Canvas>();
            
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = Context.MainCamera;
            
            canvas.SetCanvasSortingOrder(0);
            
            var scaler = canvas.GetOrAddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            float match = matchType switch
            {
                CanvasMatchType.Middle => 0.5f,
                CanvasMatchType.Width => 0,
                CanvasMatchType.Height => 1,
                _ => 0
            };
            scaler.matchWidthOrHeight = match;
            scaler.referenceResolution = canvasHorizontal ? new Vector2(1920, 1080) : new Vector2(1080, 1920);

            canvas.GetOrAddComponent<GraphicRaycaster>();
            
            return canvas;
        }

        public static Canvas CreateCanvas(this string gObjName, bool canvasHorizontal = false, CanvasMatchType matchType = CanvasMatchType.Width)
        {
            var newObject = gObjName.Create();
            var canvas = newObject.CreateCanvas(canvasHorizontal, matchType);
            return canvas;
        }
    }
}