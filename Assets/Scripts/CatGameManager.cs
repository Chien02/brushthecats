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
    public TextMeshProUGUI highScoreText;
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverScoreText;
    public NotificationManager notificationManager;
    public ScoreManager scoreManager;
    private bool isGamePaused = false;

    [Header("Cat Reference")]
    public Cat cat;
    public Brush brush;
    public Chair chair;

    private int score = 0;
    private int highScore = 0;

    [Header ("Profile database")]
    public ListManager listManager;
    // Đổi từ List<string> sang Dictionary để quản lý trạng thái
    private Dictionary<string, ItemState> catStates = new Dictionary<string, ItemState>();
    private Dictionary<string, ItemState> brushStates = new Dictionary<string, ItemState>();
    private Dictionary<string, ItemState> chairStates = new Dictionary<string, ItemState>();

    private string saveKey = "CatGameSaveData"; 

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
        LoadHighScore();
        gameOverPanel.SetActive(false);

        LoadGame();

        // Khởi tạo trạng thái mặc định (InUse) cho vật phẩm ban đầu nếu chưa có trong file save
        if (cat.catProfile && !catStates.ContainsKey(cat.catProfile.catName)) 
            catStates[cat.catProfile.catName] = ItemState.InUse;
            
        if (brush.brushInfo && !brushStates.ContainsKey(brush.brushInfo.brushName)) 
            brushStates[brush.brushInfo.brushName] = ItemState.InUse;
            
        if (chair.chairInfo && !chairStates.ContainsKey(chair.chairInfo.chairName)) 
            chairStates[chair.chairInfo.chairName] = ItemState.InUse;

        SaveGame(); 
    }

    #region Save/Load System
    public void SaveGame()
    {
        GameSaveData data = new GameSaveData();
        // Chuyển Dictionary thành List để JsonUtility có thể lưu được
        data.catData = DictToList(catStates);
        data.brushData = DictToList(brushStates);
        data.chairData = DictToList(chairStates);

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(saveKey, json);
        PlayerPrefs.Save(); 
        
        Debug.Log($"Lưu {json}");
    }

    public void LoadGame()
    {
        if (!PlayerPrefs.HasKey(saveKey)) return;
        string json = PlayerPrefs.GetString(saveKey);
        GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);

        // Chuyển ngược từ List về Dictionary khi nạp dữ liệu
        catStates = ListToDict(data.catData);
        brushStates = ListToDict(data.brushData);
        chairStates = ListToDict(data.chairData);

        Debug.Log("Tải dữ liệu Dictionary thành công!");

        // Cập nhật lại cat, brush, chair cho phù hợp - Lấy InUse
        CatProfile loadedCatProfie = null;
        BrushInfo loadedBrushInfo = null;
        ChairInfo loadedChairInfo = null;

        foreach (var state in catStates)
        {
            if (state.Value != ItemState.InUse) continue;
            loadedCatProfie = listManager.GetCatProfile(state.Key);
        }

        foreach (var state in brushStates)
        {
            if (state.Value != ItemState.InUse) continue;
            loadedBrushInfo = listManager.GetBrushInfo(state.Key);
        }

        foreach (var state in chairStates)
        {
            if (state.Value != ItemState.InUse) continue;
            loadedChairInfo = listManager.GetChairInfo(state.Key);
        }

        cat.LoadCatProfile(loadedCatProfie);
        brush.LoadBrushInfo(loadedBrushInfo);
        chair.LoadChairInfo(loadedChairInfo);
    }

    // --- Hàm hỗ trợ chuyển đổi Dictionary <-> List cho Json ---
    private List<ItemSaveData> DictToList(Dictionary<string, ItemState> dict)
    {
        List<ItemSaveData> list = new List<ItemSaveData>();
        foreach (var kvp in dict)
        {
            list.Add(new ItemSaveData { itemID = kvp.Key, state = kvp.Value });
        }
        return list;
    }

    private Dictionary<string, ItemState> ListToDict(List<ItemSaveData> list)
    {
        Dictionary<string, ItemState> dict = new Dictionary<string, ItemState>();
        if (list != null)
        {
            foreach (var item in list)
            {
                dict[item.itemID] = item.state;
            }
        }
        return dict;
    }
    #endregion

    #region Scoring Management
    public void AddScore()
    {
        score++;
        UpdateScoreUI();
        UpdateHighScore();
    }

    private void LoadHighScore()
    {
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        highScoreText.text = "Điểm cao nhất: " + (highScore / 10).ToString();
    }

    private void UpdateHighScore()
    {
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore);
            TrackMilestone();
            highScoreText.text = "Điểm cao nhất: " + (highScore / 10).ToString();
        }
    }

    private void TrackMilestone()
    {
        ScoreMilestone nextMilestone = scoreManager.GetNextMilestone(highScore / 10);
        if (nextMilestone != null && highScore / 10 == nextMilestone.score)
        {
            if (notificationManager != null)
            {
                scoreManager.UnlockMilestone(nextMilestone);
                notificationManager.ShowNotification($"{nextMilestone.rankName}!\n Mở khóa: {nextMilestone.unlockItem}");
            }

            // Đặt trạng thái Available khi mở khóa qua mốc điểm
            switch (nextMilestone.type)
            {
                case "pet":
                    if (catStates.ContainsKey(nextMilestone.unlockItem)) break;
                    catStates[nextMilestone.unlockItem] = ItemState.Unlocked;
                    // Bật đốm đỏ tương ứng cho nút đó
                    notificationManager.buttonManager.ToggleRedDot(ListType.CatList);
                    break;
                case "brush":
                    if (brushStates.ContainsKey(nextMilestone.unlockItem)) break;
                    brushStates[nextMilestone.unlockItem] = ItemState.Unlocked;
                    notificationManager.buttonManager.ToggleRedDot(ListType.BrushList);
                    break;
                case "chair":
                    if (chairStates.ContainsKey(nextMilestone.unlockItem)) break;
                    chairStates[nextMilestone.unlockItem] = ItemState.Unlocked;
                    notificationManager.buttonManager.ToggleRedDot(ListType.ChairList);
                    break;
            }
            SaveGame();
        }
    }

    private void UpdateScoreUI()
    {
        scoreText.text = "Điểm: " + (score / 10).ToString();
    }
    #endregion

    #region Game Management
    public void GameOver()
    {
        if (cat != null && cat.hairParticles != null) cat.hairParticles.Stop();
        cat.StopAllCoroutines(); 
        gameOverScoreText.text = "Số lần chải: " + (score / 10).ToString();
        gameOverPanel.SetActive(true);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void TogglePauseGame()
    {
        isGamePaused = !isGamePaused;
        Time.timeScale = isGamePaused ? 0f : 1f;
        if (cat.audioSource != null)
        {
            if (isGamePaused) cat.audioSource.Pause();
            else cat.audioSource.UnPause();
        }
    }
    #endregion

    #region Cat Status
    public void UnlockCat(CatProfile profile)
    {
        if (catStates.ContainsKey(profile.catName)) return; 
        if (highScore / 10 < profile.price) 
        {
            Debug.Log("Không đủ điểm để mở khóa mèo này!");
            return;
        }

        // Xong thì chuyển thành Unlocked
        catStates[profile.catName] = ItemState.Unlocked;
        SaveGame(); 
    }
    // Hàm gọi từ giao diện UI để kiểm tra trực tiếp State của item
    public ItemState GetCatState(CatProfile profile)
    {
        if (profile != null && catStates.TryGetValue(profile.catName, out ItemState state))
            return state;
        return ItemState.Locked; // Nếu chưa có tên trong Dictionary thì mặc định là bị khóa
    }

    public bool IsCatUnlocked(CatProfile profile)
    {
        return GetCatState(profile) != ItemState.Locked;
    }

    public void ChangeCatProfile(CatProfile newProfile)
    {
        if (cat == null || !IsCatUnlocked(newProfile)) return;

        // Đổi con mèo cũ đang InUse về trạng thái Available
        List<string> keys = new List<string>(catStates.Keys);
        foreach (string key in keys)
        {
            if (catStates[key] == ItemState.InUse)
            {
                catStates[key] = ItemState.Available;
            }
        }

        // Gán mèo mới thành InUse
        catStates[newProfile.catName] = ItemState.InUse;
        SaveGame();

        cat.LoadCatProfile(newProfile);
    }

    public void UpdateCatState(CatProfile profile, ItemState newState)
    {
        if (profile == null) return;

        if (catStates.ContainsKey(profile.catName))
        {
            catStates[profile.catName] = newState;
            SaveGame();
        }
        else
        {
            Debug.LogWarning($"Không tìm thấy profile {profile.catName} trong catStates để cập nhật trạng thái!");
        }
    }
    #endregion

    #region Brush Status
    public void UnlockBrush(BrushInfo brushInfo)
    {
        if (brushStates.ContainsKey(brushInfo.brushName)) return;
        if (highScore / 10 < brushInfo.price) return;
        
        brushStates[brushInfo.brushName] = ItemState.Unlocked;
        SaveGame(); 
    }

    public ItemState GetBrushState(BrushInfo brushInfo)
    {
        if (brushInfo != null && brushStates.TryGetValue(brushInfo.brushName, out ItemState state))
            return state;
        return ItemState.Locked;
    }

    public bool IsBrushUnlocked(BrushInfo brushInfo)
    {
        return GetBrushState(brushInfo) != ItemState.Locked;
    }

    public void ChangeBrushInfo(BrushInfo newBrushInfo)
    {
        if (brush == null || !IsBrushUnlocked(newBrushInfo)) return;

        List<string> keys = new List<string>(brushStates.Keys);
        foreach (string key in keys)
        {
            if (brushStates[key] == ItemState.InUse)
                brushStates[key] = ItemState.Available;
        }

        brushStates[newBrushInfo.brushName] = ItemState.InUse;
        SaveGame();
        brush.brushInfo = newBrushInfo;
        brush.LoadBrushInfo();
    }

    public void UpdateBrushState(BrushInfo profile, ItemState newState)
    {
        if (profile == null) return;

        if (brushStates.ContainsKey(profile.brushName))
        {
            brushStates[profile.brushName] = newState;
            SaveGame();
        }
        else
        {
            Debug.LogWarning($"Không tìm thấy profile {profile.brushName} trong brushStates để cập nhật trạng thái!");
        }
    }
    #endregion

    #region Chair Status
    public void UnlockChair(ChairInfo chairInfo)
    {
        if (chairStates.ContainsKey(chairInfo.chairName)) return;
        if (highScore / 10 < chairInfo.price) return;
        
        chairStates[chairInfo.chairName] = ItemState.Unlocked;
        SaveGame(); 
    }

    public ItemState GetChairState(ChairInfo chairInfo)
    {
        if (chairInfo != null && chairStates.TryGetValue(chairInfo.chairName, out ItemState state))
            return state;
        return ItemState.Locked;
    }

    public bool IsChairUnlocked(ChairInfo chairInfo)
    {
        return GetChairState(chairInfo) != ItemState.Locked;
    }

    public void ChangeChairInfo(ChairInfo newChairInfo)
    {
        if (chair == null || !IsChairUnlocked(newChairInfo)) return;

        List<string> keys = new List<string>(chairStates.Keys);
        foreach (string key in keys)
        {
            if (chairStates[key] == ItemState.InUse)
                chairStates[key] = ItemState.Available;
        }

        chairStates[newChairInfo.chairName] = ItemState.InUse;
        SaveGame();

        Image chairImage = chair.GetComponentInChildren<Image>();
        chairImage.sprite = newChairInfo.chairSprite;
    }

    public void UpdateChairState(ChairInfo profile, ItemState newState)
    {
        if (profile == null) return;

        if (chairStates.ContainsKey(profile.chairName))
        {
            chairStates[profile.chairName] = newState;
            SaveGame();
        }
        else
        {
            Debug.LogWarning($"Không tìm thấy profile {profile.chairName} trong chairStates để cập nhật trạng thái!");
        }
    }
    #endregion
}

// --- CÁC CLASS & ENUM DÙNG ĐỂ LƯU TRỮ DỮ LIỆU ---

[System.Serializable]
public class ItemSaveData
{
    public string itemID;
    public ItemState state;
}

[System.Serializable]
public class GameSaveData
{
    public List<ItemSaveData> catData = new List<ItemSaveData>();
    public List<ItemSaveData> brushData = new List<ItemSaveData>();
    public List<ItemSaveData> chairData = new List<ItemSaveData>();
}