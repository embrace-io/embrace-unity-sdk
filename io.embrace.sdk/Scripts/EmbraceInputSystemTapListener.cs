using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace EmbraceSDK.Instrumentation
{
	public class EmbraceInputSystemTapListener : MonoBehaviour
	{
		[SerializeField] EventSystem _eventSystem;
		EventSystem _EventSystem
		{
			get
			{
				var retValue = _eventSystem ?? (_eventSystem = FindObjectOfType<EventSystem>());

				if (retValue != null)
					return retValue;

				throw new System.NullReferenceException("There is no EventSystem in the scene. This system requires an event system");
			}
		}

		[SerializeField] GameObject selected;

		void Update()
		{
			var sys = this._EventSystem;
			selected = sys.currentSelectedGameObject;

			if (sys.alreadySelecting)
			{
				print("We are now selecting something");
			}
		}

		void FixedUpdate()
		{
			
			if (_EventSystem.alreadySelecting)
			{
				print("We are now selecting something");
			}
		}
	}
}
