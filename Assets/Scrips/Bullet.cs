using UnityEngine;

public class Bullet : MonoBehaviour // Kiểm soát viên đạn khi được bắn ra từ turret 
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb; // Vật lý 2D của viên đạn

    [Header("Attributes")]
    [SerializeField] private float bulletSpeed = 5f; // Tốc độ bay của viên đạn
    [SerializeField] private int bulletDamage = 1; // Sát thương của viên đạn lên quái
   /* [SerializeField] private float maxLifetime = 5f; */// Thời gian tồn tại tối đa của viên đạn trên scene ( ngoài thời gian này viên đạn sẽ bị hủy, tránh tồn tại quá lâu )
    [SerializeField] private float homingStrength = 1f; // Độ mạnh của cơ chế dẫn đường, quyết định đạn chuyển hướng nhanh hay chậm

    private Vector2 initialDirection; // Hướng ban đầu của viên đạn khi được bắn ra
    private Transform target; // Mục tiêu của viên đạn nhắm tới

    private void Start() // Khởi tạo đạn khi viên đạn được tạo ra
    {
        rb.linearVelocity = initialDirection * bulletSpeed; // Đặt vận tốc ban đầu cho đạn dựa trên hướng và tốc độ bay của đạn
        
        // ---------------------- Đã bỏ cơ chế này --------------------------
        /*Destroy(gameObject, maxLifetime); // Hủy viên đạn sau thời gian maxTimeLeft giây nếu viên đạn không va chạm hoặc đi ra khỏi màn hình ( đã bỏ cơ chế này, chuyển sang dùng hủy đạn khi mục tiêu biến mất )*/
    }

    private void Update()
    {
        // Nếu mục tiêu không còn tồn tại (bị tiêu diệt), hủy đạn ngay lập tức
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            Destroy(gameObject);
            return;
        }

        // Nếu có mục tiêu, điều chỉnh hướng bay
        if (target != null)
        {
            Vector2 directionToTarget = (target.position - transform.position).normalized; // Tính hướng viên đạn bay đến mục tiêu
            Vector2 currentDirection = rb.linearVelocity.normalized; // Lấy hướng bay hiện tại của viên đạn

            Vector2 newDirection = Vector2.Lerp(currentDirection, directionToTarget, homingStrength * Time.deltaTime).normalized; // Dùng Lerp để điều chỉnh dần dần hướng viên đạn về target với mức độ điều chỉnh dựa trên homingStrength
            rb.linearVelocity = newDirection * bulletSpeed; // Cập nhật lại vận tốc của viên đạn theo hướng bay mới
        }

        /*// ------------- Đã bỏ cơ chế này --------------------
        // Hủy đạn nếu ra khỏi màn hình
        Vector3 screenPos = Camera.main.WorldToViewportPoint(transform.position); // Chuyển vị trí của đạn sang toạn độ viewport 0, -1
        if (screenPos.x < 0 || screenPos.x > 1 || screenPos.y < 0 || screenPos.y > 1)
        {
            Destroy(gameObject);
        }*/
    }

    private void OnCollisionEnter2D(Collision2D other) // Xử lý va chạm với quái
    {
        EnemyHealth enemy = other.transform.GetComponent<EnemyHealth>(); // Kiểm tra xem đối tượng va chạm có chứa component EnemyHealth không
        if (enemy != null)
        {
            enemy.TakeDamage(bulletDamage); // nếu có thì gọi hàm TakeDamage() trong component EnemyHealth để gây sát thương cho quái theo sát thương của viên đạn
        }
        Destroy(gameObject); // Hủy viên đạn sau khi va chạm
    }

    public void SetDirection(Vector2 direction, Transform targetToTrack) // Dùng để thiết lập hướng bay ban đầu của viên đạn ( được gọi bởi Turret )
    {
        initialDirection = direction.normalized; // Lưu hướng ban đầu của viên đạn
        target = targetToTrack; // Gắn mục tiêu để viên đạn theo dõi
    }

    public float GetBulletSpeed() // getter truy cập tốc độ bay của viên đạn ( dùng trong tính toán dự đoán vị trí của quái trong Turret )
    {
        return bulletSpeed;
    }
}