using UnityEngine;
using TMPro; // Thêm namespace này nếu dùng TextMeshPro

public class NumberBar : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI healthText; // Thay Slider bằng TextMeshProUGUI
    public Vector3 offset; // Vị trí offset trên đầu quái

    private void Update()
    {
        // Cập nhật vị trí của text trên đầu quái
        if (Camera.main != null && healthText != null)
        {
            healthText.transform.position = Camera.main.WorldToScreenPoint(transform.parent.position + offset);
        }
        else
        {
            Debug.LogWarning("Camera.main or healthText is null in NumberBar!");
        }
    }

    public void SetHealth(float health, float maxHealth)
    {
        // Cập nhật text với chỉ currentHealth (làm tròn lên số nguyên)
        healthText.text = Mathf.Ceil(health).ToString(); // Chỉ hiển thị số máu hiện tại
    }
}