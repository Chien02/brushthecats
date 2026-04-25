using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))] 
public class Brush : MonoBehaviour
{
    [Header ("Brush Data")]
    public BrushInfo brushInfo;
    public AudioSource brushSound;
    public SpriteRenderer spriteRenderer;

    [Header ("Brush Properties")]
    public float moveSpeed = 10f;
    public float rotationAngle = 65f;
    public float animationDuration = 0.5f;
    private bool brushingFlag;
    private Vector3 defaultScale = new(1.5f, 1.5f, 1);

    [Header("Brushing Mechanics (Combo System)")]
    public float brushingRange = 0.1f; 
    public float baseDistanceThreshold = 20f; 
    public float minDistanceThreshold = 5f; 
    public float comboDistanceDecrease = 2f; 

    private float _currentDistanceThreshold;
    private float _accumulatedDownDistance = 0f;
    
    [Header ("Input & Physics")]
    public LayerMask draggableLayer; 
    private bool available = true;
    private bool isBrushing = false;
    private bool isDragging = false;

    // --- BIẾN MỚI CHO CHỨC NĂNG RESET ---
    private Vector3 _startPosition;
    private bool _isReturning = false;

    private Camera mainCamera;

    void Start()
    {
        LoadBrushInfo();
        mainCamera = Camera.main; 
        
        _currentDistanceThreshold = baseDistanceThreshold;
        
        // Lưu lại vị trí ban đầu của chiếc lược khi vừa vào game
        _startPosition = transform.position;
    }

    #region Drag and Drop
    void Update()
    {
        // Thêm điều kiện: Nếu lược đang trong quá trình bay về thì khoá không cho người chơi bấm kéo
        if (!available || Pointer.current == null || _isReturning) return;

        // 1. KHI NHẤN CHUỘT / CHẠM
        if (Pointer.current.press.wasPressedThisFrame)
        {
            Vector2 pointerPosition = Pointer.current.position.ReadValue();
            Vector2 worldPosition = mainCamera.ScreenToWorldPoint(pointerPosition);
            
            Collider2D hitCollider = Physics2D.OverlapPoint(worldPosition, draggableLayer);
            
            if (hitCollider != null && hitCollider.gameObject == this.gameObject)
            {
                isDragging = true;
            }
        }

        // 2. KHI THẢ CHUỘT / NHẤC TAY
        if (Pointer.current.press.wasReleasedThisFrame)
        {
            isDragging = false;
            isBrushing = false;
            brushingFlag = false;
            
            _currentDistanceThreshold = baseDistanceThreshold;
            _accumulatedDownDistance = 0f;
            
            if (brushSound.isPlaying) brushSound.Stop();
            IdleAnimation();
        }

        // 3. CẬP NHẬT VỊ TRÍ
        if (isDragging)
        {
            BrushAnimation();
            FollowPointer();
        }
    }

    #region Brushing Logic
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Chặn không kích hoạt nếu lược đang bay về
        if (_isReturning) return;

