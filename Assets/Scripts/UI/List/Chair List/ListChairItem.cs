using UnityEngine;

public class ListChairItem : ListItem
{
    public ChairInfo chairInfo;

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

    public void OnItemButtonClicked()
    {
        if (chairInfo == null) return;

        switch (itemState)
        {
            case ItemState.Unlocked:
                OnUnlockButtonClicked();
                
                break;
            case ItemState.Available:
                OnChangeChairButtonClicked(); 
                break;
            case ItemState.InUse:
                Debug.Log("Đây là ghế đang dùng, không thể chọn lại!");
                break;
        }
    }

    public void OnUnlockButtonClicked()
    {
        if (chairInfo == null) return;
        AvailableItem();
        SaveChairState();
    }

    public void OnChangeChairButtonClicked()
    {
        if (chairInfo == null) return;
        if (!CatGameManager.Instance.IsChairUnlocked(chairInfo))
        {
            Debug.Log("Ghế này chưa được mở khóa!");
            return;
        }

        // Gọi hàm đổi ghế trong CatGameManager
        CatGameManager.Instance.ChangeChairInfo(chairInfo);
        UpdateItemStatus(); // Cập nhật trạng thái của item
        listManager.CloseMenu();
    }

    public void UpdateItemStatus()
    {
        if (chairInfo == null) return;
        itemState = CatGameManager.Instance.GetChairState(chairInfo);

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

    public void SaveChairState()
    {
        if (chairInfo == null) return;
        CatGameManager.Instance.UpdateChairState(chairInfo, itemState);
    }
}