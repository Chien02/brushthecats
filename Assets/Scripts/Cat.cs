using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Cat : MonoBehaviour
{
    public CatProfile catProfile;
    public ParticleSystem hairParticles;
    public CatGameManager catGameManager;
    public SpriteRenderer spriteRenderer;
    public AudioSource audioSource;

    private CatState _currentState = CatState.Away;

    public CatState CurrentState
    {
        get { return _currentState; }
        set 
        {
            if (_currentState == value) return;
            _currentState = value;
            
            UpdateSprite(value);
            PlayStateSound(value);

            if (_currentState == CatState.Angry)
            {
                catGameManager.GameOver(); 
            }
        }
    }

    public bool IsBrushing { get; private set; } = false;

    [Header("Brushing Mechanics")]
    public float brushingRange = 0.1f; 
    // Khoang cach keo xuong de tinh la 1 lan chai (don vi Unity)
    public float strokeDistanceThreshold = 1.0f; 
    
    private Vector2 _startStrokePos;
    private bool _isTrackingStroke = false;

    void Awake()
    {
        LoadCatProfile(catProfile);
        PlayStateSound(CatState.Happy);
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
    }

    IEnumerator CatBehaviorRoutine()
    {
        while (CurrentState != CatState.Angry)
        {
            CurrentState = CatState.Away;
            yield return new WaitForSeconds(Random.Range(2f, 5f));

            CurrentState = CatState.Warning;
            yield return new WaitForSeconds(1f);

            CurrentState = CatState.Looking;
            yield return new WaitForSeconds(Random.Range(1f, 3f));
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

        // 1. Khi VUA NHAN CHUOT vao meo -> Bat dau theo doi 1 nhat chai
        if (wasPressedThisFrame && hit.collider != null && hit.collider.gameObject.name == "Cat")
        {
            _isTrackingStroke = true;
            _startStrokePos = mousePos;
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
                
                // TINH NANG 2: Kiem tra keo chuot huong xuong
                if (_isTrackingStroke)
                {
                    // Tinh toan y bat dau tru di y hien tai (Neu > 0 la dang keo xuong)
                    float downwardDistance = _startStrokePos.y - mousePos.y;

                    // Neu da keo xuong du khoang cach
                    if (downwardDistance >= strokeDistanceThreshold)
                    {
                        catGameManager.AddScore();
                        
                        // Khoa lai de khong cong diem lien tuc, bat buoc phai tha chuot ra
                        _isTrackingStroke = false; 
                    }
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
            _isTrackingStroke = false;
            
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
}