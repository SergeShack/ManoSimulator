using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;
#if UNITY_IOS
using UnityEngine.iOS;
#endif

public class ManomotionManager : MonoBehaviour
{
    #region Singleton
    protected static ManomotionManager _instance;
    #endregion

    #region Variables
    [Tooltip("Insert the key gotten from the webpage here https://www.manomotion.com/my-account/licenses/")]
    [SerializeField]
    protected string serial_key;

    protected HandInfoUnity[] hand_infos;
    protected VisualizationInfo visualization_info;
    public Session manomotion_session;
    protected ManoLicense _manoLicense;

    protected int _frame_number;     protected int _fps;     protected int _processing_time;
    private float fpsCooldown = 0;     private int frameCount = 0;     private List<int> processing_time_list = new List<int>();

    #endregion

    #region Libary
#if UNITY_IOS
    const string library = "__Internal";
#elif UNITY_ANDROID
    const string library = "manomotion";
#else
    const string library = "manomotion";
#endif
    #endregion

    #region Libary: Process Frame
    [DllImport(library)]
    private static extern void processFrame(ref HandInfo hand_info0, ref Session manomotion_session);

	public delegate void ProcessFrameNeeded (ref HandInfo handInfo, ref Session session);
	public static ProcessFrameNeeded OnProcessFrameNeeded;

	/// <summary>
	/// Wrapper method that calls the ManoMotion core tech to process the frame in order to perform hand tracking and gesture analysis
	/// </summary>
	protected void ProcessFrame()
    {

#if !UNITY_EDITOR || UNITY_STANDALONE
 processFrame(ref hand_infos[0].hand_info, ref manomotion_session);
#elif UNITY_EDITOR
		OnProcessFrameNeeded?.Invoke(ref hand_infos[0].hand_info, ref manomotion_session);
#endif

	}
    #endregion

    #region Library: Set Frame Array
    [DllImport(library)]
    private static extern void setFrameArray(Color32[] frame);

    protected void SetFrameArray(Color32[] pixels)
    {
#if !UNITY_EDITOR
       
        setFrameArray(pixels);

#endif
    }
    #endregion

    #region Library: Set Resolution
    [DllImport(library)]
    private static extern void setResolution(int width, int height);

    protected void SetResolution(int width, int height)
    {
        Debug.Log("Set resolution " + width + "," + height);
#if !UNITY_EDITOR

        setResolution(width, height);
#endif
    }
    #endregion

    #region Library: Initialize

    [DllImport(library)]
    private static extern ManoLicense init(string serial_key);

    protected void Init(string serial_key)
    {
        if (!isInitialized)
        {
#if !UNITY_EDITOR || UNITY_STANDALONE
            _manoLicense = init(serial_key);
            isInitialized=true;
#endif
        }

    }



    #endregion

    #region Properties

    internal int Processing_time     {         get         {             return _processing_time;         }      }      internal int Fps     {         get         {             return _fps;         }     }

    internal int Frame_number     {         get         {             return _frame_number;         }     }

    public VisualizationInfo Visualization_info
    {
        get
        {
            return visualization_info;
        }
    }

    public HandInfoUnity[] Hand_infos
    {
        get
        {
            return hand_infos;
        }
    }

    public Session Manomotion_Session
    {
        get
        {
            return manomotion_session;
        }
    }

    public static ManomotionManager Instance
    {
        get
        {
            return _instance;
        }
    }

    public string Serial_key
    {
        get
        {
            return serial_key;
        }

        set
        {
            serial_key = value;
        }
    }

    public ManoLicense ManoLicense
    {
        get
        {
            return _manoLicense;
        }
        set
        {
            _manoLicense = value;
        }
    }
    #endregion

    bool isInitialized;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            InputManager.OnFrameInitialized += InitializeManoMotionManager;
            InputManager.OnFrameUpdated += ProcessManomotion;
            InputManager.OnFrameResized += ResizeToFitFrame;
        }
        else
        {
            this.gameObject.SetActive(false);
            Debug.LogWarning("More than 1 Manomotionmanager in scene");
        }

#if UNITY_EDITOR
		isInitialized = true;
