using System;
#if LEVEL_PLAY_ENABLED
using Unity.Services.LevelPlay;
using UnityEngine.Advertisements;
#endif

namespace DucMinh.Ads
{
    public class LevelPlayProvider : IAdsProvider
    {
        #region Config

        private const string AndroidAppKey = "24314c5ad";
        private const string IOSAppKey = "";

        private string BannerAdUnitId
        {
            get
            {
#if UNITY_ANDROID
                return "2yhi6bjqnysvrp0j";
#elif UNITY_IPHONE
                return "iep3rxsyp9na3rw8";
#else
                return "unexpected_platform";
#endif
            }
        }

        private string InterstitialAdUnitId
        {
            get
            {
#if UNITY_ANDROID
                return "mb5mkfjdq7rroevf";
#elif UNITY_IPHONE
            return "wmgt0712uuux8ju4";
#else
                return "unexpected_platform";
#endif
            }
        }

        private string RewardedAdUnitId
        {
            get
            {
#if UNITY_ANDROID
                return "reg1gnhs1zlbl39t";
#elif UNITY_IPHONE
            return "qwouvdrkuwivay5q";
#else
                return "unexpected_platform";
#endif
            }
        }

        #endregion

        public bool Initialized { get; private set; }
        public AdsProviderType AdsProviderType => AdsProviderType.LevelPlay;

        private Action<bool> _onInterstitialClosed;
        private Action<bool, bool> _onRewardedClosed;

        private bool _isBannerLoadRequested;
        private bool _isBannerShowRequested;
        private bool _isInterstitialLoadRequested;
        private bool _isRewardedLoadRequested;


#if LEVEL_PLAY_ENABLED
        private LevelPlayBannerAd _bannerAd;
        private LevelPlayInterstitialAd _interstitialAd;
        private LevelPlayRewardedAd _rewardedAd;
#endif

        #region Init

        public void Initialize()
        {
#if LEVEL_PLAY_ENABLED
            LevelPlay.ValidateIntegration();
            LevelPlay.OnInitSuccess += OnSDKInitialized;
            LevelPlay.OnInitFailed += OnSDKInitializationFailed;
#if UNITY_ANDROID
            LevelPlay.Init(AndroidAppKey);
#endif
#if UNITY_IPHONE
            LevelPlay.Init(IOSAppKey);
#endif
#else
#if DEBUG_MODE
            Initialized = true;
#else
            Initialized = false;
#endif
#endif
        }
#if LEVEL_PLAY_ENABLED
        private void OnSDKInitialized(LevelPlayConfiguration levelPlayConfiguration)
        {
            Log.Debug("LevelPlay SDK Initialized");
            EnableAds();
            Initialized = true;
            
            if (_isBannerLoadRequested) LoadBanner();
            if (_isInterstitialLoadRequested) LoadInterstitial();
            if (_isRewardedLoadRequested) LoadRewarded();
        }

        private void OnSDKInitializationFailed(LevelPlayInitError levelPlayInitError)
        {
            Log.Debug(
                $"LevelPlay SDK Initialization Failed [{levelPlayInitError.ErrorCode}]: {levelPlayInitError.ErrorMessage}");
            Initialized = false;
        }
        
        private void EnableAds()
        {
            var configBuilder = new LevelPlayBannerAd.Config.Builder();
            configBuilder.SetSize(LevelPlayAdSize.BANNER);
            var bannerConfig = configBuilder.Build();
            
            _bannerAd = new LevelPlayBannerAd(BannerAdUnitId, bannerConfig);
            _bannerAd.OnAdLoaded += OnBannerLoaded;
            _bannerAd.OnAdLoadFailed += OnBannerLoadFailed;
            _bannerAd.OnAdDisplayed += OnBannerDisplayed;
            _bannerAd.OnAdDisplayFailed += OnBannerDisplayFailed;
            _bannerAd.OnAdClicked += OnBannerClicked;
            _bannerAd.OnAdCollapsed += OnBannerCollapsed;
            _bannerAd.OnAdLeftApplication += OnBannerLeftApplication;
            _bannerAd.OnAdExpanded += OnBannerAdExpanded;
            
            _interstitialAd = new LevelPlayInterstitialAd(InterstitialAdUnitId);
            _interstitialAd.OnAdLoaded += OnInterLoaded;
            _interstitialAd.OnAdLoadFailed += OnInterLoadFailed;
            _interstitialAd.OnAdDisplayed += OnInterDisplayed;
            _interstitialAd.OnAdDisplayFailed += OnInterDisplayFailed;
            _interstitialAd.OnAdClicked += OnInterOnAdClicked;
            _interstitialAd.OnAdClosed += OnInterClosed;
            _interstitialAd.OnAdInfoChanged += OnInterOnAdInfoChanged;
            
            _rewardedAd = new LevelPlayRewardedAd(RewardedAdUnitId);
            _rewardedAd.OnAdLoaded += OnRewardedLoaded;
            _rewardedAd.OnAdLoadFailed += OnRewardedLoadFailed;
            _rewardedAd.OnAdDisplayed += OnRewardedDisplayed;
            _rewardedAd.OnAdDisplayFailed += OnRewardedDisplayFailed;
            _rewardedAd.OnAdRewarded += OnRewarded;
            _rewardedAd.OnAdClicked += OnRewardedClick;
            _rewardedAd.OnAdClosed += OnRewardedClosed;
            _rewardedAd.OnAdInfoChanged += OnRewardedInfoChanged;
        }
#endif

