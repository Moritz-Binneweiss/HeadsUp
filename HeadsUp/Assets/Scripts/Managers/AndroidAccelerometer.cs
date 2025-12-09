using UnityEngine;

/// <summary>
/// Native Android Accelerometer using SensorManager
/// Works when Unity's Input.acceleration fails (Android 15+ bug)
/// </summary>
public class AndroidAccelerometer : MonoBehaviour
{
    public static AndroidAccelerometer Instance { get; private set; }

    public Vector3 acceleration { get; private set; }
    public bool isWorking { get; private set; }

    private AndroidJavaObject sensorManager;
    private AndroidJavaObject accelerometerSensor;
    private SensorListener sensorListener;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            InitializeAndroidSensor();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[AndroidAccel] Init failed: {e.Message}");
            isWorking = false;
        }
#else
        Debug.LogWarning("[AndroidAccel] Only works on Android!");
        isWorking = false;
#endif
    }

    private void InitializeAndroidSensor()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        Debug.Log("[AndroidAccel] Starting initialization...");

        // Get Unity activity
        using (
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer")
        )
        {
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>(
                "currentActivity"
            );
            Debug.Log($"[AndroidAccel] Got activity: {activity != null}");

            // Get sensor manager
            AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext");
            Debug.Log($"[AndroidAccel] Got context: {context != null}");

            sensorManager = context.Call<AndroidJavaObject>("getSystemService", "sensor");
            Debug.Log($"[AndroidAccel] Got SensorManager: {sensorManager != null}");

            // Get accelerometer (TYPE_ACCELEROMETER = 1)
            accelerometerSensor = sensorManager.Call<AndroidJavaObject>("getDefaultSensor", 1);
            Debug.Log($"[AndroidAccel] Got sensor: {accelerometerSensor != null}");

            if (accelerometerSensor != null)
            {
                // Create listener - pass 'this' directly as AndroidJavaProxy
                sensorListener = new SensorListener(this);
                Debug.Log("[AndroidAccel] Created SensorListener");

                // Register listener (SENSOR_DELAY_GAME = 1)
                bool registered = sensorManager.Call<bool>(
                    "registerListener",
                    sensorListener,
                    accelerometerSensor,
                    1
                );

                isWorking = registered;
                Debug.Log($"[AndroidAccel] ✓ Registration: {registered}");
            }
            else
            {
                Debug.LogError("[AndroidAccel] No sensor found!");
                isWorking = false;
            }
        }
#endif
    }

    public void UpdateAcceleration(float x, float y, float z)
    {
        // Android sensor coordinate system to Unity
        acceleration = new Vector3(x, y, z);
    }

    private void OnDestroy()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (sensorManager != null && sensorListener != null)
        {
            sensorManager.Call("unregisterListener", sensorListener);
            Debug.Log("[AndroidAccel] Unregistered");
        }
#endif
    }

    // Inner class for sensor events
    private class SensorListener : AndroidJavaProxy
    {
        private AndroidAccelerometer parent;
        private int callCount = 0;

        public SensorListener(AndroidAccelerometer parent)
            : base("android.hardware.SensorEventListener")
        {
            this.parent = parent;
            Debug.Log("[SensorListener] Created");
        }

        // Called when sensor values change
        public void onSensorChanged(AndroidJavaObject sensorEvent)
        {
            callCount++;
            if (callCount == 1)
            {
                Debug.Log("[SensorListener] ✓ First callback received!");
            }

            if (sensorEvent == null)
            {
                Debug.LogWarning("[SensorListener] Null event");
                return;
            }

            AndroidJavaObject values = sensorEvent.Get<AndroidJavaObject>("values");
            if (values == null)
            {
                Debug.LogWarning("[SensorListener] Null values");
                return;
            }

            float x = values.Call<float>("get", 0);
            float y = values.Call<float>("get", 1);
            float z = values.Call<float>("get", 2);

            parent.UpdateAcceleration(x, y, z);

            // Log every 2 seconds
            if (callCount % 120 == 0)
            {
                Debug.Log($"[SensorListener] X={x:F2} Y={y:F2} Z={z:F2}");
            }
        }

        // Required by interface
        public void onAccuracyChanged(AndroidJavaObject sensor, int accuracy)
        {
            Debug.Log($"[SensorListener] Accuracy changed: {accuracy}");
        }
    }
}
