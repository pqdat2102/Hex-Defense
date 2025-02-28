using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;

    [Header("Attributes")]
    [SerializeField] private float bulletSpeed = 5f;
    [SerializeField] private int bulletDamage = 1;
    [SerializeField] private float maxLifetime = 5f;

    private Vector2 initialDirection;

    private void Start()
    {
        rb.linearVelocity = initialDirection * bulletSpeed;
        Destroy(gameObject, maxLifetime);
    }

    private void Update()
    {
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

    public void SetDirection(Vector2 direction)
    {
        initialDirection = direction.normalized;
    }

    // Thêm getter để lấy bulletSpeed
    public float GetBulletSpeed()
    {
        return bulletSpeed;
    }
}