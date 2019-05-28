using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ManoSimulator : MonoBehaviour
{
	[InitializeOnLoadMethod]
	static void Init ()
	{
		ManomotionManager.OnProcessFrameNeeded += ProcessFrame;
	}

	static bool clickIsInProgress;
	static bool initialized;
	public static void ProcessFrame(ref HandInfo handInfo, ref Session session)
	{
		if (!initialized)
		{
			initialized = true;
			var cursor = GameObject.Find("cursor")?.GetComponent<Image>();
			if (cursor != null)
				cursor.color = Color.red;

			handInfo.tracking_info.depth_estimation = 0.33f;
		}
		handInfo.tracking_info.palm_center = Camera.main.ScreenToViewportPoint(Input.mousePosition);

		handInfo.tracking_info.depth_estimation += Input.mouseScrollDelta.y * 0.1f;
		handInfo.tracking_info.depth_estimation = Mathf.Clamp(handInfo.tracking_info.depth_estimation, 0f, 1f);

		handInfo.warning = Warning.NO_WARNING;

		// Clear trigger
		handInfo.gesture_info.mano_gesture_trigger = ManoGestureTrigger.NO_GESTURE;

		// LMB to Pinch
		if (!clickIsInProgress && Input.GetMouseButtonDown(0))
		{
			clickIsInProgress = true;
			handInfo.gesture_info.mano_class = ManoClass.PINCH_GESTURE_FAMILY;
			handInfo.gesture_info.state = 13;
			//Debug.Log ("Pinch - PICK");

			if (Input.GetKey(KeyCode.LeftControl))
				handInfo.gesture_info.mano_gesture_trigger = ManoGestureTrigger.CLICK;
			else if (Input.GetKey(KeyCode.LeftAlt))
				handInfo.gesture_info.mano_gesture_trigger = ManoGestureTrigger.PICK;

			LogCurrentState();
			return;
		}
		if (Input.GetMouseButtonUp(0))
		{
			clickIsInProgress = false;
			handInfo.gesture_info.mano_class = ManoClass.PINCH_GESTURE_FAMILY;
			handInfo.gesture_info.state = 0;

			if (Input.GetKey(KeyCode.LeftAlt))
				handInfo.gesture_info.mano_gesture_trigger = ManoGestureTrigger.DROP;

			//Debug.Log("Pinch - DROP");
			LogCurrentState();
			return;
		}

		// RMB to Grab
		if (!clickIsInProgress && Input.GetMouseButtonDown(1))
		{
			clickIsInProgress = true;
			handInfo.gesture_info.mano_class = ManoClass.GRAB_GESTURE_FAMILY;
			handInfo.gesture_info.state = 13;

			if (Input.GetKey(KeyCode.LeftAlt))
				handInfo.gesture_info.mano_gesture_trigger = ManoGestureTrigger.GRAB_GESTURE;

			//Debug.Log("Grab - GRAB");
			LogCurrentState();
			return;
		}
		if (Input.GetMouseButtonUp(1))
		{
			clickIsInProgress = false;
			handInfo.gesture_info.mano_class = ManoClass.GRAB_GESTURE_FAMILY;
			handInfo.gesture_info.state = 0;

			if (Input.GetKey(KeyCode.LeftAlt))
				handInfo.gesture_info.mano_gesture_trigger = ManoGestureTrigger.RELEASE_GESTURE;

			//Debug.Log("Grab - RELEASE");
			LogCurrentState();
			return;
		}

		// MMB to Point
		if (!clickIsInProgress && Input.GetMouseButtonDown(2))
		{
			clickIsInProgress = true;
			handInfo.gesture_info.mano_class = ManoClass.POINTER_GESTURE_FAMILY;
			handInfo.gesture_info.state = 13;
			//Debug.Log("Pointer - OPEN");
			LogCurrentState();
			return;
		}
		if (Input.GetMouseButtonUp(2))
		{
			clickIsInProgress = false;
			handInfo.gesture_info.mano_class = ManoClass.POINTER_GESTURE_FAMILY;
			handInfo.gesture_info.state = 0;
			//Debug.Log("Pointer - CLOSED");
			LogCurrentState();
			return;
		}

		LogCurrentState();
	}

	static void LogCurrentState ()
	{
		Debug.Log($"Palm center: {ManomotionManager.Instance.Hand_infos[0].hand_info.tracking_info.palm_center} Depth: {ManomotionManager.Instance.Hand_infos[0].hand_info.tracking_info.depth_estimation}\nMano class: {ManomotionManager.Instance.Hand_infos[0].hand_info.gesture_info.mano_class} state: {ManomotionManager.Instance.Hand_infos[0].hand_info.gesture_info.state} trigger: {ManomotionManager.Instance.Hand_infos[0].hand_info.gesture_info.mano_gesture_trigger}");
	}
}
