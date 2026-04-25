using System.Collections.Generic;
using UnityEngine;

public enum ListType
{
    CatList,
    BrushList,
    ChairList
}

public class ListManager : MonoBehaviour
{
    [Header("List")]
    public GameObject achievList; // Đây là danh sách chính, dùng để nạp dữ liệu vào.
    public GameObject catItemPrefab;
    public GameObject brushItemPrefab;
    public GameObject chairItemPrefab;
    public Transform contentParent;

    public List<CatProfile> catProfiles; // Danh sách tất cả các CatProfile có thể mở khóa
    public List<BrushInfo> brushInfos; // Danh sách tất cả các BrushInfo có thể mở khóa
    public List<ChairInfo> chairInfos; // Danh sách tất cả các ChairInfo có thể mở khóa

    [Header("Position markers")]
    public Transform targetPos;
    public Transform defaultPos;

    public CatProfile GetCatProfile(string id)
    {
        foreach (var profile in catProfiles)
        {
            if (profile.catName != id) continue;
            return profile;
        }
        return null;
    }

    public BrushInfo GetBrushInfo(string id)
    {
        foreach (var profile in brushInfos)
        {
            if (profile.brushName != id) continue;
            return profile;
        }
        return null;
    }

    public ChairInfo GetChairInfo(string id)
    {
        foreach (var profile in chairInfos)
        {
            if (profile.chairName != id) continue;
            return profile;
        }
        return null;
    }

    private void OpenMenu(ListType type)
    {
        // Hiển thị giao diện nếu chưa mở, đồng thời pause game
        if (!this.gameObject.activeSelf)
        {
            this.gameObject.SetActive(true);
            CatGameManager.Instance.TogglePauseGame();
        }

        switch (type)
        {
            case ListType.CatList:
                foreach (var profile in catProfiles)
                {
                    var itemObj = Instantiate(catItemPrefab, contentParent);
                    var item = itemObj != null ? itemObj.GetComponent<ListCatItem>() : null ; // Nếu itemObj là null thì item là null
                    if (item != null)
                    {
                        item.SetListManager(this);
                        item.LoadProfile(profile);
                    }
                }
                break;
            case ListType.BrushList:
                foreach (var profile in brushInfos)
                {
                    var itemObj = Instantiate(brushItemPrefab, contentParent);
                    var item = itemObj != null ? itemObj.GetComponent<ListBrushItem>() : null ; // Nếu itemObj là null thì item là null
                    if (item != null)
                    {
                        item.SetListManager(this);
                        item.LoadProfile(profile);
                    }
                }
                break;
            case ListType.ChairList:
                foreach (var info in chairInfos)
                {
                    var itemObj = Instantiate(chairItemPrefab, contentParent);
                    var item = itemObj != null ? itemObj.GetComponent<ListChairItem>() : null ; // Nếu itemObj là null thì item là null
                    if (item != null)
                    {
                        item.SetListManager(this);
                        item.LoadProfile(info);
                    }
                }
                break;
        }
    }

    private void ClearList()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
    }

    public void OpenCatMenu()
    {
        ClearList();
        OpenMenu(ListType.CatList);
    }

    public void OpenBrushMenu()
    {
        ClearList();
        OpenMenu(ListType.BrushList);
    }

    public void OpenChairMenu()
    {
        ClearList();
        OpenMenu(ListType.ChairList);
    }

    public void CloseMenu()
    {
        ClearList();
        this.gameObject.SetActive(false);
        CatGameManager.Instance.TogglePauseGame();
    }
}