        #endregion

        #region Interstitial

        public bool IsInterstitialReady
        {
            get
            {
#if LEVEL_PLAY_ENABLED
                return _interstitialAd?.IsAdReady() ?? false;
#else
#if DEBUG_MODE
                return true;
#else
                return false;
#endif
#endif
            }
        }

        public void LoadInterstitial()
        {
#if LEVEL_PLAY_ENABLED
            if (_interstitialAd == null)
            {
                _isInterstitialLoadRequested = true;
                return;
            }
            _interstitialAd?.LoadAd();
#endif
        }

        public void ShowInterstitial(Action<bool> onCompleted)
        {
            if (IsInterstitialReady)
            {
                _onInterstitialClosed = onCompleted;
#if LEVEL_PLAY_ENABLED
                _interstitialAd?.ShowAd();
#else
                _onInterstitialClosed?.Invoke(true);
                _onInterstitialClosed = null;
#endif
            }
            else
            {
                onCompleted?.Invoke(false);
            }
        }

        #endregion

        #region Rewarded

        private bool _isRewarded;

        public bool IsRewardedReady
        {
            get
            {
#if LEVEL_PLAY_ENABLED
                return _rewardedAd?.IsAdReady() ?? false;
#else
#if DEBUG_MODE
                return true;
#else
                return false;
#endif
#endif
            }
        }

        public void LoadRewarded()
        {
#if LEVEL_PLAY_ENABLED
            if (_rewardedAd == null)
            {
                _isRewardedLoadRequested = true;
                return;
            }
            _rewardedAd?.LoadAd();
#endif
        }

        public void ShowRewarded(Action<bool, bool> onCompleted)
        {
            if (IsRewardedReady)
            {
                _isRewarded = false;
                _onRewardedClosed = onCompleted;
#if LEVEL_PLAY_ENABLED
                _rewardedAd?.ShowAd();
#else
                _isRewarded = true;
                _onRewardedClosed?.Invoke(true, _isRewarded);
                _onRewardedClosed = null;
#endif
            }
            else
            {
                onCompleted?.Invoke(false, false);
            }
        }

        #endregion

        #region Banner

        public void LoadBanner()
        {
            Log.Debug("[LevelPlay] Load Banner");
#if LEVEL_PLAY_ENABLED
            if (_bannerAd == null)
            {
                _isBannerLoadRequested = true;
                return;
            }
            _bannerAd?.LoadAd();
#endif
        }

        public void ShowBanner(BannerPosition position)
        {
            Log.Debug("[LevelPlay] Show Banner");
#if LEVEL_PLAY_ENABLED
            if (_bannerAd == null)
            {
                _isBannerShowRequested = true;
                return;
            }
            _bannerAd?.ShowAd();
#endif
        }

        public void HideBanner()
        {
#if LEVEL_PLAY_ENABLED
            _bannerAd?.HideAd();
#endif
        }

        #endregion

        public void DestroyAds()
        {
#if LEVEL_PLAY_ENABLED
            _bannerAd?.DestroyAd();
            _rewardedAd?.DestroyAd();
            _interstitialAd?.DestroyAd();
#endif
        }

        #region Ads Events

#if LEVEL_PLAY_ENABLED
        private void OnRewardedLoaded(LevelPlayAdInfo adInfo)
        {
            Log.Debug($"[LevelPlaySample] Received RewardedVideoOnLoadedEvent With AdInfo: {adInfo}");
        }

        private void OnRewardedLoadFailed(LevelPlayAdError error)
        {
            Log.Debug($"[LevelPlaySample] Received RewardedVideoOnAdLoadFailedEvent With Error: {error}");
        }

        private void OnRewardedDisplayed(LevelPlayAdInfo adInfo)
        {
            Log.Debug($"[LevelPlaySample] Received RewardedVideoOnAdDisplayedEvent With AdInfo: {adInfo}");
        }

        private void OnRewardedDisplayFailed(LevelPlayAdInfo adInfo, LevelPlayAdError error)
        {
            _onRewardedClosed?.Invoke(false, false);
            _onRewardedClosed = null;
            _isRewarded = false;
            Log.Debug($"[LevelPlaySample] Received RewardedVideoOnAdDisplayedFailedEvent With AdInfo: {adInfo} and Error: {error}");
        }

        private void OnRewarded(LevelPlayAdInfo adInfo, LevelPlayReward reward)
        {
            _isRewarded = true;
            Log.Debug($"[LevelPlaySample] Received RewardedVideoOnAdRewardedEvent With AdInfo: {adInfo} and Reward: {reward}");
        }

