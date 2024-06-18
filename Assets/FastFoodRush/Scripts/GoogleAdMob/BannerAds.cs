using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;

public class BannerAds : MonoBehaviour
{
    private BannerView bannerView;
    // Start is called before the first frame update
    void Start()
    {
        MobileAds.Initialize(initStatus => { });
        this.RequestBanner();
    }
    private void RequestBanner()
    {
#if UNITY_ANDROID
        string adUnitId = "ca-app-pub-7158360636123601/9586702634";
#elif UNITY_IPHONE
        string adUnitId = "ca-app-pub-7158360636123601/2318586702";
#else
            string adUnitId = "unexpected_platform";
#endif
        // Create a 320x50 banner at the bottom of the screen.
        this.bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Bottom);
        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the banner with the request.
        bannerView.LoadAd(request);
    }
}
