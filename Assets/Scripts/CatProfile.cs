using UnityEngine;

[CreateAssetMenu(fileName = "NewCatProfile", menuName = "Game/Cat Profile")]
public class CatProfile : ScriptableObject
{
    public Sprite spriteAway;
    public Sprite spriteWarning;
    public Sprite spriteLooking;
    public Sprite spriteAngry;
    public Sprite hairParticleSprite;
    public string catName; 
    public string nature;
    public int age;

    [Header("Audio per State")]
    public AudioClip soundPurring;
    public AudioClip soundWarning;
    public AudioClip soundLooking;
    public AudioClip soundAngry;
    public AudioClip soundHappy;
}