using UnityEngine;
using UnityEngine.Events;
using TMPro; // Nếu dùng TextMeshProUGUI
// Nếu dùng Text thông thường thì thay bằng: using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance { get; private set; }

    [Header("Attributes")]
    [SerializeField] private int maxHealth = 100; // Máu tối đa của người chơi
    [SerializeField] private int damagePerEnemy = 10; // Số máu bị trừ khi enemy đến điểm cuối

    [Header("UI References")]
    [SerializeField] private GameObject gameOverCanvas; // Canvas hiển thị khi game over
    [SerializeField] private TextMeshProUGUI healthText; // Tham chiếu đến TextMeshProUGUI hiển thị máu
    // Nếu dùng Text thông thường thì thay bằng: [SerializeField] private Text healthText;

    public UnityEvent onGameOver = new UnityEvent(); // Sự kiện khi game over

    private int currentHealth;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        currentHealth = maxHealth;
        if (gameOverCanvas != null)
        {
            gameOverCanvas.SetActive(false); // Ẩn canvas game over lúc đầu
        }
        UpdateHealthText(); // Cập nhật text hiển thị máu ban đầu
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            GameOver();
        }
        UpdateHealthText(); // Cập nhật text khi máu thay đổi
    }

    private void GameOver()
    {
        if (gameOverCanvas != null)
        {
            gameOverCanvas.SetActive(true); // Hiển thị canvas game over
        }
        Time.timeScale = 0f; // Dừng thời gian
        onGameOver.Invoke(); // Gọi sự kiện game over
    }

    // Hàm cập nhật text hiển thị máu
    private void UpdateHealthText()
    {
        if (healthText != null)
        {
            healthText.text = "Health: " + currentHealth + "/" + maxHealth; // Ví dụ: "Health: 80/100"
        }
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }

    public int GetDamagePerEnemy()
    {
        return damagePerEnemy;
    }
}