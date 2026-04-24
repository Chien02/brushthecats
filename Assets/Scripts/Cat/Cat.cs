using System.Collections;
using UnityEngine;

public class Cat : MonoBehaviour
{
    [Header("Cat Components")]
    public CatProfile catProfile;
    public ParticleSystem hairParticles;
    public CatGameManager catGameManager;
    public SpriteRenderer spriteRenderer;
    public AudioSource audioSource;
    public AudioSource audioFXSource;
    private CatState currentState = CatState.Away;

    [Header ("Cat Properties")]
    public Vector3 defaultScale = new(1.5f, 1.5f, 1.5f);
    public int hairParticleCount = 15;

    public CatState CurrentState
    {
        get { return currentState; }
        set 
        {
            if (currentState == value) return;
            currentState = value;
            
            UpdateSprite(value);
            UpdateAnimation(value);
            PlayStateSound(value);
        }
    }

    [Header("Cat State Durations")]
    public float MaxDurationAway = 5f;
    public float minDurationAway = 1f;
    public float durationWarning = 0.5f;
    public float durationLooking = 3f;

    public bool IsBrushing { get; set; } = false;

    void Awake()
    {
        LoadCatProfile(catProfile);
        PlaySFX(catProfile.soundHappy);
    }

    void Start()
    {
        StartCoroutine(CatBehaviorRoutine());
        PlayStateSound(CatState.Away);
    }

    #region Load Data
    public void LoadCatProfile(CatProfile profile)
    {
        if (profile == null) return;
        catProfile = profile;

        ParticleSystemRenderer psr = hairParticles.GetComponent<ParticleSystemRenderer>();
        psr.material = catProfile.hairParticleMaterial;

        ResetAnimation();

        CurrentState = CatState.Away;
        UpdateSprite(CurrentState);
        UpdateAnimation(CurrentState);

        // Chơi âm thanh happy
        PlaySFX(catProfile.soundHappy);
    }
    #endregion

    IEnumerator CatBehaviorRoutine()
    {
        while (CurrentState != CatState.Angry)
        {
            CurrentState = CatState.Away;
            yield return new WaitForSeconds(Random.Range(minDurationAway, MaxDurationAway));

            CurrentState = CatState.Warning;
            yield return new WaitForSeconds(durationWarning);

            CurrentState = CatState.Looking;
            yield return new WaitForSeconds(durationLooking);
        }
    }

    private void UpdateSprite(CatState state)
    {
        if (spriteRenderer == null || catProfile == null) return;

        switch (state)
        {
            case CatState.Away: spriteRenderer.sprite = catProfile.spriteAway; break;
            case CatState.Warning: spriteRenderer.sprite = catProfile.spriteWarning; break;
            case CatState.Looking: spriteRenderer.sprite = catProfile.spriteLooking; break;
            case CatState.Angry: spriteRenderer.sprite = catProfile.spriteAngry; break;
        }
    }

    #region Interaction from Brush
    // Hàm này sẽ được Brush gọi liên tục khi đang đè lên mèo
    public void HandleBrushing(Vector3 brushPosition, bool isMoving, bool isScoringStroke)
    {
        if (CurrentState == CatState.Angry) return;

        // Nếu đang nhìn mà bị chải -> Tức giận
        if (CurrentState == CatState.Looking)
        {
            CurrentState = CatState.Angry;
            return;
        }

        // 1. Dời vị trí Particle chạy theo chuột/lược
        Vector3 particlePos = hairParticles.transform.position;
        particlePos.x = brushPosition.x;
        particlePos.y = brushPosition.y;
        hairParticles.transform.position = particlePos;

        // 2. Xử lý Logic Particle và Score
        if (isMoving)
        {
            if (!IsBrushing)
            {
                IsBrushing = true;
                hairParticles.Emit(hairParticleCount); // Bắn hạt khi bắt đầu vuốt
            }

            if (isScoringStroke && catGameManager != null)
            {
                catGameManager.AddScore(); // Cộng điểm khi đủ 1 nhát chải
            }
        }
        else
        {
            if (IsBrushing)
            {
                IsBrushing = false;
            }
        }
    }

    public void ResetBrushingState()
    {
        IsBrushing = false;
    }
    #endregion

    #region Animation
    private void UpdateAnimation(CatState state)
    {
        switch (state)
        {
            case CatState.Away: CatTween.Idle(this); break;
            case CatState.Warning: CatTween.Warning(this); break;
            case CatState.Looking: CatTween.Looking(this); break;
            case CatState.Angry: CatTween.Angry(this); break;
        }
    }

    void ResetAnimation()
    {
        if (LeanTween.isTweening(this.gameObject))
        {
            LeanTween.cancel(this.gameObject);
        }
        this.transform.localScale = defaultScale;
    }
    #endregion

    #region Sound
    void PlayStateSound(CatState state)
    {
        if (audioSource == null || catProfile == null) return;

        AudioClip clipToPlay = null;

        switch (state)
        {
            case CatState.Away: clipToPlay = catProfile.soundPurring; break;
            case CatState.Warning: clipToPlay = catProfile.soundWarning; break;
            case CatState.Looking: clipToPlay = catProfile.soundLooking; break;
            case CatState.Angry: clipToPlay = catProfile.soundAngry; break;
            case CatState.Happy: clipToPlay = catProfile.soundHappy; break; 
        }

        if (clipToPlay != null)
        {
            if (audioSource.clip != clipToPlay)
            {
                audioSource.clip = clipToPlay;
                audioSource.Play();
            }
            else if (!audioSource.isPlaying)
            {
                audioSource.UnPause();
            }
        } 
        else
        {
            audioSource.Pause();
        }
    }

    void PlaySFX(AudioClip clip)
    {
        if (audioFXSource != null && clip != null)
        {
            audioFXSource.PlayOneShot(clip);
        }
    }
    #endregion
}