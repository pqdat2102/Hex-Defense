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
        Instance = this;
        currentGold = startingGold;
    }

    private void Start()
    {
        /*// Xác nhận currentGold trong Start (không cần gán lại)
        Debug.Log("Gold confirmed in Start: " + currentGold);*/
        UpdateGoldText();
        GetCurrentGold();
    }

    // Cộng vàng (khi tiêu diệt quái)
    public void AddGold(int amount)
    {
        currentGold = Mathf.Max(0, currentGold + amount); // Đảm bảo vàng không âm
        /*Debug.Log("Gold added. New Gold: " + currentGold);*/
        UpdateGoldText();
        onGoldChanged.Invoke(currentGold); // Gọi sự kiện khi vàng thay đổi
    }

    // Trừ vàng (dùng cho mua trụ sau này)
    public bool SpendGold(int amount)
    {
        if (currentGold >= amount)
        {
            currentGold = Mathf.Max(0, currentGold - amount); // Đảm bảo vàng không âm
            UpdateGoldText();
            onGoldChanged.Invoke(currentGold);
            return true;
        }
        /*Debug.Log("Not enough gold! Current Gold: " + currentGold + ", Required: " + amount);*/
        return false;
    }

    // Lấy số vàng hiện tại
    public int GetCurrentGold()
    {
        /*Debug.Log("Getting current gold: " + currentGold);*/
        return currentGold;
    }

    // Cập nhật text hiển thị vàng
    private void UpdateGoldText()
    {
        if (goldText != null)
        {
            goldText.text = currentGold.ToString();
            /*Debug.Log("Updated gold text to: Gold: " + currentGold);*/
        }
        else
        {
           /* Debug.LogWarning("goldText is null in GoldManager!");*/
        }
    }
}