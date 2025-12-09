using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Screen References")]
    public GameObject mainMenuScreen;
    public GameObject playerSetupScreen;
    public GameObject readyScreen;
    public GameObject gameScreen;
    public GameObject resultsScreen;
    public GameObject leaderboardScreen;

    [Header("Main Menu - Category Buttons")]
    public Button[] categoryButtons; // Assign in inspector (e.g., 2-3 buttons)
    public Button randomCategoryButton;

    [Header("Player Setup")]
    public TMP_InputField playerNameInput;
    public Button addPlayerButton;
    public Button removePlayerButton;
    public Button backToMenuButton;
    public Button continueToGameButton;
    public Transform playerListContainer;
    public GameObject playerListItemPrefab;
    public Button[] roundSelectionButtons; // Round selection 1-5

    [Header("Ready Screen")]
    public TextMeshProUGUI readyPlayerNameText;
    public TextMeshProUGUI readyText; // "Ready?"
    public GameObject readyPanel;

    [Header("Game Screen")]
    public GameplayController gameplayController;
    public Image gameBackgroundImage;
    public Color correctColor = Color.green;
    public Color skipColor = new Color(0.7f, 0.3f, 0.3f); // reddish-grey

    [Header("Results Screen")]
    public TextMeshProUGUI resultsScoreText; // "12 Points!"
    public Transform resultsWordsContainer;
    public GameObject resultWordItemPrefab; // Text with background color
    public Button resultsNextButton;

    [Header("Leaderboard")]
    public Transform leaderboardContainer;
    public GameObject leaderboardItemPrefab;
    public Button leaderboardNextButton;
    public Button leaderboardBackToMenuButton;

    [Header("Categories")]
    public CategoryLoader categoryLoader;
    public List<Category> availableCategories = new List<Category>();

    private Category selectedCategory;
    private List<ResultWord> currentRoundResults = new List<ResultWord>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SetupButtons();
        LoadCategories();
        SetupCategoryButtons();
        ShowMainMenu();
    }

    private void LoadCategories()
    {
        if (categoryLoader == null)
        {
            Debug.LogError("CategoryLoader is not assigned in UIManager!");
            return;
        }

        availableCategories = categoryLoader.LoadAllCategories();

        if (availableCategories.Count == 0)
        {
            Debug.LogWarning("No categories loaded! Check Resources/Categories folder.");
        }
    }

    private void SetupCategoryButtons()
    {
        if (availableCategories == null || availableCategories.Count == 0)
        {
            Debug.LogWarning("No categories available to setup buttons!");
            return;
        }

        for (int i = 0; i < categoryButtons.Length && i < availableCategories.Count; i++)
        {
            int index = i;
            Category cat = availableCategories[index];

            if (categoryButtons[i] == null)
                continue;

            categoryButtons[i].onClick.AddListener(() => SelectCategory(cat));

            // Update button text and color
            var buttonText = categoryButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
                buttonText.text = cat.categoryName;

            var buttonImage = categoryButtons[i].GetComponent<Image>();
            if (buttonImage != null)
                buttonImage.color = cat.categoryColor;
        }

        randomCategoryButton?.onClick.AddListener(SelectRandomCategory);
    }

    private void SetupButtons()
    {
        addPlayerButton?.onClick.AddListener(AddPlayer);
        removePlayerButton?.onClick.AddListener(RemoveLastPlayer);
        backToMenuButton?.onClick.AddListener(ShowMainMenu);
        continueToGameButton?.onClick.AddListener(ShowReadyScreen);
        resultsNextButton?.onClick.AddListener(OnResultsNext);
        leaderboardNextButton?.onClick.AddListener(OnLeaderboardNext);
        leaderboardBackToMenuButton?.onClick.AddListener(ShowMainMenu);

        SetupRoundSelectionButtons();
    }

    private void SetupRoundSelectionButtons()
    {
        // Auto-find round buttons if not assigned
        if (roundSelectionButtons == null || roundSelectionButtons.Length == 0)
        {
            if (playerSetupScreen != null)
            {
                Transform roundsPanel = playerSetupScreen.transform.Find("RoundsPanel");
                if (roundsPanel != null)
                {
                    roundSelectionButtons = roundsPanel.GetComponentsInChildren<Button>();
                }
            }
        }

        // Setup listeners
        if (roundSelectionButtons != null)
        {
            for (int i = 0; i < roundSelectionButtons.Length; i++)
            {
                int roundCount = i + 1;
                if (roundSelectionButtons[i] != null)
                {
                    roundSelectionButtons[i].onClick.RemoveAllListeners();
                    roundSelectionButtons[i]
                        .onClick.AddListener(() => SelectRoundCount(roundCount));
                }
            }
        }
    }

    private void SelectRoundCount(int rounds)
    {
        GameManager.Instance.totalRounds = rounds;
        UpdateRoundButtonHighlight();
    }

    private void UpdateRoundButtonHighlight()
    {
        if (roundSelectionButtons == null)
            return;

        for (int i = 0; i < roundSelectionButtons.Length; i++)
        {
            if (roundSelectionButtons[i] == null)
                continue;

            int roundCount = i + 1;
            var buttonImage = roundSelectionButtons[i].GetComponent<Image>();

            if (buttonImage != null)
            {
                buttonImage.color =
                    roundCount == GameManager.Instance.totalRounds
                        ? new Color(0.2f, 0.8f, 0.2f) // Green - selected
                        : new Color(0.2f, 0.6f, 1f); // Blue - unselected
            }
        }
    }

    private void SelectCategory(Category category)
    {
        selectedCategory = category;
        GameManager.Instance.currentCategory = category;
        ShowPlayerSetup();
    }

    private void SelectRandomCategory()
    {
        if (availableCategories != null && availableCategories.Count > 0)
        {
            int randomIndex = Random.Range(0, availableCategories.Count);
            SelectCategory(availableCategories[randomIndex]);
        }
    }

    public void ShowMainMenu()
    {
        HideAllScreens();
        if (mainMenuScreen != null)
            mainMenuScreen.SetActive(true);

        GameManager.Instance.ClearPlayers();
    }

    public void ShowPlayerSetup()
    {
        HideAllScreens();
        if (playerSetupScreen != null)
            playerSetupScreen.SetActive(true);

        UpdatePlayerList();
        UpdateRoundButtonHighlight();
    }

    public void ShowReadyScreen()
    {
        if (GameManager.Instance.players.Count == 0)
        {
            Debug.LogWarning("No players added!");
            return;
        }

        if (GameManager.Instance.currentPlayerIndex == 0)
        {
            GameManager.Instance.PrepareNextRound();
        }

        HideAllScreens();
        if (readyScreen != null)
            readyScreen.SetActive(true);

        Player currentPlayer = GameManager.Instance.GetCurrentRoundPlayer();
        if (currentPlayer != null && readyPlayerNameText != null)
        {
            int currentRound = GameManager.Instance.GetCurrentRound();
            readyPlayerNameText.text =
                $"{currentPlayer.name}\nRound {currentRound}/{GameManager.Instance.totalRounds}";
        }
    }

    public void ShowGameScreen()
    {
        HideAllScreens();
        if (gameScreen != null)
            gameScreen.SetActive(true);

        currentRoundResults.Clear();

        if (gameplayController != null)
            gameplayController.StartRound();
    }

    public void ShowResultsScreen()
    {
        HideAllScreens();
        if (resultsScreen != null)
            resultsScreen.SetActive(true);

        UpdateResultsDisplay();
    }

    public void ShowLeaderboard()
    {
        HideAllScreens();
        if (leaderboardScreen != null)
            leaderboardScreen.SetActive(true);

        UpdateLeaderboard();
    }

    private void HideAllScreens()
    {
        if (mainMenuScreen != null)
            mainMenuScreen.SetActive(false);
        if (playerSetupScreen != null)
            playerSetupScreen.SetActive(false);
        if (readyScreen != null)
            readyScreen.SetActive(false);
        if (gameScreen != null)
            gameScreen.SetActive(false);
        if (resultsScreen != null)
            resultsScreen.SetActive(false);
        if (leaderboardScreen != null)
            leaderboardScreen.SetActive(false);
    }

    private void AddPlayer()
    {
        if (playerNameInput != null && !string.IsNullOrEmpty(playerNameInput.text))
        {
            GameManager.Instance.AddPlayer(playerNameInput.text);
            playerNameInput.text = "";
            UpdatePlayerList();
        }
    }

    private void RemoveLastPlayer()
    {
        if (GameManager.Instance.players.Count > 0)
        {
            GameManager.Instance.RemovePlayer(GameManager.Instance.players.Count - 1);
            UpdatePlayerList();
        }
    }

    private void UpdatePlayerList()
    {
        foreach (Transform child in playerListContainer)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < GameManager.Instance.players.Count; i++)
        {
            GameObject itemObj = Instantiate(playerListItemPrefab, playerListContainer);
            TextMeshProUGUI itemText = itemObj.GetComponentInChildren<TextMeshProUGUI>();

            if (itemText != null)
                itemText.text = $"{i + 1}. {GameManager.Instance.players[i].name}";
        }

        if (continueToGameButton != null)
            continueToGameButton.interactable = GameManager.Instance.players.Count > 0;
    }

    public void AddResultWord(string word, bool wasCorrect)
    {
        currentRoundResults.Add(new ResultWord { word = word, wasCorrect = wasCorrect });
    }

    public void FlashGameBackground(bool correct)
    {
        if (gameBackgroundImage != null)
        {
            StartCoroutine(FlashBackgroundCoroutine(correct));
        }
    }

    private System.Collections.IEnumerator FlashBackgroundCoroutine(bool correct)
    {
        Color flashColor = correct ? correctColor : skipColor;
        Color originalColor = gameBackgroundImage.color;

        gameBackgroundImage.color = flashColor;
        yield return new WaitForSeconds(0.3f);
        gameBackgroundImage.color = originalColor;
    }

    private void UpdateResultsDisplay()
    {
        foreach (Transform child in resultsWordsContainer)
        {
            Destroy(child.gameObject);
        }

        if (resultsScoreText != null)
        {
            int score = GameManager.Instance.correctAnswers;
            resultsScoreText.text = $"{score} Points!";
        }

        foreach (ResultWord result in currentRoundResults)
        {
            GameObject itemObj = Instantiate(resultWordItemPrefab, resultsWordsContainer);
            TextMeshProUGUI itemText = itemObj.GetComponentInChildren<TextMeshProUGUI>();
            Image itemBg = itemObj.GetComponent<Image>();

            if (itemText != null)
                itemText.text = result.word;

            if (itemBg != null)
                itemBg.color = result.wasCorrect ? correctColor : skipColor;
        }
    }

    private void OnResultsNext()
    {
        ShowLeaderboard();
    }

    private void OnLeaderboardNext()
    {
        GameManager.Instance.NextPlayer();
    }

    private void UpdateLeaderboard()
    {
        foreach (Transform child in leaderboardContainer)
        {
            Destroy(child.gameObject);
        }

        List<Player> sortedPlayers = GameManager.Instance.GetSortedPlayers();

        for (int i = 0; i < sortedPlayers.Count; i++)
        {
            GameObject itemObj = Instantiate(leaderboardItemPrefab, leaderboardContainer);
            TextMeshProUGUI itemText = itemObj.GetComponentInChildren<TextMeshProUGUI>();

            if (itemText != null)
            {
                itemText.text = $"{i + 1}. {sortedPlayers[i].name} - {sortedPlayers[i].score} Pkt";
            }
        }

        bool allPlayersFinished =
            GameManager.Instance.currentPlayerIndex >= GameManager.Instance.players.Count;

        if (leaderboardNextButton != null)
            leaderboardNextButton.gameObject.SetActive(!allPlayersFinished);

        if (leaderboardBackToMenuButton != null)
            leaderboardBackToMenuButton.gameObject.SetActive(allPlayersFinished);
    }

    [System.Serializable]
    public class ResultWord
    {
        public string word;
        public bool wasCorrect;
    }
}
