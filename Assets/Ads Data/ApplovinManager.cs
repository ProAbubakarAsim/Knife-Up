
using UnityEngine;
using System.Collections;
using UnityEngine.Advertisements;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum AdSizes
{
    Banner, SmartBanner
}
public class ApplovinManager : MonoBehaviour, IUnityAdsListener
{

    public static ApplovinManager Instance;
    private InterstitialAd interstitial;
    private BannerView bannerView;
    public RewardedAd rewardedAd;
    private bool unifiedNativeAdLoaded;
    private UnifiedNativeAd nativeAd;
    public GameObject RawImg;
    [Header("Unity Id")]
    public string UnityIdIOS, UnityIdAndroid;
    string myPlacementId = "rewardedVideo";

    [Header("Admob Ad Ids")]
    public string Admob_Interstitial_Android;
    public string Admob_Banner_Android1;
    public string Admob_Banner_Android2;
    public string Admob_SmartBanner_Android3;
    public string Admob_Rewarded_Android;
    public string Admob_Native_Android;
    [Space(20)]
    public string Admob_Interstitial_IOS;
    public string Admob_Banner_IOS;
    public string Admob_Rewarded_IOS;
    public string Admob_Native_IOS;

    [Header("Banner Type")]
    public AdSizes _bannerType;

    [Header("Banner Postition")]
    public AdPosition _bannerPosition1;
    public AdPosition _bannerPosition2;
    public AdPosition _SmartbannerPosition;

    public bool TestAds, TwoBanners;
    #region ----------------------- Start --------------------------
    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
        DontDestroyOnLoad(this.gameObject);

