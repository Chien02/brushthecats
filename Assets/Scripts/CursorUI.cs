using UnityEngine;
using UnityEngine.InputSystem;

public class CursorUI : MonoBehaviour
{
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
    }

    private void OnEnable()
    {
        pointerPositionAction.action.performed += OnPointerPositionChanged;
        //Cursor.visible = false; // Ẩn con trỏ chuột mặc định của hệ điều hành
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
}