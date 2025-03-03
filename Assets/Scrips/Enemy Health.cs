using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Attributes")]
    [SerializeField] private int maxHealth = 10;
    [SerializeField] private int goldValue = 10; // Số vàng nhận được khi tiêu diệt quái

    [Header("References")]
  /*  [SerializeField] private HealthBar healthBar; // Tham chiếu đến HealthBar của quái*/
    [SerializeField] private NumberBar numberBar;
    private int currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;
        if (numberBar != null)
        {
            numberBar.SetHealth(currentHealth, maxHealth); // Khởi tạo thanh máu
        }
        else
        {
            Debug.LogWarning("HealthBar not assigned in " + gameObject.name + "!");
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (numberBar != null)
        {
            numberBar.SetHealth(currentHealth, maxHealth); // Cập nhật thanh máu
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Cộng vàng vào GoldManager
        GoldManager goldManager = GoldManager.Instance;
        if (goldManager != null)
        {
            goldManager.AddGold(goldValue);
        }
        else
        {
            Debug.LogError("GoldManager instance is missing!");
        }

        // Gọi sự kiện để giảm enemiesAlive trong EnemySpawner
        EnemySpawner.onEnemyDestroy.Invoke();

        // Hủy quái
        Destroy(gameObject);
    }

    // Getter để lấy giá trị vàng của quái
    public int GetGoldValue()
    {
        return goldValue;
    }
}