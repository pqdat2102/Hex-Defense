using UnityEngine;

public class TurretRecoil : MonoBehaviour
{
    [Header("Recoil Settings")]
    [SerializeField] private float recoilDistance = 0.1f; // Khoảng cách giật xuống dưới (theo trục Y)
    [SerializeField] private float recoilSpeed = 5f; // Tốc độ giật xuống
    [SerializeField] private float returnSpeed = 2f; // Tốc độ trở lại vị trí ban đầu
    [SerializeField] private float cooldownAfterRecoil = 0.5f; // Thời gian chờ sau khi giật để bắn tiếp

    private Vector3 originalPosition; // Vị trí ban đầu của Recoil Point
    private bool isRecoiling = false; // Đang trong trạng thái giật
    private bool canRotateAndFire = true; // Cho phép turret quay và bắn

    private void Start()
    {
        // Lưu vị trí ban đầu của Recoil Point
        originalPosition = transform.localPosition;
    }

    // Gọi hàm này khi turret bắn
    public void ApplyRecoil()
    {
        if (canRotateAndFire)
        {
            isRecoiling = true;
            canRotateAndFire = false; // Ngăn turret quay và bắn khi đang giật
            Invoke("ResetCanRotateAndFire", cooldownAfterRecoil); // Đặt lại trạng thái quay và bắn sau cooldown
        }
    }

    private void Update()
    {
        if (isRecoiling)
        {
            // Giật xuống dưới (di chuyển theo trục Y âm, hướng xuống)
            Vector3 recoilTarget = originalPosition - (Vector3.up * recoilDistance); // Giật xuống theo trục Y
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, recoilTarget, recoilSpeed * Time.deltaTime);

            // Nếu đã giật đủ, bắt đầu trở lại vị trí ban đầu
            if (Vector3.Distance(transform.localPosition, recoilTarget) < 0.01f)
            {
                isRecoiling = false;
            }
        }
        else
        {
            // Trở lại vị trí ban đầu
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, originalPosition, returnSpeed * Time.deltaTime);

            // Khi trở về vị trí ban đầu, cho phép turret quay và bắn
            if (Vector3.Distance(transform.localPosition, originalPosition) < 0.01f && !canRotateAndFire)
            {
                canRotateAndFire = true;
            }
        }
    }

    // Đặt lại trạng thái quay và bắn sau thời gian cooldown
    private void ResetCanRotateAndFire()
    {
        canRotateAndFire = true; // Cho phép quay và bắn lại
    }

    // Getter để kiểm tra turret có thể quay và bắn hay không
    public bool CanRotateAndFire()
    {
        return canRotateAndFire;
    }
}