using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class FunctionalButtonManager : MonoBehaviour
{
    // Cấu trúc trên Inspector sẽ cho phép bạn kéo thẳng Button và RedDot vào
    public List<FunctionalButton> functionalButtons = new List<FunctionalButton>();
    
    // Đổi Dictionary để lưu toàn bộ class FunctionalButton thay vì chỉ lưu Button
    public Dictionary<ListType, FunctionalButton> funcButtonDict = new Dictionary<ListType, FunctionalButton>();

    void Awake()
    {
        foreach (var btnData in functionalButtons)
        {
            // Đưa dữ liệu vào Dictionary để sau này tra cứu nhanh
            funcButtonDict[btnData.type] = btnData;
            
            // Gán sự kiện OnClick
            btnData.button.onClick.AddListener(() => ToggleRedDot(btnData.type, true));
        }
    }

    public void ToggleRedDot(ListType type, bool turnOffFlag = false)
    {
        
        // Kiểm tra xem Dictionary có chứa type này không
        if (funcButtonDict.TryGetValue(type, out FunctionalButton targetData))
        {
            // Lấy thẳng GameObject chấm đỏ ra và đảo ngược trạng thái Active
            if (targetData.redDot != null)
            {
                if (turnOffFlag)
                {
                    targetData.redDot.SetActive(false);
                    return;
                }
                bool isCurrentlyActive = targetData.redDot.activeSelf;
                targetData.redDot.SetActive(!isCurrentlyActive);
            }
            else
            {
                Debug.LogWarning($"Chưa gắn Red Dot cho nút {type} trên Inspector!");
            }
        }
    }
}

[System.Serializable]
public class FunctionalButton
{
    public ListType type;
    public Button button;
    public GameObject redDot;
}