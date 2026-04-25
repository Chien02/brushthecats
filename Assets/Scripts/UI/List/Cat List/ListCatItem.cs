using UnityEngine;

public class ListCatItem : ListItem
{
    public CatProfile catProfile;

    public void LoadProfile(CatProfile profile)
    {
        if (profile == null) return;
        catProfile = profile;
        itemNameText.text = catProfile.catName;
        itemPriceText.text = "Mở khóa: " + catProfile.price.ToString() + " điểm";
        image.sprite = catProfile.spriteAway; // Hiển thị hình mèo ở trạng thái Away làm mặc định
        image.preserveAspect = true;

        // Cập nhật trạng thái nút
        UpdateItemStatus();
    }

    public void OnItemButtonClicked()
    {
        if (catProfile == null) return;

        switch (itemState)
        {
            case ItemState.Unlocked:
                OnUnlockButtonClicked();
                break;
            case ItemState.Available:
                OnChangeCatButtonClicked(); // CatGameManager sẽ tự đổi state: Available -> InUse, và cập nhật lại mèo InUse -> Available
                break;
            case ItemState.InUse:
                Debug.Log("Đây là mèo đang dùng, không thể chọn lại!");
                break;
        }

    }

    public void OnUnlockButtonClicked()
    {
        if (catProfile == null) return;
        AvailableItem();
        SaveCatState(); // Do hàm trên không có gọi lưu trạng thái bên CatGameManager nên cần gọi thêm hàm lưu ở đây
    }

    public void OnChangeCatButtonClicked()
    {
        if (catProfile == null) return;
        if (!CatGameManager.Instance.IsCatUnlocked(catProfile))
        {
            Debug.Log("Mèo này chưa được mở khóa!");
            return;
        }

        // Gọi hàm đổi mèo trong CatGameManager
        CatGameManager.Instance.ChangeCatProfile(catProfile);
        UpdateItemStatus(); // Cập nhật trạng thái của item
        listManager.CloseMenu();
    }

    public void UpdateItemStatus()
    {
        if (catProfile == null) return;
        itemState = CatGameManager.Instance.GetCatState(catProfile);

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

    public void SaveCatState()
    {
        if (catProfile == null) return;
        CatGameManager.Instance.UpdateCatState(catProfile, itemState);
    }

}