using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NotificationManager : MonoBehaviour
{
    public Image notificationBackground;
    public TextMeshProUGUI notificationText;
    public Transform defaultPosition;
    public Transform targetPosition;
    public float moveDuration = 0.5f;
    public AudioSource notificationSound;
    public FunctionalButtonManager buttonManager;

    public void ShowNotification(string message)
    {
        notificationText.text = message;
        notificationSound.Play();

        if (LeanTween.isTweening(notificationBackground.gameObject))
        {
            LeanTween.cancel(notificationBackground.gameObject);
        }

        LeanTween.move(notificationBackground.gameObject, targetPosition.position, moveDuration).setEase(LeanTweenType.easeOutQuad).setOnComplete(() =>
        {
            // Giữ thông báo hiển thị trong 2 giây trước khi ẩn
            LeanTween.delayedCall(2f, () =>
            {
                HideNotification();
            });
        });
    }

    public void HideNotification()
    {
        if (LeanTween.isTweening(notificationBackground.gameObject))
        {
            LeanTween.cancel(notificationBackground.gameObject);
        }

        LeanTween.move(notificationBackground.gameObject, defaultPosition.position, moveDuration).setEase(LeanTweenType.easeInQuad);
    }
}
