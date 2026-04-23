using UnityEngine;
using TMPro;
using Unity.VisualScripting;

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

    public void OnUnlockButtonClicked()
    {
        if (catProfile == null) return;
        CatGameManager.Instance.UnlockCat(catProfile);
        // Cập nhật UI
        itemPriceText.text = "Đã mở khóa!";
        UpdateItemStatus();
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
    }

    public void UpdateItemStatus()
    {
        if (catProfile == null) return;
        itemState = CatGameManager.Instance.IsCatUnlocked(catProfile) == true ? ItemState.Unlocked : ItemState.Locked;

        if (itemState == ItemState.Locked)
        {
            LockItem();
        } else
        {
            UnlockItem();
        }
    }
}