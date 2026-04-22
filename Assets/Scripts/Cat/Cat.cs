using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

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
    public float minDurationAway = 2f;
    public float durationWarning = 0.5f;
    public float durationLooking = 3f;

    public bool IsBrushing { get; private set; } = false;

    [Header("Brushing Mechanics")]
    public float brushingRange = 0.1f; 
    // Khoang cach keo xuong tich luy de tinh la 1 lan chai (don vi pixel man hinh)
    // Gia tri mac dinh ~100px, co the chinh trong Inspector
    public float strokeDistanceThreshold = 20f; 
    
    // Tich luy khoang cach keo xuong lien tuc (pixel)
    private float _accumulatedDownDistance = 0f;

    void Awake()
    {
        LoadCatProfile(catProfile);
        // Luôn kêu một tiếng khi bắt đầu
        PlaySFX(catProfile.soundHappy);
    }

    void Start()
    {
        StartCoroutine(CatBehaviorRoutine());
        PlayStateSound(CatState.Away);
    }

    void Update()
    {
        if (CurrentState == CatState.Angry) return;
        HandleInput();
    }

    public void LoadCatProfile(CatProfile profile)
    {
        if (profile == null) return;
        catProfile = profile;

        var tsa = hairParticles.textureSheetAnimation;
        tsa.SetSprite(0, catProfile.hairParticleSprite);

        CurrentState = CatState.Away;
        UpdateSprite(CurrentState);
        UpdateAnimation(CurrentState);
    }

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

    void HandleInput()
    {
        if (Mouse.current == null) return;

        Vector2 mousePosScreen = Mouse.current.position.ReadValue();
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(mousePosScreen);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        bool isMousePressed = Mouse.current.leftButton.isPressed;
        bool wasPressedThisFrame = Mouse.current.leftButton.wasPressedThisFrame;
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        
        bool mouseMoving = Mathf.Abs(mouseDelta.x) > brushingRange || Mathf.Abs(mouseDelta.y) > brushingRange;

        // 1. Khi VUA NHAN CHUOT vao meo -> Reset tich luy khoang cach
        if (wasPressedThisFrame && hit.collider != null && hit.collider.gameObject.name == "Cat")
        {
            _accumulatedDownDistance = 0f;
        }

        // 2. Khi DANG GIU CHUOT
        if (isMousePressed && hit.collider != null && hit.collider.gameObject.name == "Cat")
        {
            if (CurrentState == CatState.Looking)
            {
                CurrentState = CatState.Angry; 
                return;
            }

            // TINH NANG 1: Doi vi tri Particle chay theo chuot
            Vector3 particlePos = hairParticles.transform.position;
            particlePos.x = mousePos.x;
            particlePos.y = mousePos.y;
            hairParticles.transform.position = particlePos;

            if (mouseMoving)
            {
                if (!IsBrushing)
                {
                    IsBrushing = true;
                    hairParticles.Play();
                }
                
                // TINH NANG 2: Tich luy khoang cach keo xuong (mouseDelta.y < 0 la xuong)
                float downDelta = -mouseDelta.y; // pixel
                if (downDelta > 0)
                {
                    // Dang keo xuong: cong don vao bộ dem
                    _accumulatedDownDistance += downDelta;

                    // Du 1 "nhat chai" → cong diem, reset, tiep tuc chai
                    while (_accumulatedDownDistance >= strokeDistanceThreshold)
                    {
                        catGameManager.AddScore();
                        _accumulatedDownDistance -= strokeDistanceThreshold;
                    }
                }
                else
                {
                    // Keo nguoc len: reset bo dem, bat dau lai tu dau
                    _accumulatedDownDistance = 0f;
                }
            }
            else
            {
                if (IsBrushing)
                {
                    IsBrushing = false;
                    hairParticles.Stop();
                }
            }
        }
        // 3. Khi THA CHUOT hoac re chuot ra ngoai
        else
        {
            _accumulatedDownDistance = 0f;
            
            if (IsBrushing)
            {
                IsBrushing = false;
                hairParticles.Stop();
            }
        }
    }

    // Handle Sound
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
            else
            {
                if (!audioSource.isPlaying)
                {
                    audioSource.UnPause();
                }
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
}