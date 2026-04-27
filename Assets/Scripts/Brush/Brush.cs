using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

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
    private bool brushingFlag; // For tween animation
    private Vector3 defaultScale = new(1.5f, 1.5f, 1);

    [Header("Brushing Mechanics (Combo System)")]
    public float brushingRange = 0.1f; 
    public float baseDistanceThreshold = 20f; 
    public float minDistanceThreshold = 5f; 
    public float comboDistanceDecrease = 2f; 
    // BO SUNG: Bien buffer time de chong nhieu (flicker)
    public float stopBufferDuration = 0.15f; 
    private float _stopTimer = 0f;
    private bool _smoothedIsMoving = false;

    private float _currentDistanceThreshold;
    private float _accumulatedDistance = 0f;

    
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
            _smoothedIsMoving = false;
            
            _currentDistanceThreshold = baseDistanceThreshold;
            _accumulatedDistance = 0f;
            
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
            _accumulatedDistance = 0f;
            _currentDistanceThreshold = baseDistanceThreshold;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // Chan khong kich hoat neu luoc dang bay ve
        if (_isReturning) return;

        Cat cat = other.GetComponent<Cat>(); 
        
        if (cat == null) return;
        if (!isDragging)
        {
            isBrushing = false;
            _smoothedIsMoving = false; // Reset co di chuyen
            cat.HandleBrushing(transform.position, false, false);
            return;
        }

        Vector2 pointerDelta = Pointer.current.delta.ReadValue();
        // Bien nay la trang thai raw cua frame hien tai
        bool rawIsMoving = isDragging && pointerDelta.magnitude > brushingRange;

        // XU LY LỌC NHIỄU (SMOOTHING)
        if (rawIsMoving)
        {
            _stopTimer = stopBufferDuration; // Reset lai timer neu co di chuyen
            _smoothedIsMoving = true;
        }
        else
        {
            // Neu frame nay khong di chuyen, bat dau tru gio
            if (_stopTimer > 0)
            {
                _stopTimer -= Time.deltaTime;
            }
            else
            {
                // Chi khi het gio dem ma van khong nhuc nhich, moi xac nhan la dung
                _smoothedIsMoving = false;
            }
        }

        bool isScoringStroke = false;

        // Su dung _smoothedIsMoving thay cho isMoving cua ban
        if (_smoothedIsMoving)
        {
            isBrushing = true;
            if (_accumulatedDistance < _currentDistanceThreshold)
            {
                // Cong don cho den khi bang thi reset
                _accumulatedDistance += pointerDelta.magnitude;
                    
                if (_accumulatedDistance >= _currentDistanceThreshold)
                {
                    isScoringStroke = true;
                    _currentDistanceThreshold = Mathf.Max(minDistanceThreshold, _currentDistanceThreshold - comboDistanceDecrease);
                }
            }
            else
            {
                _accumulatedDistance = 0f;
                isScoringStroke = false;
                isBrushing = false;
            }

            if (!brushSound.isPlaying) brushSound.Play();
        }
        else
        {
            if (brushSound.isPlaying) brushSound.Stop();
        }

        // Gui trang thai da duoc lam muot sang cho Cat
        cat.HandleBrushing(transform.position, _smoothedIsMoving, isScoringStroke);
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
            _accumulatedDistance = 0f;
            
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
        _accumulatedDistance = 0f;
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