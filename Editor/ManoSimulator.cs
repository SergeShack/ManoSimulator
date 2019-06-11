using ManoMotion.Example.InteractionPoints;
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

	static Sprite NoHand, OpenHand, ClosedHand, OpenPinch, ClosedPinch, Pointer;
	static Sprite ClickTrigger, DropTrigger, GrabTrigger, ReleaseTrigger;
	static Image HandIcon;
	static Text HandSideText;
	static Sprite Cursor;

	static bool clickIsInProgress;
	static bool initialized;
	public static void ProcessFrame(ref HandInfo handInfo, ref Session session)
	{
		if (!initialized)
		{
			initialized = true;

			var manoCanvas = GameObject.Find("ManoMotionCanvas");


            var cursor = GameObject.Find("cursor")?.GetComponent<Image>();
            if (cursor == null)
            {
                var cursorObj = new GameObject("cursor", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                cursorObj.transform.parent = manoCanvas.transform;
                cursorObj.GetComponent<RectTransform>().sizeDelta = new Vector2(30, 30);
                cursor = cursorObj.GetComponent<Image>();
                Cursor = AssetDatabase.LoadAssetAtPath("Assets/Resources/ManoSimulator/Cursor.png", typeof(Sprite)) as Sprite;
                cursor.sprite = Cursor;
            }

            cursor.color = Color.red;


            if (manoCanvas.GetComponent<InteractionPointsExample>() == null)
			{
				manoCanvas.AddComponent<InteractionPointsExample>();
				var cursorMover = manoCanvas.GetComponent<InteractionPointsExample>();
				cursorMover.cursor = cursor.gameObject;
				cursorMover.cursorRectTransform = cursor.gameObject.GetComponent<RectTransform>();
				cursorMover.currentInteractionPoint = InteractionPointsExample.InteractionPoint.Center;
			}

			HandIcon = GameObject.Find("HandIcon")?.GetComponent<Image>();
			if (HandIcon == null)
			{
				var handIconGO = Object.Instantiate(cursor, cursor.transform);
				handIconGO.name = "HandIcon";
				handIconGO.GetComponent<RectTransform>().anchoredPosition = new Vector2(93f, 0);
				handIconGO.transform.localScale = Vector3.one * 4f;
				HandIcon = handIconGO.GetComponent<Image>();

				var handSideGO = new GameObject("HandSide");
				handSideGO.transform.parent = cursor.transform;
				handSideGO.AddComponent<RectTransform>();
				handSideGO.GetComponent<RectTransform>().anchoredPosition = new Vector2(115f, -70f);
				handSideGO.GetComponent<RectTransform>().sizeDelta = new Vector2(200f, 100f);
				HandSideText = handSideGO.AddComponent<Text>();
				HandSideText.font = AssetDatabase.LoadAssetAtPath("Assets/Manomotion/Fonts/Ignis/Ignis et Glacies Sharp.ttf", typeof(Font)) as Font;
				HandSideText.fontSize = 40;
				HandSideText.color = Color.red;
			}

			HandIcon.color = Color.red;

			Debug.Log("HandIcon " + HandIcon);

			handInfo.tracking_info.depth_estimation = 0.33f;
			handInfo.gesture_info.hand_side = HandSide.Backside;
			HandSideText.text = "Back";

			NoHand = AssetDatabase.LoadAssetAtPath("Assets/Resources/ManoSimulator/NoHand.png", typeof(Sprite)) as Sprite;
			OpenHand = AssetDatabase.LoadAssetAtPath("Assets/Resources/ManoSimulator/OpenHand.png", typeof(Sprite)) as Sprite;
			ClosedHand = AssetDatabase.LoadAssetAtPath("Assets/Resources/ManoSimulator/ClosedHand.png", typeof(Sprite)) as Sprite;
			OpenPinch = AssetDatabase.LoadAssetAtPath("Assets/Resources/ManoSimulator/OpenPinch.png", typeof(Sprite)) as Sprite;
			ClosedPinch = AssetDatabase.LoadAssetAtPath("Assets/Resources/ManoSimulator/ClosedPinch.png", typeof(Sprite)) as Sprite;
			Pointer = AssetDatabase.LoadAssetAtPath("Assets/Resources/ManoSimulator/Pointer.png", typeof(Sprite)) as Sprite;

			ClickTrigger = AssetDatabase.LoadAssetAtPath("Assets/Resources/ManoSimulator/ClickTrigger.png", typeof(Sprite)) as Sprite;
			DropTrigger = AssetDatabase.LoadAssetAtPath("Assets/Resources/ManoSimulator/DropTrigger.png", typeof(Sprite)) as Sprite;
			GrabTrigger = AssetDatabase.LoadAssetAtPath("Assets/Resources/ManoSimulator/GrabTrigger.png", typeof(Sprite)) as Sprite;
			ReleaseTrigger = AssetDatabase.LoadAssetAtPath("Assets/Resources/ManoSimulator/ReleaseTrigger.png", typeof(Sprite)) as Sprite;
		}
		handInfo.tracking_info.palm_center = Camera.main.ScreenToViewportPoint(Input.mousePosition);
		handInfo.tracking_info.poi = Camera.main.ScreenToViewportPoint(Input.mousePosition);

		handInfo.tracking_info.depth_estimation += Input.mouseScrollDelta.y * 0.1f;
		handInfo.tracking_info.depth_estimation = Mathf.Clamp(handInfo.tracking_info.depth_estimation, 0f, 1f);

		handInfo.warning = Warning.NO_WARNING;

		// Clear trigger
		handInfo.gesture_info.mano_gesture_trigger = ManoGestureTrigger.NO_GESTURE;

		if (Input.GetKeyUp (KeyCode.Slash))
		{
			handInfo.gesture_info.hand_side = handInfo.gesture_info.hand_side == HandSide.Backside ? HandSide.Palmside : HandSide.Backside;

			HandIcon.transform.localScale = new Vector3 (-HandIcon.transform.localScale.x, HandIcon.transform.localScale.y, HandIcon.transform.localScale.z);
			HandSideText.text = handInfo.gesture_info.hand_side == HandSide.Backside ? "Back" : "Palm";
		}

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

			if (Input.GetKey(KeyCode.LeftControl))
				handInfo.gesture_info.mano_gesture_trigger = ManoGestureTrigger.CLICK;

			//Debug.Log("Pointer - CLOSED");
			LogCurrentState();
			return;
		}

		LogCurrentState();
	}

	static void LogCurrentState ()
	{
		//Debug.Log($"Palm center: {ManomotionManager.Instance.Hand_infos[0].hand_info.tracking_info.palm_center} Depth: {ManomotionManager.Instance.Hand_infos[0].hand_info.tracking_info.depth_estimation}\nMano class: {ManomotionManager.Instance.Hand_infos[0].hand_info.gesture_info.mano_class} side: {ManomotionManager.Instance.Hand_infos[0].hand_info.gesture_info.hand_side} state: {ManomotionManager.Instance.Hand_infos[0].hand_info.gesture_info.state} trigger: {ManomotionManager.Instance.Hand_infos[0].hand_info.gesture_info.mano_gesture_trigger}");

		if (HandIcon == null)
			return;

		switch (ManomotionManager.Instance.Hand_infos[0].hand_info.gesture_info.mano_class)
		{
			case ManoClass.NO_HAND:
				HandIcon.sprite = NoHand;
				break;
			case ManoClass.GRAB_GESTURE_FAMILY:
				HandIcon.sprite = ManomotionManager.Instance.Hand_infos[0].hand_info.gesture_info.state == 0 ? OpenHand : ClosedHand;
				break;
			case ManoClass.PINCH_GESTURE_FAMILY:
				HandIcon.sprite = ManomotionManager.Instance.Hand_infos[0].hand_info.gesture_info.state == 0 ? OpenPinch : ClosedPinch;
				break;
			case ManoClass.POINTER_GESTURE_FAMILY:
				HandIcon.sprite = Pointer;
				break;
			default:
				break;
		}

		switch (ManomotionManager.Instance.Hand_infos[0].hand_info.gesture_info.mano_gesture_trigger)
		{
			case ManoGestureTrigger.NO_GESTURE:
				break;
			case ManoGestureTrigger.CLICK:
				HandIcon.sprite = ClickTrigger;
				break;
			case ManoGestureTrigger.GRAB_GESTURE:
				HandIcon.sprite = GrabTrigger;
				break;
			case ManoGestureTrigger.DROP:
				HandIcon.sprite = DropTrigger;
				break;
			case ManoGestureTrigger.PICK:
				// TODO
				HandIcon.sprite = ClickTrigger;
				break;
			case ManoGestureTrigger.RELEASE_GESTURE:
				HandIcon.sprite = ReleaseTrigger;
				break;
			default:
				break;
		}
	}
}
