using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class WebViewBasic : MonoBehaviour
{
#if UNITY_IOS
    void Start()
    {
        _embrace_basic_open_web_view("meow");
    }

    [DllImport("__Internal")]
    static extern void _embrace_basic_open_web_view(string url);
#endif
}
