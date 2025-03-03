using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ShopSlot : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("References")]
    [SerializeField] private Image towerIcon; // Hình ảnh trụ trong slot
    [SerializeField] private int towerIndex; // Index của trụ trong BuildManager.towerPrefabs

    [Header("Prefabs")]
    [SerializeField] private GameObject turretDragImagePrefab; // Prefab UI để kéo (chỉ hình ảnh)
    [SerializeField] private RectTransform canvasRectTransform; // Tham chiếu đến RectTransform của Canvas

    [Header("Attributes")]
    [SerializeField] private Color enabledColor = Color.white; // Màu khi đủ vàng
    [SerializeField] private Color disabledColor = Color.gray; // Màu khi không đủ vàng
    [SerializeField] private Color hoverColor = new Color(1f, 1f, 1f, 0.8f); // Màu khi hover

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip dragSound;
    [SerializeField] private AudioClip placeSound;

    private GameObject turretWorldPrefab; // Prefab 2D world để đặt lên plot
    private bool canInteract = true; // Có thể tương tác khi đủ vàng
    private GameObject draggedImage; // Hình ảnh đang được kéo

    private void Start()
    {
        InitializeSlot(); // Khởi tạo slot với thông tin từ BuildManager
        StartCoroutine(WaitForGoldManager());
    }

    private IEnumerator WaitForGoldManager()
    {
        yield return new WaitUntil(() => GoldManager.Instance != null); // Chờ GoldManager sẵn sàng
        CheckGoldState();
    }

    private void CheckGoldState()
    {
        GoldManager goldManager = GoldManager.Instance;
        if (goldManager != null)
        {
            UpdateShopState();
        }
    }

    private void InitializeSlot()
    {
        BuildManager buildManager = BuildManager.main;
        if (buildManager != null && towerIndex < buildManager.GetTowerPrefabs().Length)
        {
            turretWorldPrefab = buildManager.GetTowerPrefabs()[towerIndex];
        }
    }

    private void UpdateShopState()
    {
        GoldManager goldManager = GoldManager.Instance;
        if (goldManager != null)
        {
            // Lấy cost từ Turret để kiểm tra vàng, nhưng không cần hiển thị
            Turret turret = turretWorldPrefab?.GetComponent<Turret>();
            int cost = turret != null ? turret.GetCost() : 0;
            canInteract = goldManager.GetCurrentGold() >= cost;
            towerIcon.color = canInteract ? enabledColor : disabledColor;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!canInteract || turretDragImagePrefab == null) return;

        // Phát âm thanh khi kéo
        if (audioSource != null && dragSound != null)
        {
            audioSource.PlayOneShot(dragSound);
        }

        // Tạo một bản sao của hình ảnh UI để kéo
        draggedImage = Instantiate(turretDragImagePrefab, transform.position, Quaternion.identity, canvasRectTransform);
        CanvasGroup canvasGroup = draggedImage.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = draggedImage.AddComponent<CanvasGroup>();
        }
        canvasGroup.blocksRaycasts = false; // Không chặn raycast để kéo mượt
        draggedImage.GetComponent<RectTransform>().SetAsLastSibling(); // Đưa lên trên cùng
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggedImage == null || !canInteract) return;

        // Di chuyển hình ảnh theo chuột
        RectTransform rectTransform = draggedImage.GetComponent<RectTransform>();
        Vector2 position;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out position
        );
        rectTransform.anchoredPosition = position;

        // Hiển thị tầm bắn khi kéo qua các Plot
        ShowRangePreview(rectTransform.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (draggedImage == null || !canInteract) return;

        // Kiểm tra xem trụ được thả lên Plot nào
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(eventData.position);
        RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero, Mathf.Infinity, LayerMask.GetMask("Plot"));

        if (hit.collider != null)
        {
            Plot plot = hit.collider.GetComponent<Plot>();
            if (plot != null && plot.CanPlaceTurret()) // Kiểm tra cả BoxCollider2D hoạt động
            {
                GoldManager goldManager = GoldManager.Instance;
                if (goldManager != null)
                {
                    Turret turret = turretWorldPrefab?.GetComponent<Turret>();
                    int cost = turret != null ? turret.GetCost() : 0;
                    if (goldManager.SpendGold(cost))
                    {
                        // Gọi PlaceTurret của Plot để đặt trụ và tắt BoxCollider2D
                        plot.PlaceTurret(turretWorldPrefab);
                        Destroy(draggedImage); // Hủy hình ảnh UI
                        draggedImage = null;

                        // Phát âm thanh khi đặt trụ
                        if (audioSource != null && placeSound != null)
                        {
                            audioSource.PlayOneShot(placeSound);
                        }

                        // Tắt vòng tầm đánh của trụ sau khi đặt
                        HideRangeIndicator();
                        return;
                    }
                }
            }
        }

        // Nếu không thả đúng Plot, hủy hình ảnh UI
        Destroy(draggedImage);
        draggedImage = null;
    }

    private void ShowRangePreview(Vector2 position)
    {
        // Chỉ hiển thị tầm bắn khi kéo qua plot
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(position);
        RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero, Mathf.Infinity, LayerMask.GetMask("Plot"));
        if (hit.collider != null)
        {
            Plot plot = hit.collider.GetComponent<Plot>();
            if (plot != null)
            {
                Turret turret = turretWorldPrefab?.GetComponent<Turret>();
                if (turret != null)
                {
                    // Tìm hoặc tạo RangePreview
                    GameObject rangePreview = GameObject.Find("RangePreview");
                    if (rangePreview == null)
                    {
                        rangePreview = new GameObject("RangePreview");
                        LineRenderer lr = rangePreview.AddComponent<LineRenderer>();
                        lr.positionCount = 64;
                        lr.startWidth = 0.1f;
                        lr.endWidth = 0.1f;
                        lr.material = new Material(Shader.Find("Sprites/Default"));
                        lr.startColor = new Color(0, 1, 1, 0.5f); // Xanh nhạt, trong suốt
                        lr.endColor = new Color(0, 1, 1, 0.5f);
                        rangePreview.layer = 5; 
                    }
                    else
                    {
                        LineRenderer lr = rangePreview.GetComponent<LineRenderer>();
                        lr.positionCount = 64; // Reset positionCount
                        lr.enabled = true;
                    }

                    LineRenderer lineRenderer = rangePreview.GetComponent<LineRenderer>();
                    Vector3 center = plot.transform.position;
                    float range = turret.targetingRange;

                    // Tính toán các điểm cho vòng tròn
                    Vector3 firstPosition = Vector3.zero; // Lưu vị trí điểm đầu
                    for (int i = 0; i < 64; i++)
                    {
                        float angle = i * (2f * Mathf.PI) / 64f; // Giữ nguyên công thức hiện tại
                        Vector3 pos = center + new Vector3(Mathf.Cos(angle) * range, Mathf.Sin(angle) * range, 0);
                        if (i == 0)
                        {
                            firstPosition = pos; // Lưu vị trí điểm đầu
                        }
                        lineRenderer.SetPosition(i, pos);
                    }

                    // Đảm bảo điểm cuối (Point 63) khớp với điểm đầu (Point 0) để vòng tròn khép kín
                    lineRenderer.SetPosition(63, firstPosition); // Gán điểm cuối với điểm đầu

                    // Debug để kiểm tra
                    if (Debug.isDebugBuild)
                    {
                        Vector3 pos0 = lineRenderer.GetPosition(0);
                        Vector3 pos63 = lineRenderer.GetPosition(63);
                    }

                    lineRenderer.enabled = true;
                }
            }
        }
        else
        {
            // Ẩn RangePreview nếu không kéo qua Plot
            HideRangeIndicator();
        }
    }

    private void HideRangeIndicator()
    {
        GameObject rangePreview = GameObject.Find("RangePreview");
        if (rangePreview != null)
        {
            LineRenderer lr = rangePreview.GetComponent<LineRenderer>();
            if (lr != null)
            {
                lr.enabled = false;
                lr.positionCount = 0; // Reset để tránh giữ lại điểm cũ
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (canInteract)
        {
            towerIcon.color = hoverColor; // Làm trong suốt một chút khi hover
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UpdateShopState(); // Quay lại màu enabled/disabled
    }

    // Cập nhật trạng thái khi vàng thay đổi
    private void OnEnable()
    {
        GoldManager.onGoldChanged.AddListener((gold) => UpdateShopState());
    }

    private void OnDisable()
    {
        GoldManager.onGoldChanged.RemoveListener((gold) => UpdateShopState());
    }
}