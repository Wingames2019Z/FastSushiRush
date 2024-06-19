using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using GoogleMobileAds.Api;
public class RewardAds : MonoBehaviour
{
    private RewardedAd rewardedAd;
    private Action EndAction;
    string adUnitId;
    // Start is called before the first frame update
    void Start()
    {
        MobileAds.SetiOSAppPauseOnBackground(true);
#if UNITY_ANDROID
        adUnitId = "ca-app-pub-7158360636123601/6373359743";
#elif UNITY_IPHONE
        adUnitId = "ca-app-pub-7158360636123601/2075022081";
#else
            adUnitId = "unexpected_platform";
#endif
        LoadAd();
        InitializeCallBack();
    }

    public void HandleRewardedAdLoaded(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleRewardedAdLoaded event received");
    }

    public void HandleRewardedAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        MonoBehaviour.print(
            "HandleRewardedAdFailedToLoad event received with message: "
                             + args);
    }

    public void HandleRewardedAdOpening(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleRewardedAdOpening event received");
    }

    public void HandleRewardedAdFailedToShow(object sender, AdErrorEventArgs args)
    {
        MonoBehaviour.print(
            "HandleRewardedAdFailedToShow event received with message: "
                             + args.Message);
        LoadAd();
        InitializeCallBack();
    }

    public void HandleRewardedAdClosed(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleRewardedAdClosed event received");
        LoadAd();
        InitializeCallBack();
    }

    public void HandleUserEarnedReward(object sender, Reward args)
    {
        string type = args.Type;
        double amount = args.Amount;
        MonoBehaviour.print(
            "HandleRewardedAdRewarded event received for "
                        + amount.ToString() + " " + type);
        EndAction();

    }
    public void Show(Action endAction)
    {
        EndAction = endAction;
        if (this.rewardedAd.IsLoaded())
        {
            this.rewardedAd.Show();
        }
        else
        {
            EndAction();
        }
    }

    private void LoadAd()
    {
        this.rewardedAd = new RewardedAd(adUnitId);
        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the rewarded ad with the request.
        this.rewardedAd.LoadAd(request);
    }
    private void InitializeCallBack()
    {
        // Called when an ad request has successfully loaded.
        this.rewardedAd.OnAdLoaded += HandleRewardedAdLoaded;
        // Called when an ad request failed to load.
        this.rewardedAd.OnAdFailedToLoad += HandleRewardedAdFailedToLoad;
        // Called when an ad is shown.
        this.rewardedAd.OnAdOpening += HandleRewardedAdOpening;
        // Called when an ad request failed to show.
        this.rewardedAd.OnAdFailedToShow += HandleRewardedAdFailedToShow;
        // Called when the user should be rewarded for interacting with the ad.
        this.rewardedAd.OnUserEarnedReward += HandleUserEarnedReward;
        // Called when the ad is closed.
        this.rewardedAd.OnAdClosed += HandleRewardedAdClosed;

    }
}
