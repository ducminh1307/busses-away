using System;

namespace DucMinh.Ads
{
    public interface IAdsProvider
    {
        bool Initialized { get; }
        AdsProviderType AdsProviderType { get; }

        void Initialize();

        //Interstitial
        bool IsInterstitialReady { get; }
        void LoadInterstitial();
        void ShowInterstitial(Action<bool> onCompleted);

        //Rewarded
        bool IsRewardedReady { get; }
        void LoadRewarded();
        void ShowRewarded(Action<bool, bool> onCompleted);

        //Banner
        void LoadBanner();
        void ShowBanner(BannerPosition position);
        void HideBanner();
        void DestroyAds();
    }
}