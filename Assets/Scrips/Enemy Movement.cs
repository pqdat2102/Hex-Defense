using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;

    [Header("Attributes")]
    [SerializeField] private float moveSpeed = 2f;

    private Transform target;
    private int pathIndex = 0;

    private void Start()
    {
        target = LevelManager.main.path[pathIndex];
    }

    private void Update()
    {
        if (Vector2.Distance(target.position, transform.position) <= 0.1f)
        {
            pathIndex++;

            if (pathIndex >= LevelManager.main.path.Length)
            {
                // Trừ máu người chơi khi enemy đến điểm cuối
                PlayerHealth playerHealth = PlayerHealth.Instance;
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(playerHealth.GetDamagePerEnemy());
                }

                EnemySpawner.onEnemyDestroy.Invoke(); // Gọi sự kiện để giảm số enemy còn sống
                Destroy(gameObject); // Hủy enemy
                return;
            }
            else
            {
                target = LevelManager.main.path[pathIndex];
            }
        }
    }

    private void FixedUpdate()
    {
        Vector2 direction = (target.position - transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;
    }

    public Vector2 GetVelocity()
    {
        return rb.linearVelocity;
    }
}