package com.pavko.hauntscope.ads;

import android.app.Activity;

import com.google.android.gms.ads.AdError;
import com.google.android.gms.ads.AdRequest;
import com.google.android.gms.ads.FullScreenContentCallback;
import com.google.android.gms.ads.LoadAdError;
import com.google.android.gms.ads.MobileAds;
import com.google.android.gms.ads.interstitial.InterstitialAd;
import com.google.android.gms.ads.interstitial.InterstitialAdLoadCallback;
import com.google.android.gms.ads.rewarded.RewardedAd;
import com.google.android.gms.ads.rewarded.RewardedAdLoadCallback;
import com.google.android.ump.ConsentInformation;
import com.google.android.ump.ConsentRequestParameters;
import com.google.android.ump.UserMessagingPlatform;

import java.util.concurrent.atomic.AtomicBoolean;

/**
 * The only native code of Hauntscope ads: Google UMP consent, the Mobile Ads SDK, and one rewarded and one
 * interstitial ad kept loaded. Unity drives it through JNI (AdMobAdsService); every callback is reported as a
 * named event, because the SDK's callbacks are abstract classes an AndroidJavaProxy cannot implement.
 */
public final class AdsBridge {
    public interface Listener {
        void onEvent(String name, String detail);
    }

    private final Activity activity;
    private final String rewardedUnitId;
    private final String interstitialUnitId;
    private final Listener listener;
    private final AtomicBoolean sdkStarted = new AtomicBoolean(false);

    private ConsentInformation consent;
    private RewardedAd rewarded;
    private InterstitialAd interstitial;
    private boolean loadingRewarded;
    private boolean loadingInterstitial;

    public AdsBridge(Activity activity, String rewardedUnitId, String interstitialUnitId, Listener listener) {
        this.activity = activity;
        this.rewardedUnitId = rewardedUnitId;
        this.interstitialUnitId = interstitialUnitId;
        this.listener = listener;
    }

    /** Consent first (a form only where the law requires one), then the SDK. */
    public void initialize() {
        activity.runOnUiThread(() -> {
            consent = UserMessagingPlatform.getConsentInformation(activity);
            ConsentRequestParameters parameters = new ConsentRequestParameters.Builder().build();
            consent.requestConsentInfoUpdate(activity, parameters,
                    () -> UserMessagingPlatform.loadAndShowConsentFormIfRequired(activity, formError -> {
                        if (formError != null)
                            listener.onEvent("consent_error", formError.getMessage());
                        startSdk();
                    }),
                    requestError -> {
                        listener.onEvent("consent_error", requestError.getMessage());
                        startSdk();
                    });
        });
    }

    public boolean isPrivacyOptionsRequired() {
        return consent != null
                && consent.getPrivacyOptionsRequirementStatus() == ConsentInformation.PrivacyOptionsRequirementStatus.REQUIRED;
    }

    public void showPrivacyOptions() {
        activity.runOnUiThread(() -> UserMessagingPlatform.showPrivacyOptionsForm(activity,
                formError -> listener.onEvent("privacy_closed", formError != null ? formError.getMessage() : "")));
    }

    public void loadRewarded() {
        activity.runOnUiThread(() -> {
            if (rewarded != null || loadingRewarded)
                return;

            loadingRewarded = true;
            RewardedAd.load(activity, rewardedUnitId, new AdRequest.Builder().build(), new RewardedAdLoadCallback() {
                @Override
                public void onAdLoaded(RewardedAd ad) {
                    loadingRewarded = false;
                    rewarded = ad;
                    listener.onEvent("rewarded_loaded", "");
                }

                @Override
                public void onAdFailedToLoad(LoadAdError error) {
                    loadingRewarded = false;
                    listener.onEvent("rewarded_failed", error.getMessage());
                }
            });
        });
    }

    public void loadInterstitial() {
        activity.runOnUiThread(() -> {
            if (interstitial != null || loadingInterstitial)
                return;

            loadingInterstitial = true;
            InterstitialAd.load(activity, interstitialUnitId, new AdRequest.Builder().build(), new InterstitialAdLoadCallback() {
                @Override
                public void onAdLoaded(InterstitialAd ad) {
                    loadingInterstitial = false;
                    interstitial = ad;
                    listener.onEvent("interstitial_loaded", "");
                }

                @Override
                public void onAdFailedToLoad(LoadAdError error) {
                    loadingInterstitial = false;
                    listener.onEvent("interstitial_failed", error.getMessage());
                }
            });
        });
    }

    /** Reports "rewarded_closed" with "earned" when the reward was granted before the ad was closed. */
    public void showRewarded() {
        activity.runOnUiThread(() -> {
            RewardedAd ad = rewarded;
            rewarded = null;
            if (ad == null) {
                listener.onEvent("rewarded_closed", "show_failed");
                return;
            }

            AtomicBoolean earned = new AtomicBoolean(false);
            ad.setFullScreenContentCallback(new FullScreenContentCallback() {
                @Override
                public void onAdDismissedFullScreenContent() {
                    listener.onEvent("rewarded_closed", earned.get() ? "earned" : "");
                    loadRewarded();
                }

                @Override
                public void onAdFailedToShowFullScreenContent(AdError error) {
                    listener.onEvent("rewarded_closed", "show_failed");
                    loadRewarded();
                }
            });
            ad.show(activity, rewardItem -> earned.set(true));
        });
    }

    public void showInterstitial() {
        activity.runOnUiThread(() -> {
            InterstitialAd ad = interstitial;
            interstitial = null;
            if (ad == null) {
                listener.onEvent("interstitial_closed", "show_failed");
                return;
            }

            ad.setFullScreenContentCallback(new FullScreenContentCallback() {
                @Override
                public void onAdDismissedFullScreenContent() {
                    listener.onEvent("interstitial_closed", "");
                    loadInterstitial();
                }

                @Override
                public void onAdFailedToShowFullScreenContent(AdError error) {
                    listener.onEvent("interstitial_closed", "show_failed");
                    loadInterstitial();
                }
            });
            ad.show(activity);
        });
    }

    private void startSdk() {
        if (!consent.canRequestAds()) {
            listener.onEvent("init", "no_consent");
            return;
        }

        if (!sdkStarted.compareAndSet(false, true))
            return;

        // Initialisation does disk and network work, so it runs off the UI thread, as Google recommends.
        new Thread(() -> MobileAds.initialize(activity, status -> activity.runOnUiThread(() -> {
            listener.onEvent("init", "ok");
            loadRewarded();
            loadInterstitial();
        }))).start();
    }
}
