using System;

namespace DucMinh.Ads
{
    public class AdMobProvider : IAdsProvider
    {
        private bool _initialized;
        public bool Initialized => _initialized;

        public AdsProviderType AdsProviderType => AdsProviderType.AdMob;

        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public bool IsInterstitialReady { get; }
        public void LoadInterstitial()
        {
            throw new NotImplementedException();
        }

        public void ShowInterstitial(Action<bool> onCompleted)
        {
            throw new NotImplementedException();
        }

        public bool IsRewardedReady { get; }
        public void LoadRewarded()
        {
            throw new NotImplementedException();
        }

        public void ShowRewarded(Action<bool, bool> onCompleted)
        {
            throw new NotImplementedException();
        }

        public void LoadBanner()
        {
            throw new NotImplementedException();
        }

        public void ShowBanner(BannerPosition position)
        {
            throw new NotImplementedException();
        }

        public void HideBanner()
        {
            throw new NotImplementedException();
        }

        public void DestroyAds()
        {
            throw new NotImplementedException();
        }
    }
}