using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement; // Them thu vien nay de dung SceneManager

public class CatGameManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI scoreText;
    public GameObject gameOverPanel;
    private bool isGamePaused = false;

    [Header("Cat Reference")]
    public Cat cat;

    private int score = 0;

    void Start()
    {
        score = 0;
        UpdateScoreUI();
        gameOverPanel.SetActive(false);
    }

    // Ham nay de public de Cat co the goi khi dang chai long
    public void AddScore()
    {
        score++;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        // UI text hien thi tieng Viet khong dau de tranh loi font
        scoreText.text = "Số lần chải: " + (score / 10).ToString(); 
    }

    // Ham nay de public de Cat goi khi chuyen sang trang thai Angry
    public void GameOver()
    {
        // Tat particle long meo thong qua object Cat
        if (cat != null && cat.hairParticles != null)
        {
            cat.hairParticles.Stop();
        }
        
        // Dung vong lap coroutine cua meo
        cat.StopAllCoroutines(); 

        gameOverPanel.SetActive(true);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    

    // Gắn hàm này vào nút "Pause" trên màn hình
    public void TogglePauseGame()
    {
        isGamePaused = !isGamePaused;

        if (isGamePaused)
        {
            // 1. Dừng thời gian vật lý của game (Particle sẽ dừng rơi)
            Time.timeScale = 0f; 
            
            // 2. Tạm dừng tiếng mèo
            if (cat.audioSource != null) cat.audioSource.Pause(); 
            
            // Hien thi Panel Pause (neu co)
            // pausePanel.SetActive(true);
        }
        else
        {
            // 1. Chạy lại thời gian bình thường
            Time.timeScale = 1f; 
            
            // 2. Phát tiếp tiếng mèo từ đoạn đang dở
            if (cat.audioSource != null) cat.audioSource.UnPause(); 
            
            // An Panel Pause
            // pausePanel.SetActive(false);
        }
    }
}