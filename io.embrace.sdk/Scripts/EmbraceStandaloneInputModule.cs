using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace EmbraceSDK.Instrumentation
{
	public class EmbraceStandaloneInputModule : StandaloneInputModule
	{
		/// <summary>
		/// This property is vital for enabling and disabling tap capture for the input module in order to prevent ending up with PII.
		/// As a default, it is false to prevent accidental PII capture.
		/// </summary>
		[Tooltip("This property is vital for enabling and disabling tap capture for the input module in order to prevent ending up with PII.\nAs a default, it is false to prevent accidental PII capture.")]
		[NonSerialized]
		public bool EmbraceTapCaptureEnabled = false;
		
		const string EMBRACE_TAP_SPAN_ID = "emb-ui-tap";
		const string EMBRACE_VIEW_NAME = "view.name";
		const string EMBRACE_TAP_COORDS = "tap.coords";

		/// <summary>
		/// This property allows you to set a custom view name provider for Embrace instrumentation.
		/// Override the default to provide a specific view name logic, such as using a custom scene name or other identifiers.
		/// </summary>
		public IEmbraceViewNameProvider EmbraceViewNameProvider { get; set; } = 
			new DefaultViewNameProvider();

		/// <summary>
		/// This property allows you to set a custom game object name provider for Embrace instrumentation.
		/// Override the default to provide a specific game object name logic, such as using a custom naming convention.
		/// </summary>
		public IEmbraceGameObjectNameProvider EmbraceGameObjectNameProvider { get; set; } =
			new DefaultGameObjectNameProvider();
		
		/// <summary>
		/// This property allows you to set a custom function to provide the tapped name for Embrace instrumentation.
		/// Override the default to customize how the tapped name is constructed, such as including additional context or formatting.
		/// </summary>
		public Func<String, String, String> TappedNameProvider { get; set; } = 
			(aViewName, aTappedName) => $"{aViewName}:{aTappedName}";
		
		public override void Process()
		{
			CaptureMouseClick(
				GetMousePointerEventData().GetButtonState(
					PointerEventData.InputButton.Left));
			
			var touchData = GetTouchPointerEventData(Input.GetTouch(0), out var pressed, out var released);
			CaptureTouch(touchData, pressed, released);
			
			base.Process();
		}

		void CaptureTouch(PointerEventData eventData, bool pressed, bool released)
		{
			if (released)
			{
				var pointerUpHandler =
					ExecuteEvents.GetEventHandler<IPointerClickHandler>(eventData.pointerCurrentRaycast.gameObject);

				if (eventData.eligibleForClick && eventData.pointerPress == pointerUpHandler)
				{
					if (EmbraceTapCaptureEnabled)
					{
						CaptureTapSpan(
							eventData,
							EmbraceGameObjectNameProvider?.GetGameObjectName(pointerUpHandler.gameObject),
							EmbraceViewNameProvider?.GetViewName());
					}
				}
			}
		}
		
		void CaptureMouseClick(ButtonState buttonState)
		{
			var leftData = GetMousePointerEventData().GetButtonState(PointerEventData.InputButton.Left);

			if (leftData.eventData.ReleasedThisFrame())
			{
				var pointerUpHandler =
					ExecuteEvents.GetEventHandler<IPointerClickHandler>(leftData.eventData.buttonData
						.pointerCurrentRaycast.gameObject);
				if (leftData.eventData.buttonData.eligibleForClick && 
				    leftData.eventData.buttonData.pointerPress == pointerUpHandler)
				{
					if (EmbraceTapCaptureEnabled)
					{
						CaptureTapSpan(
							buttonState.eventData.buttonData, 
							EmbraceGameObjectNameProvider?.GetGameObjectName(pointerUpHandler.gameObject),
							EmbraceViewNameProvider?.GetViewName());
					}
				}
			}
		}

		void CaptureTapSpan(PointerEventData pointerEventData, string tappedName, string viewName)
		{
			var timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();

			Embrace.Instance.RecordCompletedSpan(
				spanName: EMBRACE_TAP_SPAN_ID,
				startTimeMs: timestamp,
				endTimeMs: timestamp,
				attributes: new Dictionary<string, string>()
				{
					{ EMBRACE_VIEW_NAME, TappedNameProvider(viewName, tappedName) }, // This is required for the necessary context for now.
					{ EMBRACE_TAP_COORDS, $"{pointerEventData.position.x},{pointerEventData.position.y}" },
				});
		}
		
		private class DefaultViewNameProvider : IEmbraceViewNameProvider
		{
			public string GetViewName()
			{
				return SceneManager.GetActiveScene().name;
			}

			public string GetViewName(string defaultValue)
			{
				return GetViewName();
			}

			public void SetViewName(string viewName)
			{
				// No-op, as the default view name is derived from the active scene.
			}
		}
		
		private class DefaultGameObjectNameProvider : IEmbraceGameObjectNameProvider
		{
			public string GetGameObjectName(GameObject gameObject)
			{
				return gameObject.name;
			}
		}
	}
}
