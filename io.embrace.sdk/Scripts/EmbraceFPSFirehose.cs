using System;
using System.Collections;
using System.Collections.Generic;
using EmbraceSDK;
using UnityEngine;

public class EmbraceFPSFirehose : MonoBehaviour
{
	byte[] _fpsBuffer;
	private int _samples = 5400; // TODO: Adjust size based on feedback from Embrace Remote Config; assume 3 minutes for now
	private bool _haveRunFirehose = false;

	public void InitFirehose()
	{
		_fpsBuffer = new byte[4 * _samples]; 
	}

	public bool ShouldStart()
	{
		#if DeveloperMode || UNITY_EDITOR
		return true;
		#else
		// TODO: Get result from Embrace Remote Config
		return false;
		#endif
	}
	
	void Start()
	{
		if (ShouldStart())
		{
			InitFirehose();
			StartFirehose();	
		}
	}
	
	public void StartFirehose()
	{
		if (!_haveRunFirehose)
		{
			StartCoroutine(RunFirehose());
			_haveRunFirehose = true;
		}
	}

	IEnumerator RunFirehose()
	{
		int byteIndex = 0;
		do
		{
			yield return null;
			byteIndex++; //Step the pointer
			StoreQuantizedData(byteIndex);
		} while (byteIndex < _samples);
		// Save the data and upload it to Embrace
		Embrace.Instance.LogMessage("Embrace FPS for first 3 minutes raw quantized data", 
			EMBSeverity.Info ,new Dictionary<string, string> (), _fpsBuffer);
	}

	void StoreQuantizedData(int byteIndex)
	{
		var frameTime = Time.unscaledDeltaTime;
		var fps = (int) 1f / frameTime;
		var quantizedFps = (int) Math.Clamp(fps / 5, 0, 255); // Clamp to byte range

		_fpsBuffer[byteIndex] |= (byte)quantizedFps; // Store in buffer
	}
}
