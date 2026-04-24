using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ScoreMilestone
{
    public int score;
    public string rankName;
    public string unlockItem;
    public string type;
    public bool isUnlocked;
}

[System.Serializable]
public class ScoreLadderData
{
    // Tên biến này phải TRÙNG KHỚP với chữ "milestones" trong file JSON
    public List<ScoreMilestone> milestones; 
}

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }
    public TextAsset jsonFile;
    private ScoreLadderData ladderData;

    void Start()
    {
        // 1. Đọc dữ liệu từ file JSON vào C#
        if (jsonFile != null)
        {
            ladderData = JsonUtility.FromJson<ScoreLadderData>(jsonFile.text);
            Debug.Log($"Đã tải thành công {ladderData.milestones.Count} mốc điểm!");
        }
        else
        {
            Debug.LogError("Không tìm thấy file JSON!");
        }
    }

    /// Trả về mốc điểm cao nhất mà người chơi hiện đang đạt được
    public ScoreMilestone GetCurrentMilestone(int currentScore)
    {
        if (ladderData == null || ladderData.milestones.Count == 0) return null;

        // 2. Duyệt ngược từ cuối danh sách (mốc cao nhất) lên đầu danh sách
        for (int i = ladderData.milestones.Count - 1; i >= 0; i--)
        {
            if (currentScore >= ladderData.milestones[i].score)
            {
                // Ngay khi điểm hiện tại lớn hơn hoặc bằng một mốc nào đó, trả về mốc đó ngay lập tức
                return ladderData.milestones[i];
            }
        }
        
        return ladderData.milestones[0]; // Trả về mốc thấp nhất nếu không có gì khớp
    }

    public List<ScoreMilestone> GetAllMilestones()
    {
        return ladderData != null ? ladderData.milestones : new List<ScoreMilestone>();
    }

    public ScoreMilestone GetNextMilestone(int currentScore)
    {
        if (ladderData == null || ladderData.milestones.Count == 0) return null;

        foreach (var milestone in ladderData.milestones)
        {
            if (milestone.score >= currentScore && !milestone.isUnlocked)
            {
                return milestone; // Trả về mốc tiếp theo mà người chơi chưa đạt được
            }
        }
        
        return null; // Nếu đã đạt hết tất cả mốc
    }

    public void UnlockMilestone(ScoreMilestone milestone)
    {
        if (milestone != null)
        {
            milestone.isUnlocked = true;
            Debug.Log($"Mốc {milestone.rankName} đã được mở khóa!");
        }
    }

    // Ví dụ cách gọi khi người chơi ghi điểm
    public void TestScore(int myScore)
    {
        ScoreMilestone currentRank = GetCurrentMilestone(myScore);
        Debug.Log($"Với {myScore} điểm, danh hiệu của bạn là: {currentRank.rankName}, Mở khóa: {currentRank.unlockItem} ({currentRank.type})");
    }
}