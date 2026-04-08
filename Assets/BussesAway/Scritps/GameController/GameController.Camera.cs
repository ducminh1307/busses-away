using UnityEngine;
using DucMinh;
namespace BussesAway
{
    public partial class GameController
    {
        private Camera _mainCamera;

        private void AdjustCameraToFitBoard()
        {
            if (_mainCamera == null)
            {
                _mainCamera = Context.MainCamera;
                if (_mainCamera == null)
                {
                    Log.Error("Main camera not found");
                    return;
                }
            }
            
            var boardBounds = GetBoardBounds();
            
            // float paddingWorldUnit = 0.5f; 
            var paddingWorldUnit = 0f; 
            var boardHeight = boardBounds.size.y + paddingWorldUnit;
            var boardWidth = boardBounds.size.x + paddingWorldUnit;

            var viewWidth = Mathf.Abs(_bottomRightPos.x - _topLeftPos.x);
            var viewHeight = Mathf.Abs(_topLeftPos.y - _bottomRightPos.y);
            
            var viewRatio = viewWidth / viewHeight;
            
            var boardRatio = boardWidth / boardHeight;

            float targetSize;

            if (boardRatio > viewRatio)
            {
                targetSize = (boardWidth / viewRatio) * 0.5f;
            }
            else
            {
                targetSize = boardHeight * 0.5f;
            }

            targetSize /= 0.85f; 
            
            targetSize = Mathf.Max(targetSize, BussesAwayConfig.MinCameraSize); 
            // targetSize = Mathf.Max(targetSize, 3.5f); 

            _mainCamera.orthographicSize = targetSize;

            var cameraPos = _mainCamera.transform.position;
            
            cameraPos.x = boardBounds.center.x;
            cameraPos.y = boardBounds.center.y;
            
            // cameraPos.y += 0.5f; 

            _mainCamera.transform.position = cameraPos;
        }
    }
}