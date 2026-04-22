# brushthecats
Brushing lots of meow, inspired by Brush Jjaemu. This is my solo 2D Game Project using Unity 6.

MÔ TẢ CÁC CHỨC NĂNG CỦA CÁC THÀNH PHẦN TRONG GAME.
Điểm chung của các thực thể:
- Có các thuộc tính: spriteRenderer: Sprite, audio: Audio, currentState.
- Có các phương thức: start(): void, update(): void, change(ScriptableObject): void.

Thành phần mèo
Mèo có các phương thức sau:
1. Tính toán hành động chải: bắt đầu khi người dùng bắt đầu click chuột. Nếu chuột chuyển động và khoảng cách nó di chuyển lớn hơn 2f thì tính là 1 lần chải.
2. Thay đổi vị trí particles tại vị trí chải lược: Mỗi khi nhận ra hành động chải, nó sẽ thay đổi vị trí của particle.

FINITE STATE MACHINE
Các thực thể trong trò chơi có một số trạng thái [Trạng thái + animation (hoặc 0) + audio (hoặc 0)]
- Mèo có 4 trạng thái:
    - Away: Bình thường, bạn có thể thoải mái chải lông cho nó. [animation: idle] [audio: purr]
    - Warning: Cảnh báo, là trạng thái chuyển, để bạn nhận biết mèo đang chuyển sang trạng thái Looking. [animation: giật mình] [audio: giật mình]
    - Looking: Nhìn chằm chằm, không được phép chải trong trạng thái này, nếu chải mèo sẽ chuyển sang Angry. [animation: no] [audio: no]
    - Angry: Tức giận, trò chơi kết thúc. [animation: tức giận] [audio: fahhh!]
- Luồng state của mèo:
    1. Bắt đầu với Away
    2. Sau ngẫu nhiên từ 1 - 5 giây thì chuyển sang Warning
    3. Sau 0.5 giây, từ Warning chuyển sang Looking
    4. Sau ngẫu nhiên từ 1 - 3 giây, chuyển sang Away, nếu trong thời gian này mà chải thì chuyển sang Angry
    5. Ở Angry, gọi trò chơi kết thúc từ GameManager.

- Lược:
    - Idle: Trạng thái bình thường.
            ^
            |
    [animation: rotate_brush] & [animation: rotate_idle]
            |
            V
    - Brushing: Trạng thái đang chải. Sprite ở trạng thái này bị xoay ngược chiều kim đồng hồ 20 độ và thu nhỏ xuống còn 75%.
- Luồng của lược:
    1. Bắt đầu với Idle.
    2. Khi người chơi giữ chuột, chuyển sang Brushing.
    3. Khi người chơi buông chuột, chuyển sang Idle.


