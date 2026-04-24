using UnityEngine;

public class ListBrushItem : ListItem
{
    public BrushInfo brushInfo;

    public void LoadProfile(BrushInfo profile)
    {
        if (profile == null) return;
        brushInfo = profile;
        itemNameText.text = brushInfo.brushName;
        itemPriceText.text = "Mở khóa: " + brushInfo.price.ToString() + " điểm";
        image.sprite = brushInfo.brushSprite; 
        image.preserveAspect = true;

        // Cập nhật trạng thái nút
        UpdateItemStatus();
    }

    public void OnItemButtonClicked()
    {
        if (brushInfo == null) return;

        switch (itemState)
        {
            case ItemState.Unlocked:
                OnUnlockButtonClicked();
                break;
            case ItemState.Available:
                OnChangeBrushButtonClicked(); 
                break;
            case ItemState.InUse:
                Debug.Log("Đây là lược đang dùng, không thể chọn lại!");
                break;
        }
    }

    public void OnUnlockButtonClicked()
    {
        if (brushInfo == null) return;
        AvailableItem();
        SaveBrushState();
    }

    public void OnChangeBrushButtonClicked()
    {
        if (brushInfo == null) return;
        if (!CatGameManager.Instance.IsBrushUnlocked(brushInfo))
        {
            Debug.Log("Lược này chưa được mở khóa!");
            return;
        }

        // Gọi hàm đổi lược trong CatGameManager
        CatGameManager.Instance.ChangeBrushInfo(brushInfo);
        UpdateItemStatus(); // Cập nhật trạng thái của item
        listManager.CloseMenu();
    }

    public void UpdateItemStatus()
    {
        if (brushInfo == null) return;
        itemState = CatGameManager.Instance.GetBrushState(brushInfo);

        switch (itemState)
        {
            case ItemState.Locked:
                LockItem();
                break;
            case ItemState.Unlocked:
                UnlockItem();
                break;
            case ItemState.Available:
                AvailableItem();
                break;
            case ItemState.InUse:
                UseItem();
                break;
        }
    }

    public void SaveBrushState()
    {
        if (brushInfo == null) return;
        CatGameManager.Instance.UpdateBrushState(brushInfo, itemState);
    }
}