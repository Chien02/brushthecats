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

    // Biến lưu trữ Coroutine để có thể dừng lại bất cứ lúc nào
    private Coroutine behaviorCoroutine;

    [Header ("Cat Properties")]
    public Vector3 defaultScale = new(1.5f, 1.5f, 1.5f);
    public int hairParticleCount = 15;

    [Header("Interaction Settings")]
    public float multiClickWindow = 1.0f; 
    private bool isClickTracking = false; 
    private int clickCount = 0;           
    private float clickTimer = 0f;        
    private Camera mainCamera;            

    public CatState CurrentState
    {
        get { return currentState; }
        set 
        {
            if (currentState == value) return;

            // KIỂM TRA QUAN TRỌNG: Nếu State bị ép sang Angry, dừng ngay lập tức vòng lặp tự động
            if (value == CatState.Angry && behaviorCoroutine != null)
            {
                StopCoroutine(behaviorCoroutine);
                behaviorCoroutine = null; // Đặt lại null để biết là không có coroutine nào đang chạy
            }

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
    public float durationAngry = 0.5f;

    public bool IsBrushing { get; set; } = false;

    void Awake()
    {
        LoadCatProfile(catProfile);
        PlaySFX(catProfile.soundHappy);
    }

    void Start()
    {
        mainCamera = Camera.main; 
        
        // Không start coroutine ở đây nữa. Mặc định mèo ở trạng thái Away (Idle)
        CurrentState = CatState.Away;
        PlayStateSound(CatState.Away);
    }

    void Update()
    {
        if (CurrentState == CatState.Angry) return;
        HandleInput();
    }

    void HandleInput()
    {
        if (isClickTracking)
        {
            clickTimer -= Time.deltaTime;
            
            if (clickTimer <= 0f)
            {
                isClickTracking = false;
                clickCount = 0;
            }
        }

        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
        {
            Vector2 pointerPosScreen = Pointer.current.position.ReadValue();
            Vector2 pointerPosWorld = mainCamera.ScreenToWorldPoint(pointerPosScreen);

            Collider2D hitCollider = Physics2D.OverlapPoint(pointerPosWorld);

            if (hitCollider != null && hitCollider.gameObject == this.gameObject)
            {
                if (!isClickTracking)
                {
                    isClickTracking = true;
                    clickTimer = multiClickWindow; 
                    clickCount = 1;
                }
                else
                {
                    clickCount++;

                    if (clickCount >= 3)
                    {
                        // Việc gán state này sẽ kích hoạt setter ở trên, tự động gọi StopCoroutine
                        CurrentState = CatState.Angry; 
                        Debug.Log("Meow! Tức giận vì bị chọc 3 lần!");

                        isClickTracking = false;
                        clickCount = 0;
                    }
                }
            }
        }
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

        PlaySFX(catProfile.soundHappy);
    }
    #endregion

    IEnumerator CatBehaviorRoutine()
    {
        // Sử dụng do...while để đảm bảo coroutine chạy ít nhất 1 vòng
        do
        {
            CurrentState = CatState.Away;
            yield return new WaitForSeconds(Random.Range(minDurationAway, MaxDurationAway));

            CurrentState = CatState.Warning;
            yield return new WaitForSeconds(durationWarning);

            CurrentState = CatState.Looking;
            yield return new WaitForSeconds(Random.Range(1f, durationLooking));

        } 
        // Kiểm tra sau khi chạy xong 1 vòng, nếu người chơi VẪN ĐANG chải thì lặp lại vòng mới
        while (IsBrushing && CurrentState != CatState.Angry);

        // Nếu chạy xong vòng mà không còn chải nữa (và mèo chưa bị chọc giận), đưa mèo về lại Idle
        if (CurrentState != CatState.Angry)
        {
            CurrentState = CatState.Away;
        }
        
        // Reset biến để lần chải tiếp theo có thể StartCoroutine lại từ đầu
        behaviorCoroutine = null;
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
    public void HandleBrushing(Vector3 brushPosition, bool isMoving, bool isScoringStroke)
    {
        if (CurrentState == CatState.Angry) return;

        if (CurrentState == CatState.Looking)
        {
            if (!isMoving) return;
            
            // Tương tự, nếu chuyển sang Angry ở đây, setter cũng tự động dập tắt Coroutine
            CurrentState = CatState.Angry;
            return;
        }

        Vector3 particlePos = hairParticles.transform.position;
        particlePos.x = brushPosition.x;
        particlePos.y = brushPosition.y;
        hairParticles.transform.position = particlePos;

        if (isMoving)
        {
            if (!IsBrushing)
            {
                IsBrushing = true;
                hairParticles.Emit(hairParticleCount); 

                // Nếu người chơi bắt đầu chải và coroutine chưa chạy -> Kích hoạt vòng lặp trạng thái
                if (behaviorCoroutine == null)
                {
                    behaviorCoroutine = StartCoroutine(CatBehaviorRoutine());
                }
            }

            if (isScoringStroke && catGameManager != null)
            {
                catGameManager.AddScore(); 
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
        // Chú ý: Ta không cần ngắt Coroutine ở đây vì theo yêu cầu của bạn, 
        // Coroutine vẫn phải đợi chạy xong nốt vòng "Looking" hiện tại rồi mới tự kết thúc.
    }
    #endregion

    #region Animation & Sound

    private void UpdateAnimation(CatState state)
    {
        switch (state)
        {
            case CatState.Away: CatTween.Idle(this); break;
            case CatState.Warning: CatTween.Warning(this, durationWarning); break;
            case CatState.Looking: CatTween.Looking(this); break;
            case CatState.Angry: CatTween.Angry(this, durationAngry); break;
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