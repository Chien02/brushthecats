using UnityEngine;

public class Chair : MonoBehaviour
{
    public ChairInfo chairInfo;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LoadChairInfo(chairInfo);
    }

    public void LoadChairInfo(ChairInfo chairInfo)
    {
        var spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && chairInfo != null)
        {
            spriteRenderer.sprite = chairInfo.chairSprite;
        }
    }
}
