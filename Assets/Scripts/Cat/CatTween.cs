using UnityEngine;

public class CatTween
{
    public static void Idle(Cat cat)
    {
        Vector3 originalScale = cat.transform.localScale;
        Vector3 targetScale = originalScale * 1.05f;

        LeanTween.scale(cat.gameObject, targetScale, 1f).setEase(LeanTweenType.easeInOutSine).setLoopPingPong();
    }

    public static void Warning(Cat cat)
    {
        Vector3 originalScale = cat.transform.localScale;
        float targetX = originalScale.x * 0.7f;
        float targetY = originalScale.y * 1.7f;
        Vector3 targetScale = new(targetX, targetY, 0);

        LeanTween.scale(cat.gameObject, targetScale, 0.2f).setEase(LeanTweenType.easeInOutSine).setLoopPingPong(1);
        LeanTween.scale(cat.gameObject, originalScale, 0.2f).setEase(LeanTweenType.easeInOutSine).setDelay(0.2f);
    }

    public static void Looking(Cat cat)
    {
        // Tắt mọi tween đang chạy để tránh xung đột
        LeanTween.cancel(cat.gameObject);
    }

    public static void Angry(Cat cat)
    {   
        float duration1 = 0.1f;
        float duration2 = 1.5f;

        // 1. Scale object lên mức 4f (x=4, y=4, z=4)
        Vector3 targetScale1 = Vector3.one * 6f;
        Vector3 targetScale2 = Vector3.one * 12f;
        LeanTween.scale(cat.gameObject, targetScale1, duration1).setEase(LeanTweenType.easeInOutSine);
        LeanTween.scale(cat.gameObject, targetScale2, duration2).setEase(LeanTweenType.easeInOutSine).setDelay(duration1);

        // 2. Lấy vị trí trung tâm màn hình dựa theo Camera chính
        // Chúng ta lấy x, y của Camera và giữ nguyên trục Z của Mèo để tránh bị khuất sau Camera
        Vector3 centerPosition = Camera.main.transform.position;
        centerPosition.z = cat.transform.position.z;

        // 3. Di chuyển Mèo đến vị trí trung tâm
        // Sau khi hoàn thành tween thì kết thúc trò chơi
        LeanTween.move(cat.gameObject, centerPosition, duration2).setEase(LeanTweenType.easeInOutSine).setOnComplete(() => cat.catGameManager.GameOver());
    }
}
