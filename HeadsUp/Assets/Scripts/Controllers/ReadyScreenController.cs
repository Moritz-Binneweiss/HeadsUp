using UnityEngine;

public class ReadyScreenController : MonoBehaviour
{
    [Header("Accelerometer Settings")]
    public float tiltThreshold = 0.5f;
    public float tiltCooldown = 1f;

    private float lastTiltTime;
    private bool isActive = false;

    private void OnEnable()
    {
        isActive = true;
        lastTiltTime = Time.time;

        // FORCE enable gyroscope
        Input.gyro.enabled = true;

        Debug.Log($"ReadyScreen - Gyro enabled: {Input.gyro.enabled}");
    }

    private void OnDisable()
    {
        isActive = false;
    }

    private void Update()
    {
        if (!isActive)
            return;

        if (Time.time - lastTiltTime < tiltCooldown)
            return;

        Vector3 acc = Input.acceleration;

        // Tilt down to start (use Y-axis for standard portrait)
        if (acc.y > tiltThreshold)
        {
            Debug.Log($"Starting game! Y={acc.y:F2}");
            StartGame();
        }
    }

    private void StartGame()
    {
        isActive = false;
        UIManager.Instance.ShowGameScreen();
    }

    // For testing in Unity Editor (keyboard controls)
    private void OnGUI()
    {
        if (!Application.isMobilePlatform && isActive)
        {
            if (Event.current.type == EventType.KeyDown)
            {
                if (
                    Event.current.keyCode == KeyCode.DownArrow
                    || Event.current.keyCode == KeyCode.Space
                )
                {
                    StartGame();
                }
            }
        }
    }
}
