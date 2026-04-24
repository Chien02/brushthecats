using System.Runtime.CompilerServices;
using NUnit.Framework;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.InputSystem;

public class CursorUI : MonoBehaviour
{
    public BrushInfo brushInfo;
    public AudioSource brushSound;
    public bool isBrushing = false;
    private RectTransform _cursorTransform;
    private Canvas _parentCanvas;
    private RectTransform _canvasRectTransform;
    private Camera _canvasCamera;

    [SerializeField] private InputActionReference pointerPositionAction;

    private void Awake()
    {
        _cursorTransform = GetComponent<RectTransform>();
        _parentCanvas = GetComponentInParent<Canvas>();
        
        if (_parentCanvas != null)
        {
            _canvasRectTransform = _parentCanvas.GetComponent<RectTransform>();
            
            // Chỉ cần dùng Camera nếu Canvas không phải là Screen Space - Overlay
            _canvasCamera = _parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay 
                ? null 
                : _parentCanvas.worldCamera;
        }
        LoadBrushInfo();
    }

    private void Update()
    {
        bool isPressing = Mouse.current.leftButton.isPressed;
        if (isPressing && !isBrushing) {
            isBrushing = true;
            BrushAnimation();

            if (brushSound.isPlaying) return;
            brushSound.Play();
        }
        else if (!isPressing && isBrushing)
        {
            isBrushing = false;
            IdleAnimation();
        }
    }

    #region Load Info
    public void LoadBrushInfo(BrushInfo newBrushInfo = null)
    {
        if (newBrushInfo != null) brushInfo = newBrushInfo;
        if (brushInfo != null)
        {
            // Cập nhật sprite của con trỏ chuột
            var image = GetComponentInChildren<UnityEngine.UI.Image>();
            if (image != null)
            {
                image.sprite = brushInfo.brushSprite;
                brushSound.clip = brushInfo.brushSound;
            }
        }
    }
    #endregion

    private void OnEnable()
    {
        pointerPositionAction.action.performed += OnPointerPositionChanged;
        // Cursor.visible = false; // Ẩn con trỏ chuột mặc định của hệ điều hành
    }

    private void OnDisable()
    {
        pointerPositionAction.action.performed -= OnPointerPositionChanged;
        Cursor.visible = true; // Hiện lại con trỏ khi script bị tắt
    }

    private void OnPointerPositionChanged(InputAction.CallbackContext ctx)
    {
        if (_cursorTransform == null || _canvasRectTransform == null) return;

        // Đọc vị trí chuột từ Input System
        var mousePosition = ctx.ReadValue<Vector2>();

        // Chuyển đổi tọa độ màn hình sang tọa độ của Canvas
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRectTransform, 
            mousePosition, 
            _canvasCamera, 
            out var localPoint))
        {
            _cursorTransform.anchoredPosition = localPoint;
        }
    }

    #region Animation
    public void IdleAnimation()
    {
        float duration = 0.5f;
        
        if (LeanTween.isTweening(this.gameObject))
        {
            LeanTween.cancel(this.gameObject);
        }

        LeanTween.scale(this.gameObject, Vector3.one, duration).setEase(LeanTweenType.easeInOutSine);
        LeanTween.rotateLocal(this.gameObject, Vector3.zero, duration).setEase(LeanTweenType.easeInOutSine);
    }

    public void BrushAnimation()
    {
        float scaleFactor = 0.9f;
        float rotationAngle = 20f;
        float duration = 0.5f;

        if (LeanTween.isTweening(this.gameObject))
        {
            LeanTween.cancel(this.gameObject);
        }

        Vector3 targetScale = Vector3.one * scaleFactor;
        Vector3 targetRotation = new Vector3(0, 0, rotationAngle);

        LeanTween.scale(this.gameObject, targetScale, duration).setEase(LeanTweenType.easeInOutSine);
        LeanTween.rotateLocal(this.gameObject, targetRotation, duration).setEase(LeanTweenType.easeInOutSine);
    }
    #endregion
}