using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class GoldManager : MonoBehaviour
{
    [Header("Attributes")]
    [SerializeField] private int startingGold = 100; // Số vàng ban đầu

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI goldText; // Text hiển thị vàng

    [Header("Events")]
    public static UnityEvent<int> onGoldChanged = new UnityEvent<int>(); // Sự kiện khi vàng thay đổi, truyền giá trị vàng mới

    private int currentGold;

    // Singleton instance
    public static GoldManager Instance { get; private set; }

    private void Awake()
    {
        // Đảm bảo chỉ có một instance của GoldManager
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        DontDestroyOnLoad(gameObject); // Giữ GoldManager qua các scene (nếu cần)
    }

    private void Start()
    {
        currentGold = startingGold;
        UpdateGoldText();
    }

    // Cộng vàng (khi tiêu diệt quái)
    public void AddGold(int amount)
    {
        currentGold += amount;
        UpdateGoldText();
        onGoldChanged.Invoke(currentGold); // Gọi sự kiện khi vàng thay đổi
    }

    // Trừ vàng (dùng cho mua trụ sau này)
    public bool SpendGold(int amount)
    {
        if (currentGold >= amount)
        {
            currentGold -= amount;
            UpdateGoldText();
            onGoldChanged.Invoke(currentGold);
            return true;
        }
        Debug.Log("Not enough gold!");
        return false;
    }

    // Lấy số vàng hiện tại
    public int GetCurrentGold()
    {
        return currentGold;
    }

    // Cập nhật text hiển thị vàng
    private void UpdateGoldText()
    {
        if (goldText != null)
        {
            goldText.text = "Gold: " + currentGold;
        }
    }
}