        if (TestAds)
        {
            Admob_Interstitial_Android = Admob_Interstitial_IOS = "ca-app-pub-3940256099942544/1033173712";
            Admob_Banner_Android1 = Admob_Banner_Android2 = Admob_Banner_IOS = "ca-app-pub-3940256099942544/6300978111";
            Admob_Native_Android = Admob_Native_IOS = "ca-app-pub-3940256099942544/2247696110";
            Admob_Rewarded_Android = Admob_Rewarded_IOS = "ca-app-pub-3940256099942544/5224354917";
        }
            if (PlayerPrefs.GetInt("removeads") != 1)
        {

            #region Unity
            Advertisement.AddListener(this);
#if UNITY_ANDROID
            Advertisement.Initialize(UnityIdAndroid, false);
#elif UNITY_IOS
                Advertisement.Initialize(UnityIdIOS, false);
#endif
            #endregion

            #region Admob
            MobileAds.Initialize(initStatus => { });
            RequestInterstitial();
            CreateAndLoadRewardedAd();
            if (PlayerPrefs.GetInt("removeads") != 1)
            {
                ShowBanner();
                if (TwoBanners)
                    ShowBanner2();
            }
            #endregion
        }
    }
    #endregion

    #region -------------- Update -----------------------
    void Update()
    {
        if (this.unifiedNativeAdLoaded)
        {
            this.unifiedNativeAdLoaded = false;
            // Get Texture2D for icon asset of native ad.
            Texture2D iconTexture = this.nativeAd.GetIconTexture();
            if (RawImg)
                RawImg.GetComponent<RawImage>().texture = iconTexture;
            RawImg.GetComponent<RawImage>().color = Color.white;

            // Register GameObject that will display icon asset of native ad.
            if (!this.nativeAd.RegisterIconImageGameObject(RawImg))
            {
                // Handle failure to register ad asset.
            }
        }
    }

    #endregion

    #region --------------Admob Functionality------------------
    public void RequestNativeAd()
    {
#if UNITY_ANDROID
        string adunitid = Admob_Native_Android;
#elif UNITY_IOS
        string adunitid = Admob_Native_IOS;
#endif
        AdLoader adLoader = new AdLoader.Builder(Admob_Native_Android)
            .ForUnifiedNativeAd()
            .Build();
        adLoader.OnUnifiedNativeAdLoaded += this.HandleUnifiedNativeAdLoaded;
        adLoader.LoadAd(new AdRequest.Builder().Build());
    }
    private void HandleUnifiedNativeAdLoaded(object sender, UnifiedNativeAdEventArgs args)
    {
        MonoBehaviour.print("Unified native ad loaded.");
        this.nativeAd = args.nativeAd;
        this.unifiedNativeAdLoaded = true;
    }
    private void RequestInterstitial()
    {
#if UNITY_ANDROID
        string adUnitId = Admob_Interstitial_Android;
#elif UNITY_IPHONE
        string adUnitId = Admob_Interstitial_IOS;
#else
        string adUnitId = "unexpected_platform";
#endif

        // Initialize an InterstitialAd.
        this.interstitial = new GoogleMobileAds.Api.InterstitialAd(adUnitId);
        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the interstitial with the request.
        this.interstitial.LoadAd(request);

        this.interstitial.OnAdFailedToLoad += HandleOnAdFailedToLoad;
        this.interstitial.OnAdClosed += HandleOnAdClosed;

    }
    public void CreateAndLoadRewardedAd()
    {
#if UNITY_ANDROID
        string adUnitId = Admob_Rewarded_Android;
#elif UNITY_IPHONE
        string adUnitId = Admob_Rewarded_IOS;
#else
        string adUnitId = "unexpected_platform";
#endif

        this.rewardedAd = new RewardedAd(adUnitId);
        this.rewardedAd.OnAdFailedToLoad += HandleRewardedAdFailedToLoad;
        this.rewardedAd.OnUserEarnedReward += HandleUserEarnedReward;
        this.rewardedAd.OnAdClosed += HandleRewardedAdClosed;

        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the rewarded ad with the request.
        this.rewardedAd.LoadAd(request);

    }

    public void ShowBanner()
    {
#if UNITY_ANDROID
        string adUnitId = Admob_Banner_Android1;
#elif UNITY_IPHONE
            string adUnitId = Admob_Banner_IOS;
#else
            string adUnitId = "unexpected_platform";
#endif

        // Create a 320x50 banner at the top of the screen.
        bannerView = new BannerView(adUnitId, _bannerType == AdSizes.Banner ? AdSize.Banner : AdSize.SmartBanner, _bannerPosition1);
        AdRequest request = new AdRequest.Builder().Build();

        // Load the banner with the request.
        bannerView.LoadAd(request);
    }
    private void ShowBanner2()
    {
#if UNITY_ANDROID
        string adUnitId = Admob_Banner_Android2;
#elif UNITY_IPHONE
            string adUnitId = Admob_Banner_IOS;
#else
            string adUnitId = "unexpected_platform";
#endif

        // Create a 320x50 banner at the top of the screen.
        bannerView = new BannerView(adUnitId, _bannerType == AdSizes.Banner ? AdSize.Banner : AdSize.SmartBanner, _bannerPosition2);
        AdRequest request = new AdRequest.Builder().Build();

        // Load the banner with the request.
        bannerView.LoadAd(request);
    }
    private void RequestSmartBanner()
    {
#if UNITY_ANDROID
        string adUnitId = Admob_SmartBanner_Android3;
#elif UNITY_IPHONE
            string adUnitId = Admob_Banner_IOS;
#else
            string adUnitId = "unexpected_platform";
#endif

        // Create a 320x50 banner at the top of the screen.
        bannerView = new BannerView(adUnitId, AdSize.SmartBanner, _SmartbannerPosition);
        AdRequest request = new AdRequest.Builder().Build();

        // Load the banner with the request.
        bannerView.LoadAd(request);
    }
    private void RequestSmartBanner1()
    {
#if UNITY_ANDROID
        string adUnitId = Admob_Banner_Android1;
#elif UNITY_IPHONE
            string adUnitId = Admob_Banner_IOS;
#else
            string adUnitId = "unexpected_platform";
#endif

        // Create a 320x50 banner at the top of the screen.
        bannerView = new BannerView(adUnitId, _bannerType == AdSizes.Banner ? AdSize.Banner : AdSize.SmartBanner, _bannerPosition1);
        AdRequest request = new AdRequest.Builder().Build();

        // Load the banner with the request.
        bannerView.LoadAd(request);
    }
    public void HandleUserEarnedReward(object sender, Reward args)
    {
        PlayerPrefs.SetInt("Cash", PlayerPrefs.GetInt("Cash") + 1000);
    }
    public void HandleRewardedAdClosed(object sender, EventArgs args)
    {
        this.CreateAndLoadRewardedAd();
    }
    public void HandleRewardedAdFailedToLoad(object sender, AdErrorEventArgs args)
    {
        //  this.CreateAndLoadRewardedAd();
    }
    public void HandleOnAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        //  RequestInterstitial();
    }

    public void HandleOnAdClosed(object sender, EventArgs args)
    {
        RequestInterstitial();
    }
    #endregion



    #region --------------Unity Functionality------------------

    public void OnUnityAdsDidFinish(string placementId, ShowResult showResult)
    {
        // Define conditional logic for each ad completion status:
        if (showResult == ShowResult.Finished)
        {
            PlayerPrefs.SetInt("Cash", PlayerPrefs.GetInt("Cash") + 1000);

        }
        else if (showResult == ShowResult.Skipped)
        {
            // Do not reward the user for skipping the ad.
        }
        else if (showResult == ShowResult.Failed)
        {
            Debug.LogWarning("The ad did not finish due to an error.");
        }
    }
    public void OnUnityAdsReady(string placementId)
    {
        // If the ready Placement is rewarded, show the ad:
        if (placementId == myPlacementId)
        {
            // Optional actions to take when the placement becomes ready(For example, enable the rewarded ads button)
        }
    }
    public void OnUnityAdsDidError(string message)
    {
        throw new NotImplementedException();
    }

    public void OnUnityAdsDidStart(string placementId)
    {
        throw new NotImplementedException();
    }
    #endregion

    #region ------------------- Ad Calling Functions--------------------
    public void RemoveBanner()
    {
        bannerView.Destroy();
    }


    public void ShowInterstitial()
    {
        if (PlayerPrefs.GetInt("removeads") != 1)
        {
            if (this.interstitial.IsLoaded())
            {
                this.interstitial.Show();
            }
        }
    }
    public void Admob_Unity()
    {
        if (PlayerPrefs.GetInt("removeads") != 1)
        {
            if (this.interstitial.IsLoaded())
            {
                this.interstitial.Show();
            }
            else
            {
                RequestInterstitial();
                if (Advertisement.IsReady())
                {
                    Advertisement.Show();
                }
            }

        }
    }
    public void Unity_Admob()
    {
        if (PlayerPrefs.GetInt("removeads") != 1)
        {
            if (Advertisement.IsReady())
            {
                Advertisement.Show();
            }
            else
                if (this.interstitial.IsLoaded())
            {
                this.interstitial.Show();
            }
            else
            {
                RequestInterstitial();
            }

        }
    }
    public void ShowRewardedAd()
    {
        if (this.rewardedAd.IsLoaded())
        {
            this.rewardedAd.Show();
        }
        else
        {
            this.CreateAndLoadRewardedAd();
            if (Advertisement.IsReady(myPlacementId))
            {
                Advertisement.Show(myPlacementId);
            }
        }
    }
    public void UnityInterstitial()
    {
        if (PlayerPrefs.GetInt("removeads") != 1)
        {
            if (Advertisement.IsReady())
            {
                Advertisement.Show();
            }
        }
    }



    #endregion

}
