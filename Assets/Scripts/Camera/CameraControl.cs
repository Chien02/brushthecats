using UnityEngine;

public class CameraControl : MonoBehaviour
{   
    public Transform markerPos;
    public Vector3 defaultPos = new(0, 0, -10);
    void Start()
    {
        MoveCamera(markerPos.position);
    }

    void MoveCamera(Vector3 targetPos)
    {
        if (LeanTween.isTweening(this.gameObject)) LeanTween.cancel(this.gameObject);
        LeanTween.move(this.gameObject, targetPos, 0.5f).setEase(LeanTweenType.easeInOutSine);
    }

    public void ToggleMoveCamera()
    {
        if (LeanTween.isTweening(this.gameObject)) LeanTween.cancel(this.gameObject); // Nếu đang tween, không làm gì
        if (Vector3.Distance(transform.position, markerPos.position) < 0.1f)
        {
            MoveCamera(defaultPos);
        }
        else
        {
            MoveCamera(markerPos.position);
        }
    }
}
