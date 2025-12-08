using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    public float roundDuration = 60f;

    [Header("Current Game State")]
    public Category currentCategory;
    public List<Player> players = new List<Player>();
    public int currentPlayerIndex = 0;

    private List<string> usedWords = new List<string>();
    private List<string> availableWords = new List<string>();

    [Header("Round Stats")]
    public int correctAnswers = 0;
    public int skippedAnswers = 0;

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

    public void StartNewGame(Category category)
    {
        currentCategory = category;
        currentPlayerIndex = 0;

        // Reset all player scores
        foreach (var player in players)
        {
            player.score = 0;
        }

        PrepareNextRound();
    }

    public void PrepareNextRound()
    {
        correctAnswers = 0;
        skippedAnswers = 0;
        usedWords.Clear();

        // Copy all words from category
        availableWords = new List<string>(currentCategory.words);

        // Show game screen
        UIManager.Instance.ShowGameScreen();
    }

    public string GetNextWord()
    {
        if (availableWords.Count == 0)
        {
            // Refill if we ran out of words
            availableWords = new List<string>(currentCategory.words);
            availableWords = availableWords.Except(usedWords).ToList();

            if (availableWords.Count == 0)
            {
                // All words used, reset
                usedWords.Clear();
                availableWords = new List<string>(currentCategory.words);
            }
        }

        int randomIndex = Random.Range(0, availableWords.Count);
        string word = availableWords[randomIndex];

        availableWords.RemoveAt(randomIndex);
        usedWords.Add(word);

        return word;
    }

    public void MarkCorrect()
    {
        correctAnswers++;
        if (currentPlayerIndex < players.Count)
        {
            players[currentPlayerIndex].score++;
        }
    }

    public void MarkSkipped()
    {
        skippedAnswers++;
    }

    public void EndRound()
    {
        // Show results screen
        UIManager.Instance.ShowResultsScreen();
    }

    public void NextPlayer()
    {
        currentPlayerIndex++;

        if (currentPlayerIndex >= players.Count)
        {
            // Game over, show final leaderboard
            UIManager.Instance.ShowLeaderboard();
        }
        else
        {
            // Show ready screen for next player
            UIManager.Instance.ShowReadyScreen();
        }
    }

    public Player GetCurrentPlayer()
    {
        if (currentPlayerIndex < players.Count)
        {
            return players[currentPlayerIndex];
        }
        return null;
    }

    public List<Player> GetSortedPlayers()
    {
        return players.OrderByDescending(p => p.score).ToList();
    }

    public void AddPlayer(string playerName)
    {
        players.Add(new Player { name = playerName, score = 0 });
    }

    public void RemovePlayer(int index)
    {
        if (index >= 0 && index < players.Count)
        {
            players.RemoveAt(index);
        }
    }

    public void ClearPlayers()
    {
        players.Clear();
        currentPlayerIndex = 0;
    }
}
