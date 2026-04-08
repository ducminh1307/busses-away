using System;
using System.Collections.Generic;
using UnityEngine;
using DucMinh.Ads;

namespace DucMinh
{
    public enum GameEventID
    {
        PlaceStack,
        CollectHexagons
    }

    public partial class Manager : SingletonBehavior<Manager>
    {
        public static void Init(Action callback = null)
        {
            Instance.Initialize(callback);
        }

        private bool _initialized = false;
        private void Initialize(Action callback)
        {
            if (_initialized) return;

            TimeHelper.Init();
            AudioManager.Init();
            // IAPHelper.Initialize();
            StorageService.Init();
            AdsService.Init();
            //             AnalyticsService.Initialize(new List<IAnalyticsProvider>
            //             {
            // #if DEBUG_MODE
            //                 new DebugAnalyticsProvider(),
            // #endif
            //             });

            Application.targetFrameRate = 120;
            Input.multiTouchEnabled = false;
            _initialized = true;
            // callback?.Invoke();
        }
    }
}