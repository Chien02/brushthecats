using UnityEngine;

public class ListBrushItem : ListItem
{
    private BrushInfo brushInfo;

    public void LoadProfile(BrushInfo profile)
    {
        if (profile == null) return;
        brushInfo = profile;
        itemNameText.text = brushInfo.brushName;
        itemPriceText.text = "Mở khóa: " + brushInfo.price.ToString() + " điểm";
        image.sprite = brushInfo.brushSprite; // Hiển thị hình mèo ở trạng thái Away làm mặc định
        image.preserveAspect = true;

        // Cập nhật trạng thái nút
        UpdateItemStatus();
    }

    public void OnUnlockButtonClicked()
    {
        if (brushInfo == null) return;
        CatGameManager.Instance.UnlockBrush(brushInfo);
        // Cập nhật UI
        itemPriceText.text = "Đã mở khóa!";
        UpdateItemStatus();
    }

    public void OnChangeCatButtonClicked()
    {
        if (brushInfo == null) return;
        if (!CatGameManager.Instance.IsBrushUnlocked(brushInfo))
        {
            Debug.Log("Lược này chưa được mở khóa!");
            return;
        }

        // Gọi hàm đổi lược trong CatGameManager
        CatGameManager.Instance.ChangeBrushInfo(brushInfo);
    }

    public void UpdateItemStatus()
    {
        if (brushInfo == null) return;
        itemState = CatGameManager.Instance.IsBrushUnlocked(brushInfo) ? ItemState.Unlocked : ItemState.Locked;
        
        if (itemState == ItemState.Locked)
        {
            LockItem();
        } else
        {
            UnlockItem();
        }
    }
}