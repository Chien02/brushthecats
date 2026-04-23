using UnityEngine;

[CreateAssetMenu(fileName = "BrushInfo", menuName = "Game/BrushInfo")]
public class BrushInfo : ScriptableObject
{
    public string brushName;
    public Sprite brushSprite;
    public AudioClip brushSound;
    public int price;
}
