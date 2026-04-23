using UnityEngine;

public class ListChairItem : ListItem
{
    private ChairInfo chairInfo;

    public void LoadProfile(ChairInfo profile)
    {
        if (profile == null) return;
        chairInfo = profile;
        itemNameText.text = chairInfo.chairName;
        itemPriceText.text = "Mở khóa: " + chairInfo.price.ToString() + " điểm";
        image.sprite = chairInfo.chairSprite; // Hiển thị hình ghế
        image.preserveAspect = true;

        // Cập nhật trạng thái nút
        UpdateItemStatus();
    }

    public void OnUnlockButtonClicked()
    {
        if (chairInfo == null) return;
        CatGameManager.Instance.UnlockChair(chairInfo);
        // Cập nhật UI
        itemPriceText.text = "Đã mở khóa!";
        UpdateItemStatus();
    }

    public void OnChangeCatButtonClicked()
    {
        if (chairInfo == null) return;
        if (!CatGameManager.Instance.IsChairUnlocked(chairInfo))
        {
            Debug.Log("Ghế này chưa được mở khóa!");
            return;
        }

        // Gọi hàm đổi lược trong CatGameManager
        CatGameManager.Instance.ChangeChairInfo(chairInfo);
    }

    public void UpdateItemStatus()
    {
        if (chairInfo == null) return;
        itemState = CatGameManager.Instance.IsChairUnlocked(chairInfo) ? ItemState.Unlocked : ItemState.Locked;
        
        if (itemState == ItemState.Locked)
        {
            LockItem();
        } else
        {
            UnlockItem();
        }
    }
}