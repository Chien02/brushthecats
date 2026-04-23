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

CHỨC NĂNG MỞ KHÓA VẬT PHẨM KHI ĐẠT ĐIỂM NHẤT ĐỊNH
Có 2 danh sách mà người chơi có thể mở khóa
1. Danh sách mèo
2. Danh sách lược

Chức năng mà người dùng có thể tương tác với chức năng này
1. Xem danh sách: bấm vào nút bất kì để xem danh sách
2. Mở khóa khi đủ điểm.
3. Nhận thông báo khi đủ điểm để mở khóa.

Thiết kế:
List -> 1 giao diện chiếm toàn trang: trong đó có 3 phần chính
1 Nút để trở lại màn chơi chính.
1 Thanh navigation để chuyển tab sang các danh sách. (Chứa 2 nút: Mèo, Lược)
1 Khu vực để hiển thị danh sách (Scroll Horizontal)
- Cat list
- Brush list

Item bên trong mỗi list:
- Item có 2 trạng thái: locked, unlocked.
    - Trạng thái locked: Image sẽ sẽ bị che bởi một mask màu đen (sillouet), nút unlock hiển thị, còn nút change thì bị ẩn.
    - Trạng thái unlocked: Image không bị che nữa, nút unlock đổi thành change, ẩn nút unlock.
- Item: chứa tên, mức điểm, 2 nút mở khóa/đổi, hình ảnh. Chứa profile để load.
- Chứa phương thức LoadProfile(profile)
- 2 nút chức năng của item:
    - unlock: mở khóa khi đủ, 

List tổng mỗi khi được gọi sẽ kêu list con tương ứng nạp dữ liệu.
List con chạy vòng lặp với số vòng lặp bằng với số lượng profile, mỗi lần lặp thì sinh ra 1 ui mới dựa vào prefab.

*Thông báo khi đủ điểm
Các thành phần của chức năng này:
1. File chứa mức điểm để mở khóa
2. Prefab Thông báo (UI): Image chứa 1 thành phần: text -> "Bạn đã mở khóa một vật phẩm mới"
3. Image tròn đỏ để chỉ dẫn người dùng cần bấm vào nút nào. (Được ẩn/hiện bởi hàm Announce())
2. CatGameManager đảm nhiệm việc theo dõi thông qua hàm TrackPoint(point)

Cách hoạt động:
CatGameManager gọi hàm TrackPoint() bên trong hàm AddPoint(), nếu đạt điều kiện thì gọi hàm Announce()
Hàm Announce() hoạt động như sau:
- Gọi animation cho Prefab thông báo xuất hiện
- Chơi audio "WOW"
- Gọi hàm cho chấm đỏ xuất hiện. Item.VisibleRedDot() (Chấm đỏ sẽ được tắt đi mỗi khi người dùng bấm vào nút đó).

