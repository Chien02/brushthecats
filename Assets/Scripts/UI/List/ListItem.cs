using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ListItem : MonoBehaviour
{
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemPriceText;
    public Image image;

    public Button itemButton;

    public string lockButtonText = "Khóa";
    public string unlockButtonText = "Mở";
    public string useButtonText = "Đổi";
    public string inUseButtonText = "Đang dùng";
    public string unlockPriceText = "Đã mở khóa!";
    
    public ItemState itemState = ItemState.Locked;
    public ListManager listManager;

    public void SetListManager(ListManager manager) { listManager = manager;}

    public void LockItem()
    {
        itemState = ItemState.Locked;
        image.color = Color.black;
        itemButton.GetComponentInChildren<TextMeshProUGUI>().text = lockButtonText;
        itemButton.interactable = false;
    }

    public void UnlockItem()
    {
        itemState = ItemState.Unlocked;
        image.color = Color.black;
        itemPriceText.text = unlockPriceText;
        itemButton.GetComponentInChildren<TextMeshProUGUI>().text = unlockButtonText;
        itemButton.interactable = true;
    }

    public void AvailableItem()
    {
        itemState = ItemState.Available;
        image.color = Color.white;
        itemPriceText.text = unlockPriceText;
        itemButton.GetComponentInChildren<TextMeshProUGUI>().text = useButtonText;
        itemButton.interactable = true;
    }

    public void UseItem()
    {
        itemState = ItemState.InUse;
        image.color = Color.white;
        itemPriceText.text = unlockPriceText;
        itemButton.GetComponentInChildren<TextMeshProUGUI>().text = inUseButtonText;
        itemButton.interactable = true;
    }
}
