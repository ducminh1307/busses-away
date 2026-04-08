using System;

namespace DucMinh.Ads
{
    public static class AdsService
    {
        private static bool _isInitialized;
        private static IAdsProvider _provider;

        public static AdsProviderType ProviderType { get; private set; } = AdsProviderType.LevelPlay;
        public static bool IsInitialized => _isInitialized;
        public static bool IsRemoveAds { get; set; }

        public static void Init()
        {
            if (_isInitialized) return;

            _provider = CreateProvider(ProviderType);
            if (_provider == null)
            {
                Log.Warning($"[AdsService] Unsupported ads provider: {ProviderType}");
                return;
            }

            _provider.Initialize();
            _isInitialized = true;

            LoadBanner();
            LoadInterstitial();
            LoadRewarded();
        }

        public static void SetProvider(AdsProviderType providerType)
        {
            if (_isInitialized)
            {
                Log.Warning("[AdsService] Cannot change ads provider after initialization.");
                return;
            }

            ProviderType = providerType;
        }

        public static void Destroy()
        {
            if (_provider == null) return;

            _provider.DestroyAds();
            _provider = null;
            _isInitialized = false;
        }

        public static void LoadBanner()
        {
            if (!EnsureInitialized()) return;

            _provider.LoadBanner();
        }

        public static void ShowBanner(BannerPosition position = BannerPosition.Bottom)
        {
            if (IsRemoveAds) return;
            if (!EnsureInitialized()) return;

            _provider.ShowBanner(position);
        }

        public static void HideBanner()
        {
            if (!EnsureInitialized()) return;

            _provider.HideBanner();
        }

        public static void LoadInterstitial()
        {
            if (!EnsureInitialized()) return;
            if (_provider.IsInterstitialReady) return;

            _provider.LoadInterstitial();
        }

        public static void ShowInterstitial(Action<bool> onAdClosed = null)
        {
            if (IsRemoveAds)
            {
                onAdClosed?.Invoke(true);
                return;
            }

            if (!EnsureInitialized())
            {
                onAdClosed?.Invoke(false);
                return;
            }

            if (_provider.IsInterstitialReady)
            {
                _provider.ShowInterstitial(success =>
                {
                    onAdClosed?.Invoke(success);
                    LoadInterstitial();
                });
                return;
            }

            Log.Warning("[AdsService] No interstitial ad is ready to be shown.");
            onAdClosed?.Invoke(false);
            LoadInterstitial();
        }

        public static void LoadRewarded()
        {
            if (!EnsureInitialized()) return;
            if (_provider.IsRewardedReady) return;

            _provider.LoadRewarded();
        }

        public static void ShowRewarded(Action<bool, bool> onAdClosed = null)
        {
            if (!EnsureInitialized())
            {
                onAdClosed?.Invoke(false, false);
                return;
            }

            if (_provider.IsRewardedReady)
            {
                _provider.ShowRewarded((closed, rewarded) =>
                {
                    onAdClosed?.Invoke(closed, rewarded);
                    LoadRewarded();
                });
                return;
            }

            Log.Warning("[AdsService] No rewarded ad is ready to be shown.");
            onAdClosed?.Invoke(false, false);
            LoadRewarded();
        }

        private static IAdsProvider CreateProvider(AdsProviderType providerType)
        {
            return providerType switch
            {
                AdsProviderType.UnityAds => new UnityAdsProvider(),
                AdsProviderType.AdMob => new AdMobProvider(),
                AdsProviderType.LevelPlay => new LevelPlayProvider(),
                _ => null
            };
        }

        private static bool EnsureInitialized()
        {
            if (_provider != null) return true;

            Init();
            return _provider != null;
        }
    }
}
