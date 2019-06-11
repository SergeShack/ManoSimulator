using UnityEngine;
using System;
using System.IO;

public class InputManager : MonoBehaviour
{
    #region constants
    int rezMax;
    int rezMin;
    #endregion


    #region events
    public static Action<ManoMotionFrame> OnFrameUpdated;
    public static Action<ManoMotionFrame> OnFrameInitialized;
    public static Action<ManoMotionFrame> OnFrameResized;

    public static Action<DeviceOrientation> OnOrientationChanged;
    #endregion

    public enum InputType
    {
        FrameClone,
        DeviceCamera
    }
    static InputType inputSource;

    Camera camClone;

    WebCamTexture _mobileDeviceCamera;

    public ManoMotionFrame currentFrame;


    public void SetInput(InputType newInput)
    {
        rezMax = Math.Max(Screen.width, Screen.height) / 4;
        rezMin = Math.Min(Screen.width, Screen.height) / 4;

        inputSource = newInput;
        InitializeFrame();
        switch (inputSource)
        {

            case InputType.FrameClone:
                camClone = GameObject.Find("CameraClone").GetComponent<Camera>();

                break;
            case InputType.DeviceCamera:
                ManoUtils.Instance.DetectCameraPermisions();
                InitializeDeviceCamera();
                break;

        }
        ResizeFrame();
        ResizeInput();

        currentFrame.orientation = DeviceOrientation.Unknown;

        if (OnFrameInitialized != null)
        {
            OnFrameInitialized(currentFrame);
        }
    }

    void InitializeDeviceCamera()
    {
        //Check that I do have a camera.
        if (WebCamTexture.devices.Length == 0)
        {
            //there is no camera in the device
            return;
        }

        _mobileDeviceCamera = new WebCamTexture(WebCamTexture.devices[0].name, rezMax, rezMin);
        _mobileDeviceCamera.requestedFPS = 60;
        if (!_mobileDeviceCamera.isPlaying)
        {
            _mobileDeviceCamera.Play();
        }
    }

    /// <summary>
    /// Initializes the frame clone components.
    /// </summary>
    void InitializeFrame()
    {
        currentFrame = new ManoMotionFrame();
    }

    private void Start()
    {
        // Comment this out if you are not using ARCore plugin:
        CloneCamera.FinishedUpdatingShader += UpdateFrame;
    }


    /// <summary>
    /// Checks for changes on the orientation of the device.
    /// </summary>
    void DetectOrientationChange()
    {
        if (currentFrame.width == 0)
        {
            return;
        }
        if (currentFrame.orientation != Input.deviceOrientation)
        {
            currentFrame.orientation = Input.deviceOrientation;
            ResizeFrame();
            ResizeInput();

            if (OnOrientationChanged != null)
            {
                OnOrientationChanged(currentFrame.orientation);

            }
            Debug.Log("Orientation changed");
        }

    }
    private void Update()
    {
		DetectOrientationChange();
#if UNITY_EDITOR
		if (currentFrame.width > 0)
			OnFrameUpdated?.Invoke(currentFrame);
#endif
	}

	void ResizeFrame()
    {
        switch (currentFrame.orientation)
        {
            case DeviceOrientation.Unknown:
                currentFrame.width = rezMin;
                currentFrame.height = rezMax;
                break;
            case DeviceOrientation.Portrait:
                currentFrame.width = rezMin;
                currentFrame.height = rezMax;
                break;
            case DeviceOrientation.PortraitUpsideDown:
                currentFrame.width = rezMin;
                currentFrame.height = rezMax;
                break;
            case DeviceOrientation.LandscapeLeft:
                currentFrame.width = rezMax;
                currentFrame.height = rezMin;
                break;
            case DeviceOrientation.LandscapeRight:
                currentFrame.width = rezMax;
                currentFrame.height = rezMin;
                break;
            case DeviceOrientation.FaceUp:
                break;
            case DeviceOrientation.FaceDown:
                break;
            default:
                break;
        }

        if (OnFrameResized != null)
        {
            Debug.Log("On Frame Resize");
            OnFrameResized(currentFrame);
        }
    }

    void ResizeInput()
    {
        switch (inputSource)
        {

            case InputType.FrameClone:
                ResizeFrameClone();
                break;
            case InputType.DeviceCamera:

                ResizeDeviceCamera();
                break;
            default:
                break;
        }
    }

    void ResizeFrameClone()
    {

        if (camClone.targetTexture)
        {
            camClone.targetTexture.Release();
        }

        camClone.targetTexture = new RenderTexture(currentFrame.width, currentFrame.height, 0, RenderTextureFormat.ARGB32);
        currentFrame.texture = new Texture2D(currentFrame.width, currentFrame.height);
        currentFrame.pixels = new Color32[currentFrame.width * currentFrame.height];

    }

    void ResizeDeviceCamera()
    {
#if UNITY_EDITOR
        _mobileDeviceCamera = new WebCamTexture(currentFrame.width, currentFrame.height);
        currentFrame.texture = new Texture2D(currentFrame.width, currentFrame.height);
        currentFrame.pixels = new Color32[currentFrame.width * currentFrame.height];
        return;
#endif
        if (_mobileDeviceCamera)
        {
            _mobileDeviceCamera.width = currentFrame.width;
            _mobileDeviceCamera.height = currentFrame.height;
        }
        else
        {
            Debug.LogError("I dont have a mobile device camera");
        }
    }

    void UpdateFrame()
    {
        if (currentFrame.width == 0)
        {
            return;
        }

        switch (inputSource)
        {
            case InputType.FrameClone:
                RenderTexture.active = camClone.targetTexture;
                currentFrame.texture.ReadPixels(new Rect(0, 0, RenderTexture.active.width, RenderTexture.active.height), 0, 0);
                currentFrame.texture.Apply();
                currentFrame.pixels = currentFrame.texture.GetPixels32();
                break;
            case InputType.DeviceCamera:
                currentFrame.pixels = _mobileDeviceCamera.GetPixels32();
                break;
            default:
                break;
        }

        if (OnFrameUpdated != null)
        {
            OnFrameUpdated(currentFrame);
            //Debug.LogFormat("ARUnity width {0} height{1} orientation{2}", currentFrame.width, currentFrame.height, currentFrame.orientation);
        }
    }

}

public struct ManoMotionFrame
{
    public int width;
    public int height;
    public Texture2D texture;
    public Color32[] pixels;
    public DeviceOrientation orientation;
}