#endif
	}


    #region Initialize Methods

    /// <summary>
    /// Picks the resolution.
    /// </summary>
    protected void InitializeManoMotionManager(ManoMotionFrame newFrame)
    {
        InstantiateSession(newFrame);
        InstantiateHandInfos();
        InitiateLibrary();
    }

    /// <summary>
    /// Resizes the resolution,theframe array and visualization information to fit a frame dimensions.
    /// </summary>
    /// <param name="newFrame">New frame.</param>
    void ResizeToFitFrame(ManoMotionFrame newFrame)
    {
        if (isInitialized)
        {
            SetResolution(newFrame.width, newFrame.height);
            SetFrameArray(newFrame.pixels);
            InstantiateVisualisationInfo(newFrame.width, newFrame.height);
            Debug.Log("ManoMotion Resize To Fit Frame");
        }

    }

    /// <summary>
    /// Instantiates the manager info.
    /// </summary>
    protected void InstantiateSession(ManoMotionFrame frame)
    {
        manomotion_session = new Session();
#if UNITY_ANDROID
        manomotion_session.current_plataform = Platform.UNITY_ANDROID;
#elif UNITY_IOS
        manomotion_session.current_plataform = Platform.UNITY_IOS;
#endif
        manomotion_session.image_format = ImageFormat.RGBA_IMAGE;
        manomotion_session.orientation = frame.orientation;
        manomotion_session.add_on = AddOn.DEFAULT;
        manomotion_session.smoothing_controller = 0.5f;
        manomotion_session.enabled_features.pinch_poi = 1;
        Debug.Log("Session Created");
    }

    /// <summary>
    /// Initializes the values for the hand information.
    /// </summary>
    private void InstantiateHandInfos()
    {
        hand_infos = new HandInfoUnity[1];
        for (int i = 0; i < hand_infos.Length; i++)
        {
            hand_infos[i].hand_info = new HandInfo();
            hand_infos[i].hand_info.gesture_info = new GestureInfo();
            hand_infos[i].hand_info.gesture_info.mano_class = ManoClass.NO_HAND;
            hand_infos[i].hand_info.gesture_info.hand_side = HandSide.None;
            hand_infos[i].hand_info.tracking_info = new TrackingInfo();
            hand_infos[i].hand_info.tracking_info.bounding_box = new BoundingBox();
            hand_infos[i].hand_info.tracking_info.bounding_box.top_left = new Vector3();
        }
        Debug.Log("Hands Created");
    }

    /// <summary>
    /// Instantiates the visualisation info.
    /// </summary>
    private void InstantiateVisualisationInfo(int width, int height)
    {
        visualization_info = new VisualizationInfo();
        visualization_info.rgb_image = new Texture2D(width, height);
    }

    /// <summary>
    /// Initiates the library.
    /// </summary>
    protected void InitiateLibrary()     {
        _manoLicense = new ManoLicense();

        Debug.Log("Initiating ManoMotion SDK with serial key " + serial_key + " bundle id :" + Application.identifier);
        Init(serial_key);
        Debug.Log("Initialized");
    }
    #endregion

    public static Action OnFrameManoMotionProcessed;
    protected void ProcessManomotion(ManoMotionFrame newFrame)
    {
        if (isInitialized)
        {
            visualization_info.rgb_image.SetPixels32(newFrame.pixels);
            visualization_info.rgb_image.Apply();
            CalculateFPSAndProcessingTime();


            try
            {
                long start = System.DateTime.UtcNow.Millisecond + System.DateTime.UtcNow.Second * 1000 + System.DateTime.UtcNow.Minute * 60000;
                ProcessFrame();
                long end = System.DateTime.UtcNow.Millisecond + System.DateTime.UtcNow.Second * 1000 + System.DateTime.UtcNow.Minute * 60000;
                if (start < end)
                    processing_time_list.Add((int)(end - start));

            }
            catch (System.Exception ex)
            {
                Debug.Log("exeption: " + ex.ToString());

            }

            if (OnFrameManoMotionProcessed != null)
            {
                OnFrameManoMotionProcessed();
            }
        }

    }



    /// <summary>     /// Calculates the Frames Per Second in the application and retrieves the estimated Processing time.     /// </summary>     protected void CalculateFPSAndProcessingTime()     {         fpsCooldown += Time.deltaTime;         frameCount++;         if (fpsCooldown >= 1)         {             _fps = frameCount;             frameCount = 0;             fpsCooldown -= 1;             CalculateProcessingTime();         }     }

    /// <summary>     /// Calculates the elapses time needed for processing the frame.     /// </summary>     protected void CalculateProcessingTime()     {         if (processing_time_list.Count > 0)         {             int sum = 0;             for (int i = 0; i < processing_time_list.Count; i++)             {                 sum += processing_time_list[i];             }             sum /= processing_time_list.Count;             _processing_time = sum;             processing_time_list.Clear();         }     }




}
