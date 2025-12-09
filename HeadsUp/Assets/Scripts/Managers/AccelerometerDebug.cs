using TMPro;
using UnityEngine;

/// <summary>
/// Zeigt Accelerometer-Werte auf dem Screen für Debugging
/// Attach to any GameObject and assign a TextMeshProUGUI
/// </summary>
public class AccelerometerDebug : MonoBehaviour
{
    [Header("Debug Display")]
    public TextMeshProUGUI debugText;
    public bool showDebug = true;

    private void Start()
    {
        // FORCE enable gyroscope (this often enables accelerometer too)
        Input.gyro.enabled = true;

        // Wait a frame and check again
        StartCoroutine(CheckSensors());
    }

    private System.Collections.IEnumerator CheckSensors()
    {
        yield return new WaitForSeconds(0.5f);

        Debug.Log($"Gyro supported: {SystemInfo.supportsGyroscope}");
        Debug.Log($"Gyro enabled: {Input.gyro.enabled}");
        Debug.Log($"Accelerometer supported: {SystemInfo.supportsAccelerometer}");

        if (!SystemInfo.supportsGyroscope)
        {
            Debug.LogWarning("Gyroscope not supported - trying accelerometer only");
        }

        if (!SystemInfo.supportsAccelerometer)
        {
            Debug.LogError("Accelerometer not supported on this device!");
            if (debugText != null)
            {
                debugText.text = "ACCELEROMETER NOT SUPPORTED!";
            }
        }
        else
        {
            Debug.Log("Accelerometer is supported!");

            // Force a read to initialize
            Vector3 testRead = Input.acceleration;
            Debug.Log($"Initial acceleration: {testRead}");
        }
    }

    private void Update()
    {
        if (!showDebug || debugText == null)
            return;

        // Try Unity's system first, fallback to Android native
        Vector3 acc = Input.acceleration;
        Vector3 gravity = Input.gyro.enabled ? Input.gyro.gravity : Vector3.zero;

        // Check if we should use native Android
        bool useNative = acc.magnitude < 0.1f && AndroidAccelerometer.Instance != null;
        if (useNative)
        {
            acc = AndroidAccelerometer.Instance.acceleration;
        }

        // Test all axes to see which one responds
        debugText.text =
            $"=== SENSOR STATUS ===\n"
            + $"Unity Gyro: {Input.gyro.enabled}\n"
            + $"Unity Accel: {acc.magnitude > 0.1f}\n"
            + $"Native Accel: {(AndroidAccelerometer.Instance?.isWorking ?? false)}\n"
            + $"Using: {(useNative ? "NATIVE" : "UNITY")}\n\n"
            + $"=== ACCELERATION ===\n"
            + $"X: {acc.x:F3} {GetBar(acc.x)}\n"
            + $"Y: {acc.y:F3} {GetBar(acc.y)}\n"
            + $"Z: {acc.z:F3} {GetBar(acc.z)}\n"
            + $"Mag: {acc.magnitude:F3}\n\n"
            + $"Screen: {Screen.orientation}\n"
            + $"Frame: {Time.frameCount}";
    }

    private string GetBar(float value)
    {
        int bars = Mathf.RoundToInt(value * 10);
        string result = "";

        if (bars > 0)
        {
            result = new string('>', bars) + "|" + new string('.', 10 - bars);
        }
        else if (bars < 0)
        {
            result = new string('.', 10 + bars) + "|" + new string('<', -bars);
        }
        else
        {
            result = new string('.', 10) + "|";
        }

        return $"{value:+0.00;-0.00} [{result}]";
    } // Toggle mit Taste T im Editor

    private void OnGUI()
    {
        if (!Application.isMobilePlatform && Event.current.type == EventType.KeyDown)
        {
            if (Event.current.keyCode == KeyCode.T)
            {
                showDebug = !showDebug;
            }
        }
    }
}
