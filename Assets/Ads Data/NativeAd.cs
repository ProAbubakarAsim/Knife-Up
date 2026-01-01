using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NativeAd : MonoBehaviour
{
    ApplovinManager _ads;
    // Start is called before the first frame update
    void Start()
    {
        _ads = GameObject.FindObjectOfType<ApplovinManager>();
        _ads.RawImg = this.gameObject;
        _ads.RequestNativeAd();
    }

}
