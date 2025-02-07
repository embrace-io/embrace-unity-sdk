using System;
using System.Collections;
using System.Collections.Generic;
using EmbraceSDK;
using UnityEngine;
using Random = UnityEngine.Random;

public class BinaryBlobBasic : MonoBehaviour
{
    private sbyte[] GenerateSbyteBlob(int size)
    {
        var blob = new sbyte[size];
        for (int i = 0; i < size; i++)
            blob[i] = (sbyte) Random.Range(0, int.MaxValue);
        return blob;
    }

    private byte[] GenerateByteBlob(int size)
    {
        var blob = new byte[size];
        for (int i = 0; i < size; i++)
            blob[i] = (byte) Random.Range(0, int.MaxValue);
        return blob;
    }

    IEnumerator Start()
    {
        yield return new WaitForSeconds(0.5f);
        #if UNITY_ANDROID
        var blob = GenerateSbyteBlob(1024 * 1024); // 1MiB
        #elif UNITY_IOS
        var blob = GenerateByteBlob(1024 * 1024); // 1MiB
        #endif
        EmbraceSDK.Embrace.Instance.LogMessage("binary blob", EMBSeverity.Info, null, blob);
        EmbraceSDK.Embrace.Instance.LogMessage("external attachment", EMBSeverity.Info, null, 
            (new Guid()).ToString(), "https://archive.org/download/sample-video-1280x-720-1mb_202102/SampleVideo_1280x720_1mb.mp4");
    }
}
