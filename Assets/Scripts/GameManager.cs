using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    
    public Text scoreText;
    public GameObject winPanel;
    public GameObject gameOverPanel;
    
    public bool isPlaying = true;
    private int score = 0;
    private int maxScore = 7;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        score = 0;
        UpdateScoreUI();
        
        if (winPanel != null) winPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    public void AddScore()
    {
        score++;
        UpdateScoreUI();
        
        if (score >= maxScore)
        {
            Win();
        }
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = score + " / " + maxScore;
        }
    }

    void Win()
    {
        isPlaying = false;
        Time.timeScale = 0f;
        
        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }
    }

    public void GameOver()
    {
        isPlaying = false;
        Time.timeScale = 0f;
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Windowsescritorio");
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("flapibird");
    }
}