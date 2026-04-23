using TMPro;
using Unity.VisualScripting;
using UnityEditor.Searcher;
using UnityEngine;
using UnityEngine.UI;

public class ListItem : MonoBehaviour
{
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemPriceText;
    public Image image;

    public Button itemButton;

    public string unlockButtonText = "Mở";
    public string lockButtonText = "Khóa";
    
    public ItemState itemState = ItemState.Locked;

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
        image.color = Color.white;
        itemButton.GetComponentInChildren<TextMeshProUGUI>().text = unlockButtonText;
        itemButton.interactable = true;
    }
}
