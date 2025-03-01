using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;

    [Header("Attributes")]
    [SerializeField] private float bulletSpeed = 5f;
    [SerializeField] private int bulletDamage = 1;
    [SerializeField] private float maxLifetime = 5f;
    [SerializeField] private float homingStrength = 2f;

    private Vector2 initialDirection;
    private Transform target;

    private void Start()
    {
        rb.linearVelocity = initialDirection * bulletSpeed;
        Destroy(gameObject, maxLifetime);
    }

    private void Update()
    {
        // Nếu mục tiêu không còn tồn tại (bị tiêu diệt), hủy đạn ngay lập tức
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            Destroy(gameObject);
            return;
        }

        // Nếu có mục tiêu, điều chỉnh hướng bay
        if (target != null)
        {
            Vector2 directionToTarget = (target.position - transform.position).normalized;
            Vector2 currentDirection = rb.linearVelocity.normalized;

            Vector2 newDirection = Vector2.Lerp(currentDirection, directionToTarget, homingStrength * Time.deltaTime).normalized;
            rb.linearVelocity = newDirection * bulletSpeed;
        }

        // Hủy đạn nếu ra khỏi màn hình
        Vector3 screenPos = Camera.main.WorldToViewportPoint(transform.position);
        if (screenPos.x < 0 || screenPos.x > 1 || screenPos.y < 0 || screenPos.y > 1)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        EnemyHealth enemy = other.transform.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            enemy.TakeDamage(bulletDamage);
        }
        Destroy(gameObject);
    }

    public void SetDirection(Vector2 direction, Transform targetToTrack)
    {
        initialDirection = direction.normalized;
        target = targetToTrack;
    }

    public float GetBulletSpeed()
    {
        return bulletSpeed;
    }
}