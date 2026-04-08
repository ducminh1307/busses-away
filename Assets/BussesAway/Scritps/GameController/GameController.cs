using System;
using DucMinh;
using UnityEngine;

namespace BussesAway
{
    public partial class GameController: MonoBehaviour
    {
        #region Input

        private readonly InputRaycastController2D inputController = new();

        #endregion
        
        [SerializeField] private Transform _topLeftTrans;
        [SerializeField] private Transform _bottomRightTrans;
        
        private Vector2 _topLeftPos;
        private Vector2 _bottomRightPos;
        
        public int CurrentLevel { get; private set; } = 1;
        private GameManager gameManager;
        
        private void Awake()
        {
            
            inputController.Handler = this;
            // var blockPrefab = AngryBlockConfig.BlockPrefab;
            // blockPool = new ComponentPool<Block>(blockPrefab, transform);
        }

        public void Update()
        {
            inputController.Update();
            if (InputManager.GetKeyDown(InputKey.R))
            {
                LoadLevel(CurrentLevel);
            }
        }

        public void Construct(GameManager manager)
        {
            gameManager = manager;
        }

        public void Clear()
        {
            
        }
        
        

#if DEBUG_MODE
        [Button]
#endif
        public void LoadLevel(int level)
        {
            CurrentLevel = level;
            AdjustCameraToFitBoard();
        }
        
        private bool CheckWinLevelCondition()
        {
            return true;
        }

        private bool CheckLoseLevelCondition()
        {
            return false;
        }
    }
}