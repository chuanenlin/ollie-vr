using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ArduinoBluetoothAPI;
using UnityEngine.XR;
using System;
using UnityEngine.SceneManagement;

public class BluetoothManager : MonoBehaviour
{
    private List<GameObject> bluetoothKeyListenerObjects = new List<GameObject>();
    private GameObject bluetoothMenuCanvas;
    private Button skateboard1;
    private Button skateboard2;
    private AudioSource music;
    private GameObject spinner;

    private BluetoothHelper bt;
    public static BluetoothManager btManager;
    private List<BluetoothKeyListener> bluetoothKeyListeners = new List<BluetoothKeyListener>();

    const string skateboard1Name = "Skateboard-1";
    const string skateboard2Name = "Skateboard-2";

    const string up = "up";
    const string down = "down";
    const string left = "left";
    const string right = "right";

    private void Awake()
    {
        if (btManager != null)
        {
            Destroy(gameObject);
            gameObject.SetActive(false);
            return;
        }

        btManager = this;
        DontDestroyOnLoad(gameObject);
    }
 
   void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Initialize();
    }

    // Start is called before the first frame update
    void Initialize()
    {
        bluetoothKeyListeners.Clear();
        bluetoothKeyListenerObjects.Clear();
        bluetoothKeyListenerObjects.Add(GameObject.Find("GameManager"));
        bluetoothKeyListenerObjects.Add(GameObject.Find("Player"));
        bluetoothMenuCanvas = GameObject.Find("BluetoothMenuCanvas");
        GameObject temp = bluetoothMenuCanvas.transform.Find("SkateboardButton1").gameObject;
        skateboard1 = bluetoothMenuCanvas.transform.Find("SkateboardButton1").gameObject.GetComponent<Button>();
        skateboard2 = bluetoothMenuCanvas.transform.Find("SkateboardButton2").gameObject.GetComponent<Button>();
        spinner = bluetoothMenuCanvas.transform.Find("ProgressSpinner").gameObject;

        string audioObjectName = "";

        if (PlayerPrefs.GetInt("IsBlackniteUsed") == 1)
        {
            audioObjectName = "BlackniteAudioSource";
        }
        else if (PlayerPrefs.GetInt("IsGhostvoicesUsed") == 1)
        {
            audioObjectName = "GhostvoicesAudioSource";
        }
        else
        {
            audioObjectName = "ShelterAudioSource";
        }

        music = GameObject.Find(audioObjectName).GetComponent<AudioSource>();

        foreach (GameObject obj in bluetoothKeyListenerObjects)
        {
            BluetoothKeyListener listener = obj.GetComponent<BluetoothKeyListener>();
            if (listener != null)
            {
                bluetoothKeyListeners.Add(listener);
            }
        }

#if UNITY_EDITOR
        bluetoothMenuCanvas.SetActive(false);
        Time.timeScale = 1;
        return;
#endif

        if (bt != null && bt.isConnected()) {
            bluetoothMenuCanvas.SetActive(false);
            return;
        }

        try
        {
            if (bt == null) { 
                bt = BluetoothHelper.GetInstance();
                bt.OnConnected += onBTConnected;
                bt.OnConnectionFailed += onBTConnectionFailed;
                bt.setTerminatorBasedStream("\n");
            }

            skateboard1.onClick.AddListener(onSkateboard1Connect);
            skateboard2.onClick.AddListener(onSkateboard2Connect);

            if (!bt.isConnected())
            {
                Time.timeScale = 0;
                StartCoroutine(SwitchTo2D());
                bluetoothMenuCanvas.SetActive(true);
                music.Pause();
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.UpArrow))
        {
            foreach (BluetoothKeyListener listener in bluetoothKeyListeners)
                listener.onUp();
        } else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            foreach (BluetoothKeyListener listener in bluetoothKeyListeners)
                listener.onDown();
        } else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            foreach (BluetoothKeyListener listener in bluetoothKeyListeners)
                listener.onLeft();
        } else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            foreach (BluetoothKeyListener listener in bluetoothKeyListeners)
                listener.onRight();
        }

        if(bt != null && bt.Available)
        {
            onDataReceived();
        }
    }

    void onSkateboard1Connect()
    {
        spinner.SetActive(true);
        bt.setDeviceName(skateboard1Name);
        bt.Connect();
    }

    void onSkateboard2Connect()
    {
        spinner.SetActive(true);
        bt.setDeviceName(skateboard2Name);
        bt.Connect();
    }

    void onBTConnected()
    {
        Time.timeScale = 1;
        bluetoothMenuCanvas.SetActive(false);
        StartCoroutine(SwitchToVR());
        music.Play();
        try
        {
            bt.StartListening();
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    void onBTConnectionFailed()
    {
        spinner.SetActive(false);
    }

    void onDataReceived()
    {
        string message = bt.Read();
        Debug.Log(message);

        switch (message)
        {
            case up:
                foreach (BluetoothKeyListener listener in bluetoothKeyListeners)
                    listener.onUp();
                break;
            case down:
                foreach (BluetoothKeyListener listener in bluetoothKeyListeners)
                    listener.onDown();
                break;
            case left:
                foreach (BluetoothKeyListener listener in bluetoothKeyListeners)
                    listener.onLeft();
                break;
            case right:
                foreach (BluetoothKeyListener listener in bluetoothKeyListeners)
                    listener.onRight();
                break;
        }
    }

    // Call via `StartCoroutine(SwitchToVR())` from your code. Or, use
    // `yield SwitchToVR()` if calling from inside another coroutine.
    IEnumerator SwitchToVR()
    {
        // Device names are lowercase, as returned by `XRSettings.supportedDevices`.
        string desiredDevice = "cardboard"; // Or "cardboard".

        // Some VR Devices do not support reloading when already active, see
        // https://docs.unity3d.com/ScriptReference/XR.XRSettings.LoadDeviceByName.html
        if (String.Compare(XRSettings.loadedDeviceName, desiredDevice, true) != 0)
        {
            XRSettings.LoadDeviceByName(desiredDevice);

            // Must wait one frame after calling `XRSettings.LoadDeviceByName()`.
            yield return null;
        }

        // Now it's ok to enable VR mode.
        XRSettings.enabled = true;
    }

    IEnumerator SwitchTo2D()
    {
        // Empty string loads the "None" device.
        XRSettings.LoadDeviceByName("");

        // Must wait one frame after calling `XRSettings.LoadDeviceByName()`.
        yield return null;

        // Not needed, since loading the None (`""`) device takes care of this.
        // XRSettings.enabled = false;

        // Restore 2D camera settings.
        ResetCameras();
    }

    // Resets camera transform and settings on all enabled eye cameras.
    void ResetCameras()
    {
        // Camera looping logic copied from GvrEditorEmulator.cs
        for (int i = 0; i < Camera.allCameras.Length; i++)
        {
            Camera cam = Camera.allCameras[i];
            if (cam.enabled && cam.stereoTargetEye != StereoTargetEyeMask.None)
            {

                // Reset local position.
                // Only required if you change the camera's local position while in 2D mode.
                cam.transform.localPosition = Vector3.zero;

                // Reset local rotation.
                // Only required if you change the camera's local rotation while in 2D mode.
                cam.transform.localRotation = Quaternion.identity;

                // No longer needed, see issue github.com/googlevr/gvr-unity-sdk/issues/628.
                // cam.ResetAspect();

                // No need to reset `fieldOfView`, since it's reset automatically.
            }
        }
    }

    public interface BluetoothKeyListener {
        void onUp();
        void onDown();
        void onLeft();
        void onRight();
    }
}