        if (collision.gameObject.GetComponent<Cat>() != null)
        {
            _accumulatedDownDistance = 0f;
            _currentDistanceThreshold = baseDistanceThreshold;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // Chặn không kích hoạt nếu lược đang bay về
        if (!isDragging || _isReturning) return; 

        Cat cat = other.GetComponent<Cat>(); 
        
        if (cat != null)
        {
            isBrushing = true;

            Vector2 pointerDelta = Pointer.current.delta.ReadValue();
            bool isMoving = Mathf.Abs(pointerDelta.x) > brushingRange || Mathf.Abs(pointerDelta.y) > brushingRange;
            bool isScoringStroke = false;

            if (isMoving)
            {
                float downDelta = -pointerDelta.y; 
                if (downDelta > 0)
                {
                    _accumulatedDownDistance += downDelta;
                    
                    while (_accumulatedDownDistance >= _currentDistanceThreshold)
                    {
                        isScoringStroke = true;
                        _accumulatedDownDistance -= _currentDistanceThreshold; 
                        _currentDistanceThreshold = Mathf.Max(minDistanceThreshold, _currentDistanceThreshold - comboDistanceDecrease);
                    }
                }
                else
                {
                    _accumulatedDownDistance = 0f; 
                }

                if (!brushSound.isPlaying) brushSound.Play();
            }
            else
            {
                if (brushSound.isPlaying) brushSound.Stop(); 
            }

            cat.HandleBrushing(transform.position, isMoving, isScoringStroke);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Chặn không kích hoạt nếu lược đang bay về
        if (_isReturning) return;

        Cat cat = other.GetComponent<Cat>();
        if (cat != null)
        {
            isBrushing = false;
            _currentDistanceThreshold = baseDistanceThreshold;
            _accumulatedDownDistance = 0f;
            
            cat.ResetBrushingState(); 
            
            if (brushSound.isPlaying) brushSound.Stop();
        }
    }
    #endregion

    private void FollowPointer()
    {
        Vector2 pointerPosScreen = Pointer.current.position.ReadValue();
        Vector3 pointerPosWorld = mainCamera.ScreenToWorldPoint(pointerPosScreen);
        pointerPosWorld.z = transform.position.z; 

        transform.position = Vector3.Lerp(transform.position, pointerPosWorld, moveSpeed * Time.deltaTime);
    }
    #endregion

    #region Return To Start Logic (HÀM CHO NÚT UI)
    // Gắn hàm này vào sự kiện OnClick() của một Button trong Canvas UI
    public void ResetBrushPosition()
    {
        // Chặn spam click nếu lược đang bay về rồi
        if (_isReturning) return;

        // Bật cờ trạng thái
        _isReturning = true;

        // Tắt toàn bộ trạng thái đang kéo (nếu có)
        isDragging = false;
        isBrushing = false;
        brushingFlag = false;
        _accumulatedDownDistance = 0f;
        _currentDistanceThreshold = baseDistanceThreshold;

        // Tắt âm thanh
        if (brushSound.isPlaying) brushSound.Stop();

        // Ép xoay về góc mặc định
        LeanTween.cancel(this.gameObject);
        
        // Dùng LeanTween để bay về mượt mà trong 0.5s. 
        // Sau khi hoàn thành sẽ tắt cờ _isReturning để chơi tiếp.
        LeanTween.move(this.gameObject, _startPosition, 0.5f)
            .setEase(LeanTweenType.easeOutCubic)
            .setOnComplete(() => 
            {
                _isReturning = false;
                IdleAnimation();
            });
    }
    #endregion

    #region Load Info & Animation
    public void LoadBrushInfo(BrushInfo newBrushInfo = null)
    {
        if (newBrushInfo != null) brushInfo = newBrushInfo;
        if (brushInfo != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = brushInfo.brushSprite;
            brushSound.clip = brushInfo.brushSound;
        }
    }

    public void IdleAnimation()
    {   
        if (LeanTween.isTweening(this.gameObject)) LeanTween.cancel(this.gameObject);
        LeanTween.scale(this.gameObject, defaultScale, animationDuration).setEase(LeanTweenType.easeInOutSine);
        LeanTween.rotateLocal(this.gameObject, Vector3.zero, animationDuration).setEase(LeanTweenType.easeInOutSine);
    }

    public void BrushAnimation()
    {
        if (brushingFlag) return;
        brushingFlag = true;

        float scaleFactor = 0.8f;

        if (LeanTween.isTweening(this.gameObject)) LeanTween.cancel(this.gameObject);

        Vector3 targetScale = Vector3.one * scaleFactor;
        Vector3 targetRotation = new Vector3(0, 0, rotationAngle);

        LeanTween.scale(this.gameObject, targetScale, animationDuration).setEase(LeanTweenType.easeInOutSine);
        LeanTween.rotateLocal(this.gameObject, targetRotation, animationDuration).setEase(LeanTweenType.easeInOutSine);
    }
    #endregion
}