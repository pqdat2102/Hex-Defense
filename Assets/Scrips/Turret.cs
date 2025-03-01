using UnityEditor;
using UnityEngine;

public class Turret : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform turretRotationPoint;
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firingPoint;
    [SerializeField] private Transform recoilPoint;

    [Header("Attributes")]
    [SerializeField] public float targetingRange = 5f;
    [SerializeField] private float rotationSpeed = 200f;
    [SerializeField] private float bps = 1f; // Bullets per second
    [SerializeField] private int cost = 50;

    [Header("Recoil")]
    [SerializeField] private TurretRecoil recoilScript;

    private Transform target;
    private float timeUntilFire;

    private void Start()
    {
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
                Shoot();
                timeUntilFire = 0f;
            }
        }
    }

    private void FindTarget()
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, targetingRange, (Vector2)transform.position, 0f, enemyMask);

        if (hits.Length > 0)
        {
            target = hits[0].transform;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Handles.color = Color.cyan;
        Handles.DrawWireDisc(transform.position, transform.forward, targetingRange);
    }

    private void RotateTowardsTarget()
    {
        float angle = Mathf.Atan2(target.position.y - transform.position.y, target.position.x - transform.position.x) * Mathf.Rad2Deg - 90f;
        Quaternion targetRotation = Quaternion.Euler(new Vector3(0f, 0f, angle));
        turretRotationPoint.rotation = Quaternion.RotateTowards(turretRotationPoint.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private bool CheckTargetIsInRange()
    {
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