using UnityEditor;
using UnityEngine;

public class Turret : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform turretRotationPoint; // Điểm xoay của trụ
    [SerializeField] private LayerMask enemyMask; // Layer của quái
    [SerializeField] private GameObject bulletPrefab; // Prefab của trụ
    [SerializeField] private Transform firingPoint; // Điểm bắn đạn - xuất hiện của viên đạn
    [SerializeField] private Transform recoilPoint; // Điểm giật

    [Header("Attributes")]
    [SerializeField] public float targetingRange = 5f; // Tầm bắn của trụ
    [SerializeField] private float rotationSpeed = 200f; // Tốc độ xoay của nòng súng
    [SerializeField] private float bps = 1f; // Bullets per second - số lượng đạn bắn mỗi giây
    [SerializeField] private int cost = 50; // Giá của trụ - dùng để mua trụ

    [Header("Recoil")]
    [SerializeField] private TurretRecoil recoilScript; // Scrips xử lý giật nòng súng

    private Transform target; // mục tiêu của trụ
    private float timeUntilFire; // Thời gian đếm ngược để bắn phát tiếp theo
    private float targetAngle; // Góc mục tiêu mà trụ cần quay đến

    private void Start()
    {
        // Kiểm tra xem component recoil được gắn chưa, nếu chưa thì tự lấy trong recoil Point
        if (recoilScript == null && recoilPoint != null)
        {
            recoilScript = recoilPoint.GetComponent<TurretRecoil>();
        }
    }

    private void Update()
    {
        if (target == null)
        {
            FindTarget();
            return;
        }

        if (recoilScript != null && !recoilScript.CanRotateAndFire())
        {
            return;
        }

        RotateTowardsTarget();

        if (!CheckTargetIsInRange())
        {
            target = null;
        }
        else
        {
            timeUntilFire += Time.deltaTime;

            if (timeUntilFire >= 1f / bps && (recoilScript == null || recoilScript.CanRotateAndFire()))
            {
                // Kiểm tra xem trụ đã quay đúng hướng chưa (thêm ngưỡng sai số, ví dụ 5 độ)
                float currentAngle = turretRotationPoint.eulerAngles.z;
                float angleDifference = Mathf.Abs(Mathf.DeltaAngle(currentAngle, targetAngle));
                if (angleDifference < 5f) // Ngưỡng 5 độ, có thể điều chỉnh
                {
                    Shoot();
                    timeUntilFire = 0f;
                }
            }
        }
    }

    private void FindTarget()
    {
        // Sử dụng OverlapCircleAll để tìm tất cả collider trong tầm bắn
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, targetingRange, enemyMask);

        Transform closestTarget = null;
        float closestDistance = targetingRange;

        foreach (Collider2D hit in hits)
        {
            if (hit.transform != null && hit.gameObject.activeInHierarchy) // Kiểm tra mục tiêu còn tồn tại
            {
                float distance = Vector2.Distance(transform.position, hit.transform.position);
                if (distance < closestDistance)
                {
                    closestTarget = hit.transform;
                    closestDistance = distance;
                }
            }
        }

        target = closestTarget;
    }

    private void OnDrawGizmosSelected()
    {
        // Vẽ vòng tròn tầm bắn trong Scene View với màu đỏ
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, targetingRange);
    }

    private void RotateTowardsTarget()
    {
        if (target == null) return;

        float angle = Mathf.Atan2(target.position.y - transform.position.y, target.position.x - transform.position.x) * Mathf.Rad2Deg - 90f;
        targetAngle = angle; // Lưu góc mục tiêu
        Quaternion targetRotation = Quaternion.Euler(new Vector3(0f, 0f, angle));
        turretRotationPoint.rotation = Quaternion.RotateTowards(turretRotationPoint.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private bool CheckTargetIsInRange()
    {
        if (target == null) return false;
        return Vector2.Distance(target.position, transform.position) <= targetingRange;
    }

    private void Shoot()
    {
        if (firingPoint != null && bulletPrefab != null && (recoilScript == null || recoilScript.CanRotateAndFire()))
        {
            GameObject bulletObj = Instantiate(bulletPrefab, firingPoint.position, Quaternion.identity);
            Bullet bulletScript = bulletObj.GetComponent<Bullet>();

            // Lấy vận tốc từ EnemyMovement
            EnemyMovement enemyMovement = target.GetComponent<EnemyMovement>();
            Vector2 targetVelocity = enemyMovement ? enemyMovement.GetVelocity() : Vector2.zero;

            // Tính vị trí tương lai chính xác của mục tiêu
            Vector2 currentTargetPos = target.position;
            float distance = Vector2.Distance(firingPoint.position, currentTargetPos);
            float bulletSpeed = bulletScript.GetBulletSpeed();
            float timeToHit = distance / bulletSpeed;

            Vector2 predictedPos = currentTargetPos + targetVelocity * timeToHit;
            Vector2 direction = (predictedPos - (Vector2)firingPoint.position).normalized;

            // Truyền hướng và mục tiêu cho đạn
            bulletScript.SetDirection(direction, target);

            // Áp dụng hiệu ứng giật
            if (recoilScript != null)
            {
                recoilScript.ApplyRecoil();
            }
        }
    }

    public int GetCost()
    {
        return cost;
    }
}