        private void OnRewardedClick(LevelPlayAdInfo adInfo)
        {
            Log.Debug($"[LevelPlaySample] Received RewardedVideoOnAdClickedEvent With AdInfo: {adInfo}");
        }

        private void OnRewardedClosed(LevelPlayAdInfo adInfo)
        {
            _onRewardedClosed?.Invoke(true, _isRewarded);
            _onRewardedClosed = null;
            Log.Debug($"[LevelPlaySample] Received RewardedVideoOnAdClosedEvent With AdInfo: {adInfo}");
        }

        private void OnRewardedInfoChanged(LevelPlayAdInfo adInfo)
        {
            Log.Debug($"[LevelPlaySample] Received RewardedVideoOnAdInfoChangedEvent With AdInfo {adInfo}");
        }

        private void OnInterLoaded(LevelPlayAdInfo adInfo)
        {
            Log.Debug($"[LevelPlaySample] Received InterstitialOnAdLoadedEvent With AdInfo: {adInfo}");
        }

        private void OnInterLoadFailed(LevelPlayAdError error)
        {
            Log.Debug($"[LevelPlaySample] Received InterstitialOnAdLoadFailedEvent With Error: {error}");
        }

        private void OnInterDisplayed(LevelPlayAdInfo adInfo)
        {
            Log.Debug($"[LevelPlaySample] Received InterstitialOnAdDisplayedEvent With AdInfo: {adInfo}");
        }

        private void OnInterDisplayFailed(LevelPlayAdInfo adInfo, LevelPlayAdError error)
        {
            _onInterstitialClosed?.Invoke(false);
            _onInterstitialClosed = null;
            Log.Debug(
                $"[LevelPlaySample] Received InterstitialOnAdDisplayFailedEvent With AdInfo: {adInfo} and Error: {error}");
        }

        private void OnInterOnAdClicked(LevelPlayAdInfo adInfo)
        {
            Log.Debug($"[LevelPlaySample] Received InterstitialOnAdClickedEvent With AdInfo: {adInfo}");
        }

        private void OnInterClosed(LevelPlayAdInfo adInfo)
        {
            _onInterstitialClosed?.Invoke(true);
            _onInterstitialClosed = null;
            Log.Debug($"[LevelPlaySample] Received InterstitialOnAdClosedEvent With AdInfo: {adInfo}");
        }

        private void OnInterOnAdInfoChanged(LevelPlayAdInfo adInfo)
        {
            Log.Debug($"[LevelPlaySample] Received InterstitialOnAdInfoChangedEvent With AdInfo: {adInfo}");
        }

        private void OnBannerLoaded(LevelPlayAdInfo adInfo)
        {
            Log.Debug($"[LevelPlaySample] Received BannerOnAdLoadedEvent With AdInfo: {adInfo}");
            if (_isBannerShowRequested)
            {
                _bannerAd.ShowAd();
                _isBannerShowRequested = false;
            }
        }

        private void OnBannerLoadFailed(LevelPlayAdError error)
        {
            Log.Debug($"[LevelPlaySample] Received BannerOnAdLoadFailedEvent With Error: {error}");
        }

        private void OnBannerClicked(LevelPlayAdInfo adInfo)
        {
            Log.Debug($"[LevelPlaySample] Received BannerOnAdClickedEvent With AdInfo: {adInfo}");
        }

        private void OnBannerDisplayed(LevelPlayAdInfo adInfo)
        {
            Log.Debug($"[LevelPlaySample] Received BannerOnAdDisplayedEvent With AdInfo: {adInfo}");
        }

        private void OnBannerDisplayFailed(LevelPlayAdInfo adInfo, LevelPlayAdError error)
        {
            Log.Debug($"[LevelPlaySample] Received BannerOnAdDisplayFailedEvent With AdInfo: {adInfo} and Error: {error}");
        }

        private void OnBannerCollapsed(LevelPlayAdInfo adInfo)
        {
            Log.Debug($"[LevelPlaySample] Received BannerOnAdCollapsedEvent With AdInfo: {adInfo}");
        }

        private void OnBannerLeftApplication(LevelPlayAdInfo adInfo)
        {
            Log.Debug($"[LevelPlaySample] Received BannerOnAdLeftApplicationEvent With AdInfo: {adInfo}");
        }

        private void OnBannerAdExpanded(LevelPlayAdInfo adInfo)
        {
            Log.Debug($"[LevelPlaySample] Received BannerOnAdExpandedEvent With AdInfo: {adInfo}");
        }

        private void ImpressionDataReadyEvent(LevelPlayImpressionData impressionData)
        {
            Log.Debug($"[LevelPlaySample] Received ImpressionDataReadyEvent ToString(): {impressionData}");
            Log.Debug($"[LevelPlaySample] Received ImpressionDataReadyEvent allData: {impressionData.AllData}");
        }
#endif
        #endregion
    }
}
