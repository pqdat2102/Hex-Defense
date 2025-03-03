using UnityEngine;

public class Plot : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Color hoverColor;

    private GameObject tower; // Trụ hiện tại trên plot
    private Color startColor;
    private BoxCollider2D plotCollider; // Tham chiếu đến BoxCollider2D của plot

    private bool dattru = true;
    private void Start()
    {
        startColor = sr.color;
        plotCollider = GetComponent<BoxCollider2D>(); // Lấy BoxCollider2D của plot
        // Đảm bảo trạng thái ban đầu đúng
        
    }

    private void OnMouseEnter()
    {
        if (tower == null && plotCollider != null && plotCollider.enabled) // Chỉ đổi màu nếu không có trụ và collider còn hoạt động
        {
            sr.color = hoverColor;
        }
    }

    private void OnMouseExit()
    {
        if (plotCollider != null && plotCollider.enabled) // Chỉ quay lại màu gốc nếu collider còn hoạt động
        {
            sr.color = startColor;
        }
    }

    // Hàm kiểm tra xem có thể đặt trụ không (chỉ cho phép nếu không có trụ và collider hoạt động)
    public bool CanPlaceTurret()
    {
        return dattru;
    }

    // Hàm đặt trụ khi thả từ shop, tắt BoxCollider2D sau khi đặt (giữ sprite hiển thị)
    public void PlaceTurret(GameObject turretPrefab)
    {
        if (CanPlaceTurret())
        {
            tower = Instantiate(turretPrefab, transform.position, Quaternion.identity);
            plotCollider.enabled = false;
            dattru = false;
        }
    }

/*    public void EnableInteractions()
    {
        if (tower != null)
        {
            Destroy(tower);
        }
        tower = null;
        if (plotCollider != null)
        {
            plotCollider.enabled = true; // Kích hoạt lại BoxCollider2D khi mở tương tác
        }
        sr.color = startColor; // Đặt lại màu ban đầu
    }*/
}