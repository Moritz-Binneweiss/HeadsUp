using UnityEngine;
using UnityEngine.UI;

public class ReadyScreenController : MonoBehaviour
{
    [Header("Accelerometer Settings")]
    public float tiltThreshold = 0.5f;
    public float tiltCooldown = 1f;

    [Header("Round Selection Buttons")]
    public Button[] roundButtons; // Auto-populated

    private float lastTiltTime;
    private bool isActive = false;

    private void OnEnable()
    {
        isActive = true;
        lastTiltTime = Time.time;

        // Auto-find round buttons if not assigned
        if (roundButtons == null || roundButtons.Length == 0)
        {
            FindRoundButtons();
        }

        SetupRoundButtons();
    }

    private void FindRoundButtons()
    {
        Transform roundsPanel = transform.Find("RoundsPanel");
        if (roundsPanel != null)
        {
            Button[] foundButtons = roundsPanel.GetComponentsInChildren<Button>();
            roundButtons = foundButtons;
        }
    }

    private void SetupRoundButtons()
    {
        if (roundButtons == null)
            return;

        for (int i = 0; i < roundButtons.Length; i++)
        {
            int roundCount = i + 1; // Closure for lambda
            if (roundButtons[i] != null)
            {
                roundButtons[i].onClick.RemoveAllListeners();
                roundButtons[i].onClick.AddListener(() => SetRoundCount(roundCount));
            }
        }

        // Highlight current selection
        UpdateButtonHighlight();
    }

    private void SetRoundCount(int rounds)
    {
        GameManager.Instance.totalRounds = rounds;
        UpdateButtonHighlight();
    }

    private void UpdateButtonHighlight()
    {
        if (roundButtons == null)
            return;

        for (int i = 0; i < roundButtons.Length; i++)
        {
            if (roundButtons[i] != null)
            {
                int roundCount = i + 1;
                Image buttonImage = roundButtons[i].GetComponent<Image>();

                if (buttonImage != null)
                {
                    if (roundCount == GameManager.Instance.totalRounds)
                    {
                        // Selected button - green
                        buttonImage.color = new Color(0.2f, 0.8f, 0.2f);
                    }
                    else
                    {
                        // Unselected button - blue
                        buttonImage.color = new Color(0.2f, 0.6f, 1f);
                    }
                }
            }
        }
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

        Vector3 acceleration = Input.acceleration;

        // Tilt down = Start game
        if (acceleration.y < -tiltThreshold)
        {
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
