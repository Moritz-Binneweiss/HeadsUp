using System.Collections;
using TMPro;
using UnityEngine;

public class GameplayController : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI wordText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI playerNameText;
    public GameObject gameScreen;

    [Header("Accelerometer Settings")]
    public float tiltThreshold = 0.5f;
    public float tiltCooldown = 0.5f;

    private float timeRemaining;
    private string currentWord;
    private bool isGameActive = false;
    private float lastTiltTime;
    private bool canDetectTilt = true;

    private void Start()
    {
        // Enable accelerometer if available
        if (SystemInfo.supportsAccelerometer)
        {
            Input.acceleration.ToString(); // Initialize accelerometer
        }

        // Auto-find UI references if not assigned
        if (wordText == null || timerText == null || playerNameText == null)
        {
            FindUIReferences();
        }
    }

    private void FindUIReferences()
    {
        if (gameScreen == null)
            gameScreen = GameObject.Find("GameScreen");

        if (gameScreen == null)
        {
            Debug.LogError("GameplayController: GameScreen not found!");
            return;
        }

        // Find UI elements by name
        wordText =
            wordText ?? gameScreen.transform.Find("WordText")?.GetComponent<TextMeshProUGUI>();
        timerText =
            timerText ?? gameScreen.transform.Find("TimerText")?.GetComponent<TextMeshProUGUI>();
        playerNameText =
            playerNameText
            ?? gameScreen.transform.Find("PlayerNameText")?.GetComponent<TextMeshProUGUI>();

        // Log missing references
        if (wordText == null)
            Debug.LogWarning("GameplayController: wordText not found!");
        if (timerText == null)
            Debug.LogWarning("GameplayController: timerText not found!");
        if (playerNameText == null)
            Debug.LogWarning("GameplayController: playerNameText not found!");
    }

    public void StartRound()
    {
        isGameActive = true;
        timeRemaining = GameManager.Instance.roundDuration;
        lastTiltTime = -tiltCooldown;
        canDetectTilt = true;

        // Show first word
        ShowNextWord();

        // Update player name
        Player currentPlayer = GameManager.Instance.GetCurrentRoundPlayer();
        if (currentPlayer != null && playerNameText != null)
        {
            playerNameText.text = currentPlayer.name;
        }
    }

    private void Update()
    {
        if (!isGameActive)
            return;

        // Update timer
        timeRemaining -= Time.deltaTime;
        UpdateTimerDisplay();

        if (timeRemaining <= 0)
        {
            EndRound();
            return;
        }

        // Detect phone tilt
        DetectTilt();
    }

    private void DetectTilt()
    {
        if (!canDetectTilt)
            return;
        if (Time.time - lastTiltTime < tiltCooldown)
            return;

        Vector3 acceleration = Input.acceleration;

        // Tilt down = Correct (phone tilted forward/down)
        if (acceleration.y < -tiltThreshold)
        {
            OnCorrect();
            lastTiltTime = Time.time;
        }
        // Tilt up = Skip (phone tilted backward/up)
        else if (acceleration.y > tiltThreshold)
        {
            OnSkip();
            lastTiltTime = Time.time;
        }
    }

    private void OnCorrect()
    {
        // Track word result for results screen
        UIManager.Instance.AddResultWord(currentWord, true);

        GameManager.Instance.MarkCorrect();

        // Flash background green
        UIManager.Instance.FlashGameBackground(true);

        StartCoroutine(ShowFeedbackAndNextWord("Richtig!", Color.green));
    }

    private void OnSkip()
    {
        // Track word result for results screen
        UIManager.Instance.AddResultWord(currentWord, false);

        GameManager.Instance.MarkSkipped();

        // Flash background red-grey
        UIManager.Instance.FlashGameBackground(false);

        StartCoroutine(ShowFeedbackAndNextWord("Übersprungen", Color.yellow));
    }

    private IEnumerator ShowFeedbackAndNextWord(string feedback, Color color)
    {
        canDetectTilt = false;

        // Show feedback
        Color originalColor = wordText.color;
        wordText.text = feedback;
        wordText.color = color;

        yield return new WaitForSeconds(0.3f);

        // Show next word
        wordText.color = originalColor;
        ShowNextWord();

        canDetectTilt = true;
    }

    private void ShowNextWord()
    {
        currentWord = GameManager.Instance.GetNextWord();
        wordText.text = currentWord;
    }

    private void UpdateTimerDisplay()
    {
        if (timerText == null)
            return;

        int minutes = Mathf.FloorToInt(timeRemaining / 60);
        int seconds = Mathf.FloorToInt(timeRemaining % 60);
        timerText.text = $"{minutes:00}:{seconds:00}";

        // Change color when time is running out
        timerText.color = timeRemaining <= 10 ? Color.red : Color.white;
    }

    private void EndRound()
    {
        isGameActive = false;
        GameManager.Instance.EndRound();
    }

    // For testing in Unity Editor (keyboard controls)
    private void OnGUI()
    {
        if (!Application.isMobilePlatform && isGameActive)
        {
            if (Event.current.type == EventType.KeyDown)
            {
                if (Event.current.keyCode == KeyCode.UpArrow)
                {
                    OnSkip();
                }
                else if (Event.current.keyCode == KeyCode.DownArrow)
                {
                    OnCorrect();
                }
                else if (Event.current.keyCode == KeyCode.RightArrow)
                {
                    // Developer shortcut: Skip entire round
                    Debug.Log("[DEV] Skipping round...");
                    StopAllCoroutines();
                    EndRound();
                }
            }
        }
    }
}
