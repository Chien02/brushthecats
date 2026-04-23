using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.UI;

public class CatGameManager : MonoBehaviour
{
    public static CatGameManager Instance { get; private set; }
    [Header("UI References")]
    public TextMeshProUGUI scoreText;
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverScoreText;
    private bool isGamePaused = false;

    [Header("Cat Reference")]
    public Cat cat;

    public CursorUI brush;

    public Chair chair;

    private int score = 0;

    private List<CatProfile> unlockedCats = new List<CatProfile>();
    private List<BrushInfo> unlockedBrushes = new List<BrushInfo>();
    private List<ChairInfo> unlockedChairs = new List<ChairInfo>();

    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        score = 0;
        UpdateScoreUI();
        gameOverPanel.SetActive(false);

        // Thêm vào danh sách mở khóa các vật phẩm mặc định
        if (cat.catProfile) unlockedCats.Add(cat.catProfile);
        if (brush.brushInfo) unlockedBrushes.Add(brush.brushInfo);
        if (chair.chairInfo) unlockedChairs.Add(chair.chairInfo);
    }


    #region Scoring Management
    public void AddScore()
    {
        score++;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        scoreText.text = "Điểm: " + (score / 10).ToString();
    }
    #endregion

    #region Game Management
    public void GameOver()
    {
        // Tat particle long meo thong qua object Cat
        if (cat != null && cat.hairParticles != null)
        {
            cat.hairParticles.Stop();
        }
        
        // Dung vong lap coroutine cua meo
        cat.StopAllCoroutines(); 

        gameOverScoreText.text = "Số lần chải: " + (score / 10).ToString();

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
    #endregion

    #region Cat Status
    public void UnlockCat(CatProfile profile)
    {
        if (unlockedCats.Contains(profile)) return; // Đã mở khóa rồi, không làm gì
        // Kiểm tra đủ điểm chưa
        if (score / 10 < profile.price) 
        {
            Debug.Log("Không đủ điểm để mở khóa mèo này!");
            return;
        }

        unlockedCats.Add(profile);
    }

    public bool IsCatUnlocked(CatProfile profile)
    {
        return unlockedCats.Contains(profile);
    }

    public List<CatProfile> GetUnlockedCats()
    {
        return unlockedCats;
    }

    public void ChangeCatProfile(CatProfile newProfile)
    {
        if (cat == null) return;
        cat.LoadCatProfile(newProfile);
    }
    #endregion

    #region Brush Status
    public void UnlockBrush(BrushInfo brushInfo)
    {
        if (unlockedBrushes.Contains(brushInfo)) return;
        if (score / 10 < brushInfo.price)
        {
            Debug.Log("Không đủ điểm để mở khóa lược này!");
            return;
        }
        unlockedBrushes.Add(brushInfo);
    }

    public bool IsBrushUnlocked(BrushInfo brushInfo)
    {
        return unlockedBrushes.Contains(brushInfo);
    }

    public List<BrushInfo> GetUnlockedBrushes()
    {
        return unlockedBrushes;
    }

    public void ChangeBrushInfo(BrushInfo newBrushInfo)
    {
        if (brush == null) return;
        Image brushImage = brush.GetComponentInChildren<Image>();
        brushImage.sprite = newBrushInfo.brushSprite;
    }
    #endregion

    #region Chair Status
    public void UnlockChair(ChairInfo chairInfo)
    {
        if (unlockedChairs.Contains(chairInfo)) return;
        if (score / 10 < chairInfo.price)
        {
            Debug.Log("Không đủ điểm để mở khóa lược này!");
            return;
        }
        unlockedChairs.Add(chairInfo);
    }

    public bool IsChairUnlocked(ChairInfo chairInfo)
    {
        return unlockedChairs.Contains(chairInfo);
    }

    public List<ChairInfo> GetUnlockedChairs()
    {
        return unlockedChairs;
    }

    public void ChangeChairInfo(ChairInfo newChairInfo)
    {
        if (chair == null) return;
        Image chairImage = chair.GetComponentInChildren<Image>();
        chairImage.sprite = newChairInfo.chairSprite;
    }
    #endregion

}