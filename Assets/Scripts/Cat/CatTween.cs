using UnityEngine;

public class CatTween
{
    public static void Idle(Cat cat)
    {
        // Phải cancel animation trước đó để tránh xung đột
        LeanTween.cancel(cat.gameObject);
        
        // Trả scale về mặc định trước khi bắt đầu nhịp thở mới
        cat.transform.localScale = cat.defaultScale; 

        Vector3 targetScale = cat.defaultScale * 1.05f;
        LeanTween.scale(cat.gameObject, targetScale, 1f).setEase(LeanTweenType.easeInOutSine).setLoopPingPong();
    }

    public static void Warning(Cat cat, float duration = 0.5f)
    {
        // Hủy Idle đang chạy vô hạn để bắt đầu Warning
        LeanTween.cancel(cat.gameObject);
        cat.transform.localScale = cat.defaultScale;

        float targetX = cat.defaultScale.x * 0.7f;
        float targetY = cat.defaultScale.y * 1.7f;
        Vector3 targetScale = new(targetX, targetY, 0);

        // LƯU Ý LOGIC: setLoopPingPong(1) nghĩa là đi từ A -> B -> A.
        // Thời gian đi A -> B là duration/2. Thời gian về B -> A là duration/2.
        // Tổng thời gian vừa tròn bằng biến 'duration', khớp hoàn hảo với WaitForSeconds bên Cat.cs.
        LeanTween.scale(cat.gameObject, targetScale, duration / 2f).setEase(LeanTweenType.easeInOutSine).setLoopPingPong(1);
    }

    public static void Looking(Cat cat)
    {
        // State này mèo đứng im nhìn chằm chằm nên chỉ cần ngắt Tween và reset scale
        LeanTween.cancel(cat.gameObject);
        cat.transform.localScale = cat.defaultScale;
    }

    public static void Angry(Cat cat, float duration = 0.5f)
    {   
        // 0. ƯU TIÊN TUYỆT ĐỐI: Ngắt ngay lập tức mọi animation đang chạy
        LeanTween.cancel(cat.gameObject);

        // 1. Phóng to mèo (Dùng Tween để scale mượt mà tạo cảm giác lao tới, thay vì giật cục)
        Vector3 targetScale = Vector3.one * 12f;
        LeanTween.scale(cat.gameObject, targetScale, duration).setEase(LeanTweenType.easeInOutSine);

        // 2. Lấy vị trí trung tâm màn hình, giữ nguyên trục Z của mèo
        Vector3 centerPosition = Camera.main.transform.position;
        centerPosition.z = cat.transform.position.z;

        // 3. Di chuyển Mèo đến vị trí trung tâm
        // Khi hành động lao tới (move) hoàn thành -> Kết thúc game ngay lập tức
        LeanTween.move(cat.gameObject, centerPosition, duration)
                 .setEase(LeanTweenType.easeInOutSine)
                 .setOnComplete(() => 
                 {
                     if (cat.catGameManager != null)
                     {
                         cat.catGameManager.GameOver();
                     }
                 });
    }